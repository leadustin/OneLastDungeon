using System.Collections.Generic;
using UnityEngine;

// WICHTIG: Die Structs (ItemDropConfig, LootConfig, AIConfig) wurden HIER GELÖSCHT.
// Sie stehen jetzt zentral in GameEnums.cs.

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
        // Einfache Vorschau-Berechnung für den Editor
        float dmg = GetStatValue(StatType.Damage);
        float hp = GetStatValue(StatType.MaxHealth);
        float def = GetStatValue(StatType.Defense);

        calc_EffectiveHealth = hp * (1 + (def / 100f)); // Grobe Formel
        calc_DPS = dmg;
    }

    // Hilfsmethode um Stats im Editor auszulesen
    private float GetStatValue(StatType type)
    {
        if (stats == null) return 0;
        foreach (var s in stats)
        {
            if (s.type == type) return s.value;
        }
        return 0;
    }
}