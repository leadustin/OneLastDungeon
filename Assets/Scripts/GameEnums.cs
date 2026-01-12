using UnityEngine;

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
    None,           // Für Items ohne Slot (Tränke etc.)
    Head,           // Kopf
    Shoulders,      // Schulter
    Chest,          // Brust
    Bracers,        // Armschiene
    Gloves,         // Handschuhe
    Belt,           // Gürtel
    Legs,           // Beine
    Feet,           // Füße
    MainHand,       // Waffe
    OffHand,        // Schild / Buch
    Necklace,       // Halskette
    Ring            // Ring
}

// 1. Identität & Rolle
public enum UnitRank
{
    Normal,
    Elite,
    Boss
}

public enum UnitRole
{
    Tank,           // Viel HP/Def, zieht Aggro
    GlassCannon,    // Viel Schaden, wenig HP
    Support,        // Heilt/Bufft
    Debuffer,       // Schwächt Gegner
    Balanced        // Allrounder
}

// 2. STATS (Zusammengeführt: Attribute + Kampfwerte + Resistenzen)
public enum StatType
{
    // --- Attribute (Basiswerte) ---
    Strength,       // Stärke (Erhöht z.B. Physischen Schaden)
    Agility,        // Beweglichkeit (Erhöht z.B. Crit / Ausweichen)
    Intelligence,   // Intelligenz (Erhöht z.B. Magischen Schaden / Mana)
    Vitality,       // Vitalität (Erhöht HP)

    // --- Ressourcen ---
    MaxHealth,
    MaxMana,

    // --- Offensive Kampfwerte ---
    Damage,
    AttackSpeed,
    CritChance,
    CritMultiplier,
    ArmorPenetration,
    Accuracy,

    // --- Defensive Kampfwerte ---
    Defense,
    Evasion,
    LifeLeech,

    // --- Resistenzen (Wichtig für Rüstung & Magie) ---
    ResistPhysical,
    ResistFire,
    ResistIce,
    ResistPoison,
    ResistLightning,
    ResistMagic     // Allgemeine Magieresistenz
}

// 4. Schadensarten
public enum DamageType
{
    Physical,
    Fire,
    Ice,
    Poison,
    Lightning,
    True // Ignoriert alles
}

public enum HitType
{
    Normal,
    Critical,
    Miss,
    Block
}

// 6. AI Verhalten
public enum TargetPriority
{
    Tank,           // Höchste Bedrohung / Rüstung
    LowestHP,       // Finisher
    Random,         // Unberechenbar
    HighestDPS,     // Tötet den gefährlichsten Spieler
    Support         // Fokus auf Heiler
}

// Für das interne Berechnungssystem
public enum StatModType
{
    Flat = 100,
    PercentAdd = 200,
    PercentMult = 300
}

public enum AttackRange
{
    Melee,  // Nahkampf (läuft hin)
    Ranged  // Fernkampf (bleibt stehen)
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
    // --- REIHE 1: BLAU (FRONT / TANK) ---
    [InspectorName("Front - Oben (Slot 0)")] Front_Top = 0,
    [InspectorName("Front - Mitte (Slot 1)")] Front_Center = 1,
    [InspectorName("Front - Unten (Slot 2)")] Front_Bottom = 2,

    // --- REIHE 2: GELB (MID / SUPPORT) ---
    [InspectorName("Mitte - Oben (Slot 3)")] Mid_Top = 3,
    [InspectorName("Mitte - Mitte (Slot 4)")] Mid_Center = 4,
    [InspectorName("Mitte - Unten (Slot 5)")] Mid_Bottom = 5,

    // --- REIHE 3: ROT (BACK / RANGE) ---
    [InspectorName("Hinten - Oben (Slot 6)")] Back_Top = 6,
    [InspectorName("Hinten - Mitte (Slot 7)")] Back_Center = 7,
    [InspectorName("Hinten - Unten (Slot 8)")] Back_Bottom = 8
}

[System.Serializable]
public struct StatConfig
{
    public StatType type;
    public float value;
}