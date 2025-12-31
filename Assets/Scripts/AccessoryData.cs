using UnityEngine;

[CreateAssetMenu(fileName = "NewRing", menuName = "Dungeon/Items/Ring")]
public class AccessoryData : PurchasableCardData // Erbt jetzt von PurchasableCardData
{
    [Header("Ring Boni")]
    public int bonusHealth = 0;
    public int bonusDamage = 0;
    public int bonusDefense = 0;
    [TextArea] public string effectDescription = "Magischer Effekt...";

    protected void OnValidate()
    {
        cardType = CardType.Accessory;
    }
}