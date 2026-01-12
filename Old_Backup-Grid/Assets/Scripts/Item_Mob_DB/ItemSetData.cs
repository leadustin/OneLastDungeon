using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SetBonusStep
{
    public int itemsRequired;
    public List<ItemBonus> bonuses;
}

[CreateAssetMenu(fileName = "NewItemSet", menuName = "Game Data/Item Set")]
public class ItemSetData : ScriptableObject
{
    public string setName;

    // --- NEU: Liste der Items, die zu diesem Set gehören ---
    public List<CardData> itemsInSet;
    // ------------------------------------------------------

    [Header("Boni nach Anzahl")]
    public List<SetBonusStep> setBonuses;
}