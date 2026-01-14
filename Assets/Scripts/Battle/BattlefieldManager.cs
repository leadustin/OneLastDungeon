using System.Collections.Generic;
using UnityEngine;

public class BattlefieldManager : MonoBehaviour
{
    public static BattlefieldManager Instance;

    [Header("Core Assets")]
    public GameObject unitTokenPrefab;
    public SpriteRenderer backgroundRenderer;

    [Header("Grid Configuration")]
    public List<Transform> playerSlots;
    public List<Transform> enemySlots;

    [Header("Aktuelles Level")]
    public LevelTemplate currentLevel;

    [Header("Systems")]
    public BattleSystem battleSystem;
    public BattleResultUI resultUI;

    // State
    private BattleUnit activeHero;
    private readonly List<BattleUnit> activeEnemies = new();


    // Session Kasse (Was haben wir in diesem Kampf gesammelt?)
    private int sessionGoldEarned = 0;
    private int sessionXPEarned = 0;
    private readonly List<DroppedItem> sessionItems = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Prüfen, ob wir über den GameManager gekommen sind (Levelwahl)
        if (GameManager.Instance != null && GameManager.Instance.levelToLoad != null)
        {
            LoadLevel(GameManager.Instance.levelToLoad);
        }
        else if (currentLevel != null)
        {
            // Fallback für Tests direkt aus der Szene
            LoadLevel(currentLevel);
        }
    }

    public void LoadLevel(LevelTemplate level)
    {
        currentLevel = level;

        // Reset Session Loot
        sessionGoldEarned = 0;
        sessionXPEarned = 0;
        sessionItems.Clear();

        // Hintergrund setzen
        if (backgroundRenderer != null && level.backgroundImage != null)
        {
            backgroundRenderer.sprite = level.backgroundImage;
        }

        SetupBattle();
    }

    void SetupBattle()
    {
        // 1. Helden spawnen (Leader)
        if (PartyManager.Instance != null)
        {
            var leaderData = PartyManager.Instance.GetLeader();
            if (leaderData != null)
            {
                activeHero = SpawnHero(playerSlots[4], leaderData); // Mitte-Mitte
            }
        }
        else
        {
            // Fallback ohne PartyManager (Debug)
            Debug.LogWarning("Kein PartyManager gefunden! Spawne Dummy Hero?");
        }

        // 2. Gegner spawnen (Erste Welle)
        SpawnWave();
    }

    void SpawnWave()
    {
        ClearEnemies();

        if (currentLevel != null && currentLevel.waves.Count > 0)
        {
            // Simpel: Wir nehmen immer die erste Welle für diesen Test
            // Später: Wellen-Index hochzählen
            var wave = currentLevel.waves[0];

            foreach (var setup in wave.enemies)
            {
                // Slot finden basierend auf GridPosition Enum
                int slotIndex = (int)setup.position;
                if (slotIndex >= enemySlots.Count) slotIndex = 0;

                Transform targetSlot = enemySlots[slotIndex];
                BattleUnit newEnemy = SpawnEnemy(targetSlot, setup.enemyTemplate);
                activeEnemies.Add(newEnemy);
            }
        }

        // Kampf starten!
        if (battleSystem != null && activeHero != null)
        {
            battleSystem.StartBattle(activeHero, activeEnemies);
        }
    }

    // --- SPAWN HELPER ---

    BattleUnit SpawnHero(Transform slot, HeroRuntimeData data)
    {
        foreach (Transform child in slot)
            Destroy(child.gameObject);

        GameObject token = Instantiate(unitTokenPrefab, slot.position, Quaternion.identity);
        token.transform.SetParent(slot);

        if (token.TryGetComponent<BattleUnit>(out var unit))
        {
            unit.SetupHeroFromRuntime(data);
        }

        return unit;
    }


    BattleUnit SpawnEnemy(Transform slot, EnemyTemplate enemyData)
    {
        foreach (Transform child in slot)
            Destroy(child.gameObject);

        GameObject token = Instantiate(unitTokenPrefab, slot.position, Quaternion.identity);
        token.transform.SetParent(slot);

        if (token.TryGetComponent<BattleUnit>(out var unit))
        {
            unit.SetupEnemy(enemyData);
        }

        return unit;
    }


    void ClearEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }

    // --- LOOT LOGIC ---

    public void AddLoot(int gold, int xp, List<DroppedItem> items)
    {
        sessionGoldEarned += gold;
        sessionXPEarned += xp;

        if (items != null)
        {
            foreach (var newItem in items)
            {
                // Stapelbare Items zusammenfassen
                var existing = sessionItems.Find(x => x.item == newItem.item);
                if (existing != null)
                {
                    existing.amount += newItem.amount;
                }
                else
                {
                    sessionItems.Add(new DroppedItem { item = newItem.item, amount = newItem.amount });
                }
            }
        }
    }

    // --- UI HELPER ---

    public void ToggleSlotVisuals(bool show)
    {
        // Optional: Zeige Kreise unter den Einheiten an/aus
        // Hier erstmal leer lassen oder Visuals implementieren
    }

    // --- NEU: DIESE METHODEN FEHLTEN ---

    public void OnBattleWon()
    {
        Debug.Log("Kampf gewonnen!");

        // 1. Level als geschafft markieren im GameManager
        if (GameManager.Instance != null && currentLevel != null)
        {
            // Belohnung für Level-Abschluss (zusätzlich zum Loot?)
            sessionGoldEarned += currentLevel.goldReward;
            sessionXPEarned += currentLevel.xpReward;
        }

        // 2. UI anzeigen (und Loot übergeben)
        if (resultUI != null)
        {
            resultUI.ShowVictory(sessionGoldEarned, sessionXPEarned, sessionItems);
        }
    }

    public void OnBattleLost()
    {
        Debug.Log("Kampf verloren!");

        // Bei Niederlage zeigen wir das UI an, aber ohne Loot (oder wenig)
        if (resultUI != null)
        {
            // Wir rufen ShowVictory mit 0 auf. 
            // TODO: Später eine ShowDefeat() Methode im UI bauen für roten Text "NIEDERLAGE"
            resultUI.ShowVictory(0, 0, null);
        }
    }
}