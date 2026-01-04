using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Game Data/Items/Armor")] // <--- ANGEPASST
public class ArmorData : PurchasableCardData
{
    [Header("Welches Rüstungsteil?")]
    public EquipmentType equipmentType;

    [Header("Rüstung Werte")]
    public int defenseAmount = 3;

    protected void OnValidate()
    {
        cardType = CardType.Armor;
    }
}