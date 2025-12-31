using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Dungeon/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Größe")]
    public int gridWidth = 3;
    public int gridHeight = 3;

    [Header("Schwierigkeit & Ökonomie")]
    public float goldMultiplier = 1.0f; // Level 1 = 1.0, Level 2 = 1.5, etc.

    [Header("Spawn-Wahrscheinlichkeiten")]
    public List<SpawnRate> enemies;
    public List<SpawnRate> items;
}

[System.Serializable]
public class SpawnRate
{
    public CardData card;
    [Tooltip("Je höher die Zahl, desto öfter erscheint die Karte.")]
    public float weight = 1.0f;
}