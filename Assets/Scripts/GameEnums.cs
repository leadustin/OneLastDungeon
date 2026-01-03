using UnityEngine;

// Deine definierte Liste aller Spielwerte für den Idle-Battler
public enum StatType
{
    // --- 1. Offensiv ---
    AttackDamage,           // Angriffsschaden (Flat)
    AttackSpeed,            // Angriffstempo (%)
    CritChance,             // Kritische Trefferchance (%)
    CritDamage,             // Kritischer Trefferschaden (%)
    ArmorPenetration,       // Rüstungsignorierung (Flat/%)
    BonusDamageLowHp,       // Bonus-Schaden gegen Ziele unter X % Leben

    // --- 2. Defensiv ---
    MaxHealth,              // Maximales Leben (Flat)
    Armor,                  // Rüstung (Flat)
    MagicResist,            // Magieresistenz (Flat)
    DamageReduction,        // Schadensreduktion (%)
    StartShield,            // Schild beim Kampfbeginn
    DamageReductionLowHp,   // Schadensreduktion unter X % Leben

    // --- 3. Kampfdynamik ("Aktiv") ---
    Lifesteal,              // Lebensraub (%)
    HealthRegen,            // Lebensregeneration pro Sekunde
    DodgeChance,            // Ausweichchance (%)
    BlockChance,            // Blockchance (%)
    Thorns,                 // Gegenschlag bei erlittenem Schaden
    KillHeal,               // Heilung beim Töten eines Gegners

    // --- 4. On-Hit / Procs (Chance auf Effekt) ---
    OnHitDamage,            // Chance auf zusätzlichen Schaden
    StunChance,             // Chance auf Betäubung
    SlowChance,             // Chance auf Verlangsamung
    StatusEffectChance,     // Chance auf Blutung / Gift / Brand
    MultiAttack,            // Mehrfachangriff (cleave / chain)
    DeathExplosion,         // Explosionsschaden beim Tod eines Gegners

    // --- 5. Skalierung (Mid-/Late-Game) ---
    BonusPerRound,          // Bonus pro Runde
    BonusPerWin,            // Bonus pro Sieg in Folge
    BonusAllyDeath,         // Bonus pro Tod eines Verbündeten
    BonusPerItem,           // Bonus pro eingesammeltem Item

    // --- 6. Anti-Kontrolle (Meta) ---
    Tenacity,               // Verkürzte Betäubungsdauer / Kontroll-Resistenz
    StartImmunity,          // Immunität gegen erste Kontrolle
    Cleanse,                // Reinigung negativer Effekte
    KnockbackResist         // Rückwurf- oder Knockback-Resistenz
}

// Die Struktur für einen einzelnen Bonus auf einem Item
[System.Serializable]
public struct ItemBonus
{
    public StatType statType; // Welcher Wert (z.B. CritChance)
    public float amount;      // Wie viel (z.B. 10.5)
}