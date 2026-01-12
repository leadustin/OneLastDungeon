using UnityEngine;

public enum StatusType
{
    HealOverTime,   // HoT (z.B. Regeneration)
    DamageOverTime  // DoT (z.B. Brennen, Gift)
}

[System.Serializable]
public class ActiveStatusEffect
{
    public string effectName;   // Name (z.B. "Brennen")
    public StatusType type;     // Typ
    public int amountPerRound;  // Wie stark pro Runde?
    public int remainingRounds; // Wie lange noch?

    // Konstruktor zum einfachen Erstellen
    public ActiveStatusEffect(string name, StatusType type, int amount, int duration)
    {
        this.effectName = name;
        this.type = type;
        this.amountPerRound = amount;
        this.remainingRounds = duration;
    }
}