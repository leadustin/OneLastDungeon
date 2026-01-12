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
    private int currentWaveIndex = 0;
    private BattleUnit activeHero;
    private List<BattleUnit> activeEnemies = new List<BattleUnit>();

    // Session Kasse
    private int sessionGoldEarned = 0;
    private int sessionXPEarned = 0;
    private List<DroppedItem> sessionItems = new List<DroppedItem>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.levelToLoad != null)
        {
            LoadLevel(GameManager.Instance.levelToLoad);
        }
        else if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
    }

    public void LoadLevel(LevelTemplate level)
    {
        currentLevel = level;
        currentWaveIndex = 0;

        // Reset Kasse
        sessionGoldEarned = 0;
        sessionXPEarned = 0;
        sessionItems.Clear();

        if (backgroundRenderer != null && level.backgroundImage != null)
        {
            backgroundRenderer.sprite = level.backgroundImage;
        }

        SpawnHeroIfNeeded();
        StartNextWave();
    }

    // --- HIER WURDE GEÄNDERT: LÄDT JETZT AUS PARTY MANAGER ---
    void SpawnHeroIfNeeded()
    {
        // Sicherheitscheck: Haben wir überhaupt Party-Daten?
        if (PartyManager.Instance == null || PartyManager.Instance.activeParty.Count == 0)
        {
            Debug.LogWarning("Keine Party im PartyManager! (Stelle sicher, dass PartyManager in der Startszene ist)");
            return;
        }

        // Alte Token löschen
        foreach (var slot in playerSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }

        activeHero = null;

        // Wir spawnen (vereinfacht) nur den ersten Helden auf Slot 0
        HeroRuntimeData heroData = PartyManager.Instance.activeParty[0];
        Transform spawnSlot = playerSlots[0];

        GameObject token = Instantiate(unitTokenPrefab, spawnSlot.position, Quaternion.identity);
        token.transform.SetParent(spawnSlot);

        BattleUnit unit = token.GetComponent<BattleUnit>();
        if (unit != null)
        {
            // WICHTIG: Hier rufen wir die neue Setup-Methode auf
            unit.SetupHeroFromRuntime(heroData);
        }
        activeHero = unit;
    }

    // ... (Der Rest bleibt gleich) ...
    public void OnWaveCleared()
    {
        Debug.Log($"Welle {currentWaveIndex + 1} geschafft!");
        currentWaveIndex++;

        if (currentLevel != null && currentLevel.waves != null && currentWaveIndex < currentLevel.waves.Count)
        {
            StartNextWave();
        }
        else
        {
            Debug.Log(">>> LEVEL COMPLETED <<<");
            if (resultUI != null)
            {
                int finalGold = sessionGoldEarned + currentLevel.goldReward;
                int finalXP = sessionXPEarned + currentLevel.xpReward;
                resultUI.ShowVictory(finalGold, finalXP, sessionItems);
            }
        }
    }

    public void OnPlayerDefeated()
    {
        if (resultUI != null) resultUI.ShowDefeat();
    }

    void StartNextWave()
    {
        ClearEnemies();
        if (currentLevel == null || currentLevel.waves == null || currentWaveIndex >= currentLevel.waves.Count) return;

        WaveDefinition wave = currentLevel.waves[currentWaveIndex];
        foreach (var enemySetup in wave.enemies)
        {
            int slotIdx = (int)enemySetup.position;
            if (slotIdx < enemySlots.Count && enemySetup.enemyTemplate != null)
            {
                BattleUnit newEnemy = SpawnEnemy(enemySlots[slotIdx], enemySetup.enemyTemplate);
                activeEnemies.Add(newEnemy);
            }
        }
        battleSystem.StartBattle(activeHero, activeEnemies);
    }

    public void AddLoot(int gold, int xp, List<DroppedItem> items)
    {
        sessionGoldEarned += gold;
        sessionXPEarned += xp;
        if (items != null)
        {
            foreach (var newItem in items)
            {
                var existing = sessionItems.Find(x => x.item == newItem.item);
                if (existing != null) existing.amount += newItem.amount;
                else sessionItems.Add(new DroppedItem { item = newItem.item, amount = newItem.amount });
            }
        }
    }

    void ClearEnemies()
    {
        foreach (var enemy in activeEnemies) if (enemy != null) Destroy(enemy.gameObject);
        activeEnemies.Clear();
    }

    BattleUnit SpawnEnemy(Transform slot, EnemyTemplate enemyData)
    {
        foreach (Transform child in slot) Destroy(child.gameObject);
        GameObject token = Instantiate(unitTokenPrefab, slot.position, Quaternion.identity);
        token.transform.SetParent(slot);
        BattleUnit unit = token.GetComponent<BattleUnit>();
        if (unit != null) unit.SetupEnemy(enemyData);
        return unit;
    }

    public void ToggleSlotVisuals(bool show)
    {
        foreach (Transform slot in playerSlots)
        {
            SpriteRenderer sr = slot.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = show;
        }
    }
}