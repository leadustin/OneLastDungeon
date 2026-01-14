using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelEnemySetup
{
    public EnemyTemplate enemyTemplate;
    [Tooltip("Wo soll der Gegner stehen?")]
    public GridPosition position;
}

// NEU: Eine Welle besteht aus einer Liste von Gegnern
[System.Serializable]
public struct WaveDefinition
{
    public string waveName; // z.B. "Vorhut" oder "BOSS"
    public List<LevelEnemySetup> enemies;
}

[CreateAssetMenu(fileName = "New Level", menuName = "RPG/Level Template")]
public class LevelTemplate : ScriptableObject
{
    [Header("Level Info")]
    public string levelName;
    public string levelID;
    public bool isBossLevel;

    [Header("Atmosphäre")]
    public Sprite backgroundImage;

    [Header("Wellen Konfiguration")]
    // Statt einer flachen Liste haben wir jetzt eine Liste von Wellen
    public List<WaveDefinition> waves;

    [Header("Belohnung bei Sieg")]
    public int goldReward;
    public int xpReward;
}