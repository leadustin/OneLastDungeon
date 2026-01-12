using UnityEngine;
using System.Collections.Generic;

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

    public float currentHP;
    public float maxHP;

    public AttackRange attackRange;
    public UnitVisuals Visuals { get; private set; }

    [Header("Loot")]
    public int xpDropAmount;
    public int goldDropAmount;
    public List<DroppedItem> itemDrops = new List<DroppedItem>();

    [Header("Fähigkeiten & Effekte")]
    public List<RuntimeSkill> activeSkills = new List<RuntimeSkill>();
    public List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();

    private StatsHandler statsHandler;
    private UnitUI unitUI;
    private HeroRuntimeData linkedHeroData;

    private void Awake()
    {
        statsHandler = GetComponent<StatsHandler>();
        unitUI = GetComponent<UnitUI>();
        Visuals = GetComponent<UnitVisuals>();
    }

    public void Initialize(bool isPlayer)
    {
        isPlayerTeam = isPlayer;
        foreach (var skill in activeSkills)
        {
            if (skill.template != null)
                skill.currentCooldown = skill.template.initialTurnDelay;
        }
    }

    public void SetupHeroFromRuntime(HeroRuntimeData data)
    {
        linkedHeroData = data;
        unitName = data.heroName;
        currentLevel = data.currentLevel;
        isPlayerTeam = true;

        maxHP = data.GetTotalStat(StatType.MaxHealth);
        currentHP = maxHP;

        activeSkills = data.activeSkills;

        if (statsHandler != null)
        {
            statsHandler.SetBaseStat(StatType.Damage, data.GetTotalStat(StatType.Damage));
            statsHandler.SetBaseStat(StatType.Defense, data.GetTotalStat(StatType.Defense));
        }

        SetupUI();
    }

    public void SetupEnemy(EnemyTemplate template)
    {
        unitName = template.unitName;
        isPlayerTeam = false;
        attackRange = template.attackRange;

        float damage = 0;
        float defense = 0;

        foreach (var stat in template.stats)
        {
            if (stat.type == StatType.MaxHealth) maxHP = stat.value;
            if (stat.type == StatType.Damage) damage = stat.value;
            if (stat.type == StatType.Defense) defense = stat.value;
        }
        currentHP = maxHP;

        if (statsHandler != null)
        {
            statsHandler.SetBaseStat(StatType.Damage, damage);
            statsHandler.SetBaseStat(StatType.Defense, defense);
        }

        activeSkills.Clear();
        foreach (var skillTmpl in template.skills)
        {
            activeSkills.Add(new RuntimeSkill(skillTmpl));
        }

        if (template.icon != null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = template.icon;
        }

        goldDropAmount = Random.Range(template.rewards.minGold, template.rewards.maxGold);
        xpDropAmount = template.rewards.xpReward;

        SetupUI();
    }

    void SetupUI()
    {
        if (unitUI != null)
        {
            unitUI.SetupUI(this);
            unitUI.UpdateHealthBar(currentHP, maxHP, true);
        }
    }

    // --- RUNDEN LOGIK (HIER WAR DER FEHLER) ---

    // ÄNDERUNG: Rückgabetyp ist jetzt 'bool' (true = Stunned)
    public bool ProcessTurnStart()
    {
        bool isStunned = false;

        // Status Effekte abarbeiten
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            // Prüfen ob Betäubt
            if (effect.type == StatusEffectType.Stun || effect.type == StatusEffectType.Freeze)
            {
                isStunned = true;
            }

            // Schaden oder Heilung
            if (effect.type == StatusEffectType.Poison || effect.type == StatusEffectType.Burn)
            {
                TakeDamage(effect.amount, HitType.Normal, effect.type);
            }
            else if (effect.type == StatusEffectType.Regeneration)
            {
                Heal(effect.amount);
            }

            // Dauer reduzieren
            effect.remainingTurns--;
            if (effect.remainingTurns <= 0)
            {
                activeEffects.RemoveAt(i);
            }
        }

        return isStunned;
    }

    public void ReduceCooldowns()
    {
        foreach (var skill in activeSkills)
        {
            skill.TickCooldown();
        }
    }

    // --- KAMPF LOGIK ---

    public bool TakeDamage(float dmg, HitType hitType, StatusEffectType statusSource = StatusEffectType.None, Sprite skillIcon = null)
    {
        if (currentHP <= 0) return true;

        float defense = GetDefenseValue();
        if (hitType != HitType.Critical && statusSource == StatusEffectType.None)
        {
            dmg = Mathf.Max(1, dmg - (defense * 0.5f));
        }

        if (hitType == HitType.Miss)
        {
            if (unitUI != null) unitUI.SpawnDamageText(0, HitType.Miss);
            return false;
        }

        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
            return true;
        }

        if (unitUI != null)
        {
            unitUI.UpdateHealthBar(currentHP, maxHP, false);
            unitUI.SpawnDamageText(dmg, hitType, statusSource, skillIcon);
        }
        return false;
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        if (unitUI != null)
        {
            unitUI.UpdateHealthBar(currentHP, maxHP, false);
            unitUI.SpawnDamageText(amount, HitType.Heal);
        }
    }

    public void ApplyEffect(SkillEffectConfig config)
    {
        activeEffects.Add(new ActiveStatusEffect(config));
    }

    private void Die()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (unitUI != null)
        {
            if (unitUI.gameObject != this.gameObject)
                unitUI.gameObject.SetActive(false);
            else
            {
                unitUI.enabled = false;
                var c = unitUI.GetComponent<Canvas>();
                if (c) c.enabled = false;
            }
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

    public float GetDamageValue() => statsHandler != null ? statsHandler.GetStatValue(StatType.Damage) : 10;
    public float GetDefenseValue() => statsHandler != null ? statsHandler.GetStatValue(StatType.Defense) : 0;
}