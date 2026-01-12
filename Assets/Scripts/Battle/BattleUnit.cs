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
public class RuntimeSkill
{
    public SkillTemplate template;
    public int currentCooldown;

    public RuntimeSkill(SkillTemplate tmpl)
    {
        template = tmpl;
        currentCooldown = tmpl.initialTurnDelay;
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

    private UnitUI unitUI;
    private StatsHandler statsHandler;

    void Awake()
    {
        Visuals = GetComponent<UnitVisuals>();
        statsHandler = GetComponent<StatsHandler>();
        unitUI = GetComponent<UnitUI>();
    }

    public void Initialize(bool playerTeam)
    {
        isPlayerTeam = playerTeam;
        if (statsHandler != null)
        {
            maxHP = statsHandler.GetStatValue(StatType.MaxHealth);
            currentHP = maxHP;
        }
        foreach (var skill in activeSkills) skill.currentCooldown = skill.template.initialTurnDelay;
        activeEffects.Clear();
    }

    // --- NEU: DIESE METHODE LÄDT ALLES AUTOMATISCH ---
    public void SetupHeroFromRuntime(HeroRuntimeData data)
    {
        if (data == null) return;

        isPlayerTeam = true;
        unitName = data.heroName;
        currentLevel = data.currentLevel;
        gameObject.name = "Hero_" + unitName;
        attackRange = data.heroTemplate.attackRange;

        // Visuals
        if (Visuals != null)
        {
            Visuals.SetData(data.heroTemplate.classIcon, data.heroTemplate.deathAnimationFrames);
            Visuals.FlipOrientation(true);
        }

        // Stats laden (ALLES AUTOMATISCH PER SCHLEIFE!)
        if (statsHandler != null)
        {
            // Wir gehen jeden einzelnen Stat-Typ durch, den es im Spiel gibt
            foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
            {
                // Wir holen den totalen Wert vom Helden (inkl. Items)
                float totalVal = data.GetTotalStat(statType);

                // Wir setzen ihn im Kampf-Handler
                statsHandler.SetBaseStat(statType, totalVal);
            }

            InitializeBattleStats();
        }

        // Skills laden
        activeSkills.Clear();
        if (data.heroTemplate.skills != null)
        {
            foreach (var skillTmpl in data.heroTemplate.skills)
                activeSkills.Add(new RuntimeSkill(skillTmpl));
        }
    }

    // Für Gegner (Nutzt weiterhin das Template)
    public void SetupEnemy(EnemyTemplate enemyTemplate)
    {
        if (enemyTemplate == null) return;

        isPlayerTeam = false;
        unitName = enemyTemplate.unitName;
        gameObject.name = "Enemy_" + unitName;
        attackRange = enemyTemplate.attackRange;

        if (Visuals != null)
        {
            Visuals.SetData(enemyTemplate.icon, enemyTemplate.deathAnimationFrames);
            Visuals.FlipOrientation(false);
        }

        if (statsHandler != null)
        {
            if (enemyTemplate.stats != null)
            {
                foreach (var stat in enemyTemplate.stats) statsHandler.SetBaseStat(stat.type, stat.value);
            }
            InitializeBattleStats();
        }

        // Loot berechnen
        xpDropAmount = enemyTemplate.rewards.xpReward;
        goldDropAmount = Random.Range(enemyTemplate.rewards.minGold, enemyTemplate.rewards.maxGold + 1);

        itemDrops.Clear();
        if (enemyTemplate.rewards.possibleDrops != null)
        {
            foreach (var dropConfig in enemyTemplate.rewards.possibleDrops)
            {
                if (Random.value <= dropConfig.dropChance)
                {
                    int amount = Random.Range(dropConfig.minAmount, dropConfig.maxAmount + 1);
                    if (amount > 0 && dropConfig.item != null)
                    {
                        itemDrops.Add(new DroppedItem { item = dropConfig.item, amount = amount });
                    }
                }
            }
        }

        activeSkills.Clear();
        if (enemyTemplate.skills != null)
        {
            foreach (var skillTmpl in enemyTemplate.skills)
                if (skillTmpl != null) activeSkills.Add(new RuntimeSkill(skillTmpl));
        }
    }

    private void InitializeBattleStats()
    {
        if (statsHandler != null)
        {
            maxHP = statsHandler.GetStatValue(StatType.MaxHealth);
            currentHP = maxHP;
            if (unitUI != null) unitUI.UpdateHealthBar(currentHP, maxHP, true);
        }
    }

    public void ReduceCooldowns()
    {
        foreach (var skill in activeSkills)
            if (skill.currentCooldown > 0) skill.currentCooldown--;
    }

    public bool ProcessTurnStart()
    {
        bool isStunned = false;
        List<ActiveStatusEffect> expiredEffects = new List<ActiveStatusEffect>();

        foreach (var effect in activeEffects)
        {
            switch (effect.type)
            {
                case StatusEffectType.Burn: TakeDamage(effect.amount, HitType.Normal, StatusEffectType.Burn); break;
                case StatusEffectType.Poison: TakeDamage(effect.amount, HitType.Normal, StatusEffectType.Poison); break;
                case StatusEffectType.Regeneration: Heal(effect.amount); break;
                case StatusEffectType.Freeze:
                case StatusEffectType.Stun: isStunned = true; if (unitUI != null) unitUI.SpawnDamageText(0, HitType.Block, StatusEffectType.Stun); break;
            }
            effect.remainingTurns--;
            if (effect.remainingTurns <= 0) expiredEffects.Add(effect);
        }
        foreach (var oldEffect in expiredEffects) activeEffects.Remove(oldEffect);
        return isStunned;
    }

    public void ApplyEffect(SkillEffectConfig config)
    {
        activeEffects.Add(new ActiveStatusEffect(config));
    }

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        if (unitUI != null)
        {
            unitUI.UpdateHealthBar(currentHP, maxHP, false);
            unitUI.SpawnDamageText(amount, HitType.Normal);
        }
    }

    public bool TakeDamage(float dmg, HitType hitType = HitType.Normal, StatusEffectType statusSource = StatusEffectType.None, Sprite skillIcon = null)
    {
        if (hitType == HitType.Miss)
        {
            if (unitUI != null) unitUI.SpawnDamageText(0, HitType.Miss);
            return currentHP <= 0;
        }

        currentHP -= dmg;
        if (currentHP <= 0) { currentHP = 0; Die(); }

        if (unitUI != null)
        {
            unitUI.UpdateHealthBar(currentHP, maxHP, false);
            unitUI.SpawnDamageText(dmg, hitType, statusSource, skillIcon);
        }
        return currentHP <= 0;
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

        // --- LOOT VERGABE ---
        if (!isPlayerTeam && BattlefieldManager.Instance != null)
        {
            BattlefieldManager.Instance.AddLoot(goldDropAmount, xpDropAmount, itemDrops);
        }

        if (Visuals != null) StartCoroutine(PerformDeathSequence());
        else gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator PerformDeathSequence()
    {
        yield return StartCoroutine(Visuals.PlayDeathAnimation());
        gameObject.SetActive(false);
    }

    public float GetDamageValue() => statsHandler != null ? statsHandler.GetStatValue(StatType.Damage) : 0;
    public float GetDefenseValue() => statsHandler != null ? statsHandler.GetStatValue(StatType.Defense) : 0;
}