using UnityEngine;

public abstract class ItemTemplate : ScriptableObject
{
    [Header("Basis Info")]
    public string id;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Filter & Kategorien")]
    public ItemCategory category;   // z.B. Weapon, Armor, Material
    public ItemSubType subType;

    public ItemType type;
    public ItemRarity rarity;

    [Header("Wirtschaft")]
    public int maxStackSize = 1; // 1 für Waffen, 99 für Materialien
    public int sellPrice = 10;

    // Kleiner Helfer für die Farbe im Tooltip
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Trash: return Color.gray;
            case ItemRarity.Common: return Color.white;
            case ItemRarity.Fine: return Color.cyan;          // Blue
            case ItemRarity.Masterwork: return Color.green;
            case ItemRarity.Rare: return Color.yellow;
            case ItemRarity.Exotic: return new Color(1f, 0.5f, 0f); // Orange
            case ItemRarity.Legendary: return new Color(0.6f, 0f, 0.8f); // Purple
            default: return Color.white;
        }
    }
}