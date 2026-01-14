using UnityEngine;

public static class CombatMath
{
    // --- BALANCING KONSTANTEN ---

    // "ResistScaling" (K): Gilt jetzt für Armor UND alle Resistenzen gleich.
    // Standard 10f heißt: 10 Resistenz auf Lvl 1 = 50% Reduktion.
    public static float RESIST_SCALING = 10f;

    // Für Dodge/Block/Status (Chance-basierte Stats)
    public static float CHANCE_SCALING = 20f;

    // ========================================================================
    // 1. MAPPING: DamageType -> StatType
    // ========================================================================
    public static StatType GetResistanceStat(DamageType dmgType)
    {
        switch (dmgType)
        {
            case DamageType.Physical: return StatType.Armor;
            case DamageType.Fire: return StatType.FireResist;
            case DamageType.Ice: return StatType.IceResist;
            case DamageType.Lightning: return StatType.LightningResist;
            case DamageType.Poison: return StatType.PoisonResist;
            case DamageType.Arcane: return StatType.ArcaneResist;
            case DamageType.Holy: return StatType.HolyResist;
            case DamageType.Shadow: return StatType.ShadowResist;
            default: return StatType.Armor; // Fallback
        }
    }

    // ========================================================================
    // 2. FORMEL: UNIFIZIERTE SCHADENSREDUKTION
    // ========================================================================

    public static float CalculateFinalDamage(float rawDamage, DamageType dmgType, float defenderResistValue, int attackerLevel)
    {
        // 1. Resistenzwert prüfen
        if (defenderResistValue <= 0) return rawDamage;

        // 2. Deine Soft-Cap Formel: Resist / (Resist + K * Lvl)
        float reduction = defenderResistValue / (defenderResistValue + (RESIST_SCALING * attackerLevel));

        // Optional: Hardcap bei z.B. 85%, damit niemand komplett immun wird
        if (reduction > 0.85f) reduction = 0.85f;

        // 3. Finaler Schaden
        return rawDamage * (1f - reduction);
    }

    // ========================================================================
    // 3. HILFSFUNKTIONEN (Für UI / Analyse)
    // ========================================================================

    // Berechnet % für Tooltip
    public static float GetReductionPercent(float resistValue, int attackerLevel)
    {
        if (resistValue <= 0) return 0f;
        return resistValue / (resistValue + (RESIST_SCALING * attackerLevel));
    }

    // Berechnet Effective HP (EHP) basierend auf Resistenz
    public static float GetEffectiveHP(float hp, float resistValue, int attackerLevel)
    {
        float reduction = GetReductionPercent(resistValue, attackerLevel);
        if (reduction >= 0.99f) reduction = 0.99f;
        return hp / (1f - reduction);
    }

    // Für Dodge/Crit Chancen (Andere Konstante)
    public static float CalculateChance(float rating, int level)
    {
        if (rating <= 0) return 0f;
        return rating / (rating + (CHANCE_SCALING * level));
    }
}