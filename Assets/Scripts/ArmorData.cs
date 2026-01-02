using UnityEngine;

[CreateAssetMenu(fileName = "New Armor", menuName = "Card System/Armor")]
public class ArmorData : PurchasableCardData
{
    [Header("Welches Rüstungsteil?")]
    public EquipmentType equipmentType; // <--- NEU: Wähle hier Head, Chest, Legs etc.

    [Header("Rüstung Werte")]
    public int defenseAmount = 3;

    protected void OnValidate()
    {
        cardType = CardType.Armor;
    }
}