using UnityEngine;

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

    // --- NEU: HIER IST DAS FEHLENDE FELD ---
    [TextArea]
    public string description;
    // ---------------------------------------

    public Sprite artwork;
    public CardType cardType;
}