using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Dungeon/Enemy")]
public class EnemyData : CardData
{
    [Header("Kampf Werte")]
    public int health = 20;
    public int damage = 5;
    public int defense = 0;

    [Header("Loot (Währung in Kupfer)")]
    public int minGoldDrop = 5;
    public int maxGoldDrop = 15;

    [Header("Loot (Items)")]
    public List<LootDrop> potentialDrops;

    protected void OnValidate()
    {
        cardType = CardType.Enemy;
    }
}

[System.Serializable]
public class LootDrop
{
    public CardData item;
    public float dropChance;
}