using System.Collections.Generic;
using UnityEngine;

public class BattlefieldManager : MonoBehaviour
{
    public static BattlefieldManager Instance;

    [Header("Settings")]
    public GameObject unitTokenPrefab; // Das Chip-Prefab

    [Header("Slots")]
    public List<Transform> playerSlots;
    public List<Transform> enemySlots;

    [Header("Kampf Daten")]
    // ZIEH HIER EINE DEINER ENEMY-DATEIEN REIN (z.B. Ork)
    public EnemyData debugEnemyToSpawn;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnBattleSetup();
    }

    void SpawnBattleSetup()
    {
        // 1. Den HELDEN spawnen
        // Wir brauchen hier keine Daten übergeben, er holt sie sich selbst vom PlayerManager
        if (playerSlots.Count > 0)
        {
            SpawnPlayerToken(playerSlots[0]); // Slot 0 = Vorne Oben (oder wo du ihn haben willst)
        }

        // 2. Den GEGNER spawnen
        // Hier nutzen wir die EnemyData, die du im Inspector reingezogen hast
        if (enemySlots.Count > 0 && debugEnemyToSpawn != null)
        {
            SpawnEnemyToken(enemySlots[4], debugEnemyToSpawn); // Slot 4 = Mitte
        }
    }

    // --- Hilfsfunktionen ---

    void SpawnPlayerToken(Transform slot)
    {
        GameObject token = Instantiate(unitTokenPrefab, slot.position, Quaternion.identity);
        token.transform.SetParent(slot);

        BattleUnit unit = token.GetComponent<BattleUnit>();
        if (unit != null)
        {
            unit.SetupPlayer(); // <--- Ruft die Logik für Helden auf
        }
    }

    void SpawnEnemyToken(Transform slot, EnemyData data)
    {
        GameObject token = Instantiate(unitTokenPrefab, slot.position, Quaternion.identity);
        token.transform.SetParent(slot);

        BattleUnit unit = token.GetComponent<BattleUnit>();
        if (unit != null)
        {
            unit.SetupEnemy(data); // <--- Ruft die Logik für Gegner auf (mit Daten)
        }
    }
}