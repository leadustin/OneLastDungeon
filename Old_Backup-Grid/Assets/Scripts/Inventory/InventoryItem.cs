using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public CardData data;
    public int stackSize;

    public InventoryItem(CardData d, int s)
    {
        data = d;
        stackSize = s;
    }
}