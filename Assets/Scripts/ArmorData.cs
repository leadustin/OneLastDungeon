using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Card System/Armor")]
public class ArmorData : PurchasableCardData // Erbt jetzt von PurchasableCardData
{
    [Header("Rüstung Werte")]
    public int defenseAmount = 3;

    protected void OnValidate()
    {
        cardType = CardType.Armor;
    }
}