using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Card System/Weapon")]
public class WeaponData : PurchasableCardData
{
    [Header("Waffen Typ")]
    public EquipmentType equipmentType = EquipmentType.Weapon; // <--- NEU (Standard: Waffe)

    [Header("Waffen Werte")]
    public int damageAmount = 5;

    protected void OnValidate()
    {
        cardType = CardType.Weapon;
    }
}