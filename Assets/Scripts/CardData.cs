using UnityEngine;
using System.Collections.Generic; // <--- WICHTIG: Hinzugefügt für List<>

public enum CardType
{
    Weapon,
    Armor,
    Potion,
    Gold,
    Enemy,
    Resource,
    Accessory,
    Material
}

public abstract class CardData : ScriptableObject
{
    public string cardName;

    [TextArea]
    public string description; // Dein Lore-Text

    public Sprite artwork;
    public CardType cardType;

    // --- NEU: Die Liste für die Boni ---
    [Header("Zusätzliche Boni (Affixe)")]
    public List<ItemBonus> itemBonuses;
    // ----------------------------------
}