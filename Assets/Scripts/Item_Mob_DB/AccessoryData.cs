using UnityEngine;

[CreateAssetMenu(fileName = "NewAccessory", menuName = "Game Data/Items/Accessory")]
public class AccessoryData : PurchasableCardData
{
    [Header("Art des Schmucks")]
    public EquipmentType equipmentType;

    [Header("Boni")]
    public int bonusHealth = 0;
    public int bonusDamage = 0;
    public int bonusDefense = 0;

    protected void OnValidate()
    {
        cardType = CardType.Accessory;
    }
}