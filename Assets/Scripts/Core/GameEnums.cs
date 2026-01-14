using UnityEngine;
using System.Collections.Generic;

// --- DATEN STRUKTUREN ---

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
    [Range(0f, 1f)] public float dropChance;
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
    PlayerLeader,
    GlassCannon
}

public enum ItemRarity
{
    Trash, Common, Fine, Masterwork, Rare, Exotic, Legendary
}

public enum ItemType
{
    Weapon, Armor, Jewelry, Consumable, Material, Quest
}

public enum EquipmentSlot
{
    None, Head, Shoulders, Chest, Bracers, Gloves, Belt, Legs, Feet, MainHand, OffHand, Necklace, Ring
}

public enum UnitRank { Normal, Elite, Boss }

public enum UnitRole { Tank, GlassCannon, Support, Debuffer, Balanced }

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
    // --- BASIS ---
    MaxHealth,
    MaxMana,

    // --- OFFENSIV (PHYSISCH) ---
    PhysicalDamage,
    CritChance,
    CritDamage,
    Speed,
    Accuracy,

    // --- OFFENSIV (ELEMENTAR) ---
    FireDamage,
    IceDamage,
    LightningDamage,
    PoisonDamage,
    ArcaneDamage,
    HolyDamage,
    ShadowDamage,

    // --- DEFENSIV ---
    Armor,
    FireResist,
    IceResist,
    LightningResist,
    PoisonResist,
    ArcaneResist,
    HolyResist,
    ShadowResist,

    // --- SEKUNDÄR ---
    DodgeChance,
    BlockChance,
    StatusResist
}

public enum StatModType { Flat = 100, PercentAdd = 200, PercentMult = 300 }

public enum AttackRange { Melee, Ranged }

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
    Front_Top = 0, Front_Center = 1, Front_Bottom = 2,
    Mid_Top = 3, Mid_Center = 4, Mid_Bottom = 5,
    Back_Top = 6, Back_Center = 7, Back_Bottom = 8,
    None = 99
}

// --- HIER WAR DER FEHLER ---
public enum SkillTargetMode
{
    SingleEnemy,
    AllEnemies,
    Self,
    SingleAlly, // War vorher 'Ally'
    AllAllies,  // Fehlte komplett
    RandomEnemy
}