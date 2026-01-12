using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game Data/Items/Weapon")]
public class WeaponData : PurchasableCardData
{
    [Header("Waffen Art")]
    public EquipmentType equipmentType;

    [Header("Waffen Schaden")]
    public int minDamage = 8;
    public int maxDamage = 12;

    protected void OnValidate()
    {
        cardType = CardType.Weapon;
        if (maxDamage < minDamage) maxDamage = minDamage;
    }
}