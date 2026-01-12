using UnityEngine;

public enum PotionType
{
    Health,
    Mana
}

[CreateAssetMenu(fileName = "NewPotion", menuName = "Game Data/Items/Potion")]
public class ConsumableData : PurchasableCardData
{
    [Header("Trank Einstellungen")]
    public PotionType potionType;

    [Header("Wirkungs-Bereich")]
    public int minAmount = 10;
    public int maxAmount = 20;

    protected void OnValidate()
    {
        cardType = CardType.Potion;
    }
}