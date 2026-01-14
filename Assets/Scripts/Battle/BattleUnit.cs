using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// --- HILFSKLASSEN ---
[System.Serializable]
public class ActiveStatusEffect
{
    public StatusEffectType type;
    public int remainingTurns;
    public float amount;

    public ActiveStatusEffect(SkillEffectConfig config)
    {
        type = config.type;
        remainingTurns = config.duration;
        amount = config.amount;
    }
}

[System.Serializable]
public class DroppedItem
{
    public ItemTemplate item;
    public int amount;
}

// --- HAUPT KLASSE ---
[RequireComponent(typeof(StatsHandler))]
public class BattleUnit : MonoBehaviour
{
    [Header("Live Daten")]
    public string unitName;
    public bool isPlayerTeam;
    public int currentLevel = 1;

    [Header("Identifikation (Neu)")]
    // -1 = Einzigartig, 0-4 = Hat einen Form-Marker (Kreis, Viereck...)
    public int markerIndex = -1;

    public float currentHP;
    public float maxHP;

    // --- ATB & LIMIT SYSTEM ---
    [Header("ATB System")]
    public float currentATB = 0f;
    public float maxATB = 100f;
    public float speed = 10f;

    [Header("Limit Break")]
    public float limitGauge = 0f;
    public float maxLimit = 100f;
    public bool isLimitReady => limitGauge >= maxLimit;

    public AttackRange attackRange;
    public UnitVisuals Visuals { get; private set; }

    [Header("Visuals Configuration")]
    [SerializeField] private SpriteRenderer iconRenderer;

    [Header("Loot")]
    public int xpDropAmount;
    public int goldDropAmount;
    public List<DroppedItem> itemDrops = new List<DroppedItem>();

    [Header("Fähigkeiten & Effekte")]
    public List<RuntimeSkill> activeSkills = new List<RuntimeSkill>();
    public List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

    private StatsHandler statsHandler;
    private UnitUI unitUI;

    private void Awake()
    {
        statsHandler = GetComponent<StatsHandler>();
        unitUI = GetComponent<UnitUI>();
        Visuals = GetComponent<UnitVisuals>();
    }

    public void Initialize(bool isPlayer)
    {
        isPlayerTeam = isPlayer;
        currentATB = Random.Range(0, 20);
        limitGauge = 0;
        markerIndex = -1; // Reset

        foreach (var skill in activeSkills)
        {
            if (skill.template != null)
                skill.currentCooldown = skill.template.initialTurnDelay;
        }
        UpdateStatsCache();
    }

    public void UpdateStatsCache()
    {
        maxHP = GetRawStat(StatType.MaxHealth);
        speed = Mathf.Max(1f, GetRawStat(StatType.Speed));
    }

    public void SetupHeroFromRuntime(HeroRuntimeData data)
    {
        unitName = data.heroName;
        currentLevel = data.currentLevel;
        isPlayerTeam = true;

        if (statsHandler != null)
        {
            statsHandler.SetBaseStat(StatType.MaxHealth, data.GetTotalStat(StatType.MaxHealth));
            statsHandler.SetBaseStat(StatType.PhysicalDamage, data.GetTotalStat(StatType.PhysicalDamage));
            statsHandler.SetBaseStat(StatType.Armor, data.GetTotalStat(StatType.Armor));
            statsHandler.SetBaseStat(StatType.Speed, data.GetTotalStat(StatType.Speed));
        }

        activeSkills = data.activeSkills;

        if (data.heroTemplate != null && data.heroTemplate.classIcon != null)
        {
            var sr = GetIconRenderer();
            if (sr != null)
            {
                sr.sprite = data.heroTemplate.classIcon;
                sr.color = Color.white;
            }
        }

        UpdateStatsCache();
        currentHP = maxHP;
        SetupUI();
    }

    public void SetupEnemy(EnemyTemplate template)
    {
        unitName = template.unitName;
        isPlayerTeam = false;
        attackRange = template.attackRange;
        currentLevel = 1;

        if (statsHandler != null)
        {
            foreach (var stat in template.stats)
                statsHandler.SetBaseStat(stat.type, stat.value);
        }

        activeSkills.Clear();
        if (template.skills != null)
        {
            foreach (var s in template.skills)
                if (s != null) activeSkills.Add(new RuntimeSkill(s));
        }

        if (template.icon != null)
        {
            var sr = GetIconRenderer();
            if (sr != null) { sr.sprite = template.icon; sr.color = Color.white; }
        }

        if (template.rewards.possibleDrops != null)
        {
            goldDropAmount = Random.Range(template.rewards.minGold, template.rewards.maxGold);
            xpDropAmount = template.rewards.xpReward;
        }

        UpdateStatsCache();
        currentHP = maxHP;
        SetupUI();
    }

    // --- VISUAL HELPER ---

    private SpriteRenderer GetIconRenderer()
    {
        if (iconRenderer != null) return iconRenderer;
        var children = GetComponentsInChildren<Transform>(true);
        foreach (var t in children)
        {
            if (t.name == "Character_Icon")
            {
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr != null) { iconRenderer = sr; return sr; }
            }
        }
        return GetComponentInChildren<SpriteRenderer>();
    }

    // --- DIESE METHODE HAT GEFEHLT (Fix für CS1061) ---
    public Sprite GetIcon()
    {
        // 1. Haben wir ein manuell zugewiesenes Icon (Priorität)?
        if (iconRenderer != null) return iconRenderer.sprite;

        // 2. Fallback: Visuals Renderer (Das Sprite, das auf dem Feld rumläuft)
        if (Visuals != null && Visuals.unitRenderer != null) return Visuals.unitRenderer.sprite;

        return null;
    }
    // --------------------------------------------------

    void SetupUI()
    {
        if (unitUI != null)
        {
            unitUI.SetupUI(this);
            unitUI.UpdateHealthBar(currentHP, maxHP, true);
        }
    }

    // --- TURN LOGIC (ATB) ---

    public void TickATB(float deltaTime)
    {
        if (currentHP <= 0) { currentATB = 0; return; }

        if (currentATB < maxATB)
        {
            currentATB += speed * deltaTime;
            if (currentATB > maxATB) currentATB = maxATB;
        }
    }

    public void ResetATB()
    {
        currentATB = 0;
    }

    public void OnTurnStart()
    {
        ReduceCooldowns();
        ProcessStatusEffects();
    }

    private void ProcessStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            if (effect.type == StatusEffectType.Poison || effect.type == StatusEffectType.Burn)
            {
                TakeDamageDirect(effect.amount, HitType.Normal, effect.type);
            }
            else if (effect.type == StatusEffectType.Regeneration)
            {
                Heal(effect.amount);
            }

            effect.remainingTurns--;
            if (effect.remainingTurns <= 0) activeEffects.RemoveAt(i);
        }
    }

    public void ReduceCooldowns()
    {
        foreach (var skill in activeSkills) skill.TickCooldown();
    }

    // --- KAMPF ---

    public bool TakeDamage(float rawDmg, HitType hitType, int attackerLevel, DamageType damageType = DamageType.Physical)
    {
        if (currentHP <= 0) return true;

        StatType resistStat = CombatMath.GetResistanceStat(damageType);
        float myResistValue = GetRawStat(resistStat);
        float finalDmg = CombatMath.CalculateFinalDamage(rawDmg, damageType, myResistValue, attackerLevel);

        if (hitType == HitType.Critical) finalDmg *= 1.5f;

        float pctLost = finalDmg / maxHP;
        limitGauge += pctLost * 100f * 0.5f;
        if (limitGauge > maxLimit) limitGauge = maxLimit;

        return TakeDamageDirect(finalDmg, hitType);
    }

    private bool TakeDamageDirect(float finalDmg, HitType hitType, StatusEffectType source = StatusEffectType.None)
    {
        if (hitType == HitType.Miss)
        {
            if (unitUI != null) unitUI.SpawnDamageText(0, HitType.Miss);
            return false;
        }

        currentHP -= finalDmg;
        if (currentHP <= 0) { currentHP = 0; Die(); return true; }

        if (unitUI != null)
        {
            unitUI.UpdateHealthBar(currentHP, maxHP, false);
            unitUI.SpawnDamageText(finalDmg, hitType, source);
        }
        return false;
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        if (unitUI != null) { unitUI.UpdateHealthBar(currentHP, maxHP, false); unitUI.SpawnDamageText(amount, HitType.Heal); }
    }

    public void ApplyEffect(SkillEffectConfig config) => activeEffects.Add(new ActiveStatusEffect(config));

    private void Die()
    {
        currentATB = 0;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (unitUI != null)
        {
            if (unitUI.gameObject != this.gameObject) unitUI.gameObject.SetActive(false);
            else { unitUI.enabled = false; var c = unitUI.GetComponent<Canvas>(); if (c) c.enabled = false; }
        }

        if (!isPlayerTeam && BattlefieldManager.Instance != null)
        {
            BattlefieldManager.Instance.AddLoot(goldDropAmount, xpDropAmount, itemDrops);
        }

        if (Visuals != null) StartCoroutine(PerformDeathSequence());
        else gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator PerformDeathSequence()
    {
        if (Visuals != null) yield return StartCoroutine(Visuals.PlayDeathAnimation());
        gameObject.SetActive(false);
    }

    public float GetRawStat(StatType type) => statsHandler != null ? statsHandler.GetStatValue(type) : 0;
    public float GetDamageValue() => GetRawStat(StatType.PhysicalDamage);
}