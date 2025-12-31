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
    public Sprite artwork;
    public CardType cardType;
}