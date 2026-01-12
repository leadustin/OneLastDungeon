using UnityEngine;
using System.Collections.Generic;

// 1. Die Sieg-Typen
public enum WinCondition
{
    ReachExit,  // Typ A: Zum Ausgang laufen
    ClearAll,   // Typ B: Alle Gegner besiegen
    KillQuota   // Typ C: Bestimmte Anzahl töten
}

// 2. Die fehlende Klasse für die Wahrscheinlichkeiten (WICHTIG!)
[System.Serializable]
public class SpawnRate
{
    public CardData card;
    public float weight; // Wie wahrscheinlich ist diese Karte?
}

// 3. Das eigentliche LevelData ScriptableObject
[CreateAssetMenu(fileName = "NewLevel", menuName = "CardGame/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Design")]
    public GameObject levelPrefab;

    [Header("Spielregeln")]
    public WinCondition winCondition = WinCondition.ReachExit;
    public bool respawnCards = true; // AN = Endlos, AUS = Level leert sich
    public int enemiesToKill = 0;    // Nur für "KillQuota" wichtig
    public CardData emptyCard;       // <-- Die "Leere Kachel" (Boden)

    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 4;
    public float goldMultiplier = 1.0f;

    [Header("Inhalte")]
    public List<SpawnRate> enemies;
    public List<SpawnRate> items;
}