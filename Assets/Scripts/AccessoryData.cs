using UnityEngine;

[CreateAssetMenu(fileName = "NewRing", menuName = "Dungeon/Items/Ring")]
public class AccessoryData : PurchasableCardData
{
    [Header("Art des Schmucks")]
    public EquipmentType equipmentType;

    [Header("Boni")]
    public int bonusHealth = 0;
    public int bonusDamage = 0;
    public int bonusDefense = 0;

    // HIER GELÖSCHT: public string effectDescription...
    // Nutze stattdessen das geerbte "description" Feld im Inspector.

    protected void OnValidate()
    {
        cardType = CardType.Accessory;
    }
}