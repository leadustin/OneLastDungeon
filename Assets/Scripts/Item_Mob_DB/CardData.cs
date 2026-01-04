using UnityEngine;
using System.Collections.Generic;

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
    public string description;

    public Sprite artwork;
    public CardType cardType;

    [Header("Einzel-Item Boni")]
    public List<ItemBonus> itemBonuses;

    [Header("Set Zugehörigkeit")]
    public ItemSetData itemSet;
}