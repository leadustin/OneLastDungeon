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
        unitName = data.heroName;
        currentLevel = data.currentLevel;
        isPlayerTeam = true;

        // RAW STATS laden
        if (statsHandler != null)
        {
            statsHandler.SetBaseStat(StatType.MaxHealth, data.GetTotalStat(StatType.MaxHealth));
            statsHandler.SetBaseStat(StatType.PhysicalDamage, data.GetTotalStat(StatType.PhysicalDamage));
            // FIX: Defense -> Armor
            statsHandler.SetBaseStat(StatType.Armor, data.GetTotalStat(StatType.Armor));

            // Weitere Stats (Elementar Resistenzen etc. falls im RuntimeData vorhanden) laden...
        }

        // HP voll machen
        maxHP = GetRawStat(StatType.MaxHealth);
        currentHP = maxHP;

        activeSkills = data.activeSkills;
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
            {
                statsHandler.SetBaseStat(stat.type, stat.value);
            }
        }

        maxHP = GetRawStat(StatType.MaxHealth);
        currentHP = maxHP;

        activeSkills.Clear();

        // --- FIX ANFANG ---
        // 1. Prüfen, ob die Liste überhaupt existiert
        if (template.skills != null)
        {
            foreach (var skillTmpl in template.skills)
            {
                // 2. WICHTIG: Prüfen, ob der Skill-Slot leer ist (None)
                if (skillTmpl != null)
                {
                    activeSkills.Add(new RuntimeSkill(skillTmpl));
                }
                else
                {
                    // Optional: Warnung ausgeben, damit du den Fehler im Inspector findest
                    Debug.LogWarning($"Gegner '{unitName}' hat einen leeren Skill-Slot! Bitte im Inspector fixen.");
                }
            }
        }
        // --- FIX ENDE ---

        if (template.icon != null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = template.icon;
        }

        // Safety Check für Rewards, falls diese im Template vergessen wurden
        if (template.rewards.possibleDrops != null) // Nur als Vorsichtsmaßnahme
        {
            goldDropAmount = Random.Range(template.rewards.minGold, template.rewards.maxGold);
            xpDropAmount = template.rewards.xpReward;
        }

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

    // --- TURN LOGIC ---

    public bool ProcessTurnStart()
    {
        bool isStunned = false;
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            if (effect.type == StatusEffectType.Stun || effect.type == StatusEffectType.Freeze) isStunned = true;

            if (effect.type == StatusEffectType.Poison || effect.type == StatusEffectType.Burn)
            {
                // DoTs ignorieren oft Rüstung -> True Damage oder Magic Damage
                // Wir nutzen hier Physical als Placeholder oder einen spezifischen DoT Typ
                TakeDamageDirect(effect.amount, HitType.Normal, effect.type);
            }
            else if (effect.type == StatusEffectType.Regeneration)
            {
                Heal(effect.amount);
            }

            effect.remainingTurns--;
            if (effect.remainingTurns <= 0) activeEffects.RemoveAt(i);
        }
        return isStunned;
    }

    public void ReduceCooldowns()
    {
        foreach (var skill in activeSkills) skill.TickCooldown();
    }

    // --- KAMPF & CALCULATIONS ---

    // Aktualisierte TakeDamage Methode mit DamageType Support
    public bool TakeDamage(float rawDmg, HitType hitType, int attackerLevel, DamageType damageType = DamageType.Physical)
    {
        if (currentHP <= 0) return true;

        // 1. Welche Resistenz brauchen wir? (Armor, FireResist, etc.)
        StatType resistStat = CombatMath.GetResistanceStat(damageType);

        // 2. Wie hoch ist der Wert dieser Resistenz bei mir?
        float myResistValue = GetRawStat(resistStat);

        // 3. Berechnung via Unified CombatMath
        float finalDmg = CombatMath.CalculateFinalDamage(rawDmg, damageType, myResistValue, attackerLevel);

        if (hitType == HitType.Critical) finalDmg *= 1.5f;

        return TakeDamageDirect(finalDmg, hitType);
    }

    // Für True Damage oder interne Abzüge
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

    // --- GETTER (Raw Stats) ---
    public float GetRawStat(StatType type) => statsHandler != null ? statsHandler.GetStatValue(type) : 0;

    public float GetDamageValue() => GetRawStat(StatType.PhysicalDamage);
}