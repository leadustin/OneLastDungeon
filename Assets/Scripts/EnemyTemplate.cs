using System.Collections.Generic;
using UnityEngine;

// --- LOOT STRUKTUREN ---
[System.Serializable]
public struct ItemDropConfig
{
    public ItemTemplate item;
    [Range(0f, 1f)] public float dropChance; // 0.3 = 30%
    public int minAmount;
    public int maxAmount;
}

[System.Serializable]
public struct LootConfig
{
    [Header("Garantierte Belohnung")]
    public int xpReward;        // Fixer Wert
    public int minGold;         // Von...
    public int maxGold;         // ...bis

    [Header("Zufällige Drops")]
    public List<ItemDropConfig> possibleDrops;
}

// --- ANDERE HELFER ---
[System.Serializable]
public struct AIConfig
{
    [Range(0f, 1f)] public float aggression;
    public float skillFrequency;
    public TargetPriority targetPreference;
    [Header("Enrage")]
    public bool hasEnrage;
    [Range(0, 100)] public float enrageAtHPPercent;
}

// --- HAUPT KLASSE ---
[CreateAssetMenu(fileName = "New Enemy", menuName = "RPG/Enemy Template")]
public class EnemyTemplate : ScriptableObject
{
    [Header("1. Identität")]
    public string enemyID;
    public string unitName;
    public Sprite icon;

    [Tooltip("Markiere alle gesliceten Sprites im Projekt und ziehe sie hier rein.")]
    public Sprite[] deathAnimationFrames;

    public UnitRank rank;
    public UnitRole role;
    public AttackRange attackRange;

    [Header("2-4. Stats Konfiguration")]
    public List<StatConfig> stats;

    [Header("6. AI Verhalten")]
    public AIConfig aiBehavior;

    [Header("7. Skills")]
    public List<SkillTemplate> skills;

    [Header("9. Rewards")]
    public LootConfig rewards;

    [Header("10. Design Zusammenfassung")]
    [TextArea(5, 10)]
    public string designSummary;

    // --- Live Analyse (Auto-Calc) ---
#pragma warning disable 0414
    [SerializeField] private float calc_DPS;
    [SerializeField] private float calc_EffectiveHealth;
#pragma warning restore 0414

    private void OnValidate()
    {
        CalculateDerivedStats();
    }

    private void CalculateDerivedStats()
    {
        float dmg = GetStatValue(StatType.Damage);
        float hp = GetStatValue(StatType.MaxHealth);
        float def = GetStatValue(StatType.Defense);
        calc_EffectiveHealth = hp * (1 + (def / 100f));
        calc_DPS = dmg;
    }

    private float GetStatValue(StatType type)
    {
        if (stats == null) return 0;
        foreach (var s in stats) if (s.type == type) return s.value;
        return 0;
    }
}