using UnityEngine;
using System.Collections.Generic;

// --- DATEN STRUKTUREN (Alles hier gesammelt) ---

[System.Serializable]
public class StatConfig
{
    public StatType type;
    public float value;
}

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
    public int xpReward;
    public int minGold;
    public int maxGold;

    [Header("Zufällige Drops")]
    public List<ItemDropConfig> possibleDrops;
}

// --- ENUMS ---

public enum HitType
{
    Normal,
    Critical,
    Miss,
    Blocked,
    Heal
}

public enum TargetPriority
{
    Random,
    LowestHP,
    HighestHP,
    PlayerLeader, // Slot 0
    GlassCannon   // Wenig HP, viel Dmg
}

public enum ItemRarity
{
    Trash,      // Grey
    Common,     // White
    Fine,       // Blue
    Masterwork, // Green
    Rare,       // Yellow
    Exotic,     // Orange
    Legendary   // Purple
}

public enum ItemType
{
    Weapon,
    Armor,
    Jewelry,
    Consumable,
    Material,
    Quest
}

public enum EquipmentSlot
{
    None,
    Head,
    Shoulders,
    Chest,
    Bracers,
    Gloves,
    Belt,
    Legs,
    Feet,
    MainHand,
    OffHand,
    Necklace,
    Ring
}

public enum UnitRank
{
    Normal,
    Elite,
    Boss
}

public enum UnitRole
{
    Tank,
    GlassCannon,
    Support,
    Debuffer,
    Balanced
}

public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Lightning,
    Poison,
    Arcane,
    Holy,
    Shadow
}

public enum StatType
{
    MaxHealth,
    MaxMana,
    Damage,
    Defense,
    Speed,
    CritChance,
    CritDamage,
    DodgeChance,
    Accuracy,
    BlockChance
}

public enum StatModType
{
    Flat = 100,
    PercentAdd = 200,
    PercentMult = 300
}

public enum AttackRange
{
    Melee,
    Ranged
}

public enum StatusEffectType
{
    [InspectorName("Keiner")] None,
    [InspectorName("Verbrennen (Feuer DoT)")] Burn,
    [InspectorName("Vergiftung (Gift DoT)")] Poison,
    [InspectorName("Einfrieren (Stun)")] Freeze,
    [InspectorName("Betäubung (Physisch Stun)")] Stun,
    [InspectorName("Regeneration (Heilung)")] Regeneration,
    [InspectorName("Schwäche (Weniger Dmg)")] Weakness
}

public enum GridPosition
{
    [InspectorName("Front - Oben (Slot 0)")] Front_Top = 0,
    [InspectorName("Front - Mitte (Slot 1)")] Front_Center = 1,
    [InspectorName("Front - Unten (Slot 2)")] Front_Bottom = 2,

    [InspectorName("Mitte - Oben (Slot 3)")] Mid_Top = 3,
    [InspectorName("Mitte - Mitte (Slot 4)")] Mid_Center = 4,
    [InspectorName("Mitte - Unten (Slot 5)")] Mid_Bottom = 5,

    [InspectorName("Hinten - Oben (Slot 6)")] Back_Top = 6,
    [InspectorName("Hinten - Mitte (Slot 7)")] Back_Center = 7,
    [InspectorName("Hinten - Unten (Slot 8)")] Back_Bottom = 8,

    None = 99
}

public enum SkillTargetMode
{
    [InspectorName("Einzelner Gegner")] SingleEnemy,
    [InspectorName("Alle Gegner")] AllEnemies,
    [InspectorName("Selbst")] Self,
    [InspectorName("Einzelner Verbündeter")] Ally,
    [InspectorName("Zufälliger Gegner")] RandomEnemy
}