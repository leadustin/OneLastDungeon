using UnityEngine;

public enum PotionType
{
    Health,
    Mana
}

[CreateAssetMenu(fileName = "New Potion", menuName = "Card System/Potion")]
public class ConsumableData : PurchasableCardData // Erbt jetzt von PurchasableCardData
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