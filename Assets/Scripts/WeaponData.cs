using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card System/Weapon")]
public class WeaponData : PurchasableCardData // Erbt jetzt von PurchasableCardData
{
    [Header("Waffen Werte")]
    public int damageAmount = 5;

    protected void OnValidate()
    {
        cardType = CardType.Weapon;
    }
}