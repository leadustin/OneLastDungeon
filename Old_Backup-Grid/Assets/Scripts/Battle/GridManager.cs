using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Level Flow")]
    public List<LevelData> levels;
    public static int savedLevelIndex = 0;
    public int currentLevelIndex = 0;

    [Header("Level Design")]
    public Tilemap levelLayout;
    public TileBase startTile;
    public TileBase exitTile;
    public CardData exitCardData;
    public GameObject missionDisplayPrefab;

    [Header("Aktuelles Level (Debug)")]
    public LevelData currentLevel;
    public int currentKillCount = 0; // --- NEU: Zählt getötete Gegner ---

    [Header("UI")]
    public GameObject resultWindowPrefab;

    [Header("Layout")]
    public float spacingX = 1.6f;
    public float spacingY = 1.6f;
    public float gridOffsetY = 1.0f;
    public GameObject cardPrefab;

    [Header("Editor Debugging")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    public Vector2 cardGizmoSize = new Vector2(1.4f, 2.0f);

    private Dictionary<Vector2Int, CardController> allCards = new Dictionary<Vector2Int, CardController>();
    private Vector2Int playerPos;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void OnNewGameClicked()
    {
        GridManager.savedLevelIndex = 0; // Reset für neuen Run
        SceneManager.LoadScene("GameScene");
    }

    void Start()
    {
        PlayerManager.Instance.ResetSessionStats();

        // --- ÄNDERUNG: Wir laden das Level, das wir uns gemerkt haben ---
        // Beim allerersten Start ist es 0. Nach Level 2 ist es 2.
        LoadLevel(savedLevelIndex);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Count == 0) return;
        if (index >= levels.Count) return;
        var oldWindows = FindObjectsByType<ResultWindow>(FindObjectsSortMode.None);
        foreach (var win in oldWindows) Destroy(win.gameObject);

        savedLevelIndex = index;
        currentLevelIndex = index;
        currentLevel = levels[currentLevelIndex];
        currentKillCount = 0;

        // Prefab (Tilemap) laden, ABER noch nicht scannen/spawnen
        if (currentLevel.levelPrefab != null)
        {
            if (levelLayout != null && levelLayout.transform.parent == transform)
            {
                Destroy(levelLayout.gameObject);
            }

            GameObject layoutObj = Instantiate(currentLevel.levelPrefab, transform);
            levelLayout = layoutObj.GetComponent<Tilemap>();
            layoutObj.transform.localPosition = Vector3.zero;

            // WICHTIG: Wir verstecken die Blaupause sofort, damit man sie nicht sieht
            layoutObj.SetActive(false);
        }

        // Grid löschen (alte Karten wegräumen)
        foreach (var card in allCards.Values)
        {
            if (card != null && card.gameObject != null) Destroy(card.gameObject);
        }
        allCards.Clear();

        // Statt Grid zu bauen, zeigen wir jetzt das UI
        ShowLevelMission();
    }

    void ShowLevelMission()
    {
        if (missionDisplayPrefab == null)
        {
            // Fallback: Kein UI da? Dann sofort starten!
            StartLevelGameplay();
            return;
        }

        string missionTitle = $"Level {currentLevelIndex + 1}";
        string missionText = "";

        switch (currentLevel.winCondition)
        {
            case WinCondition.ReachExit: missionText = "Finde den Ausgang!"; break;
            case WinCondition.ClearAll: missionText = "Besiege alle Gegner!"; break;
            case WinCondition.KillQuota: missionText = $"Besiege {currentLevel.enemiesToKill} Gegner!"; break;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            GameObject uiObj = Instantiate(missionDisplayPrefab, canvas.transform);
            uiObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            MissionDisplay displayScript = uiObj.GetComponent<MissionDisplay>();
            if (displayScript != null)
            {
                displayScript.SetMissionInfo(missionTitle, missionText);

                // HIER IST DER TRICK:
                // Wir sagen dem UI: "Wenn geklickt wird, ruf meine StartLevelGameplay Funktion auf"
                displayScript.onFightClicked = StartLevelGameplay;
            }
        }
        else
        {
            // Notfall: Kein Canvas -> Sofort starten
            StartLevelGameplay();
        }
    }

    void StartLevelGameplay()
    {
        if (levelLayout != null)
        {
            // Jetzt erst wird das Grid berechnet und Karten gespawnt
            GenerateGridFromTilemap();
        }
    }

    public void EndGame(bool playerWon)
    {
        // 1. Canvas finden (damit wir das UI dort reinwerfen können)
        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas != null && resultWindowPrefab != null)
        {
            // 2. Fenster instanziieren (erschaffen)
            GameObject windowObj = Instantiate(resultWindowPrefab, canvas.transform);

            // 3. Mittig positionieren
            RectTransform rect = windowObj.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;

            // 4. Daten übergeben
            ResultWindow winScript = windowObj.GetComponent<ResultWindow>();
            if (winScript != null)
            {
                // Wir holen uns die gesammelten Werte aus dem PlayerManager
                int goldEarned = (int)PlayerManager.Instance.sessionMoney;
                int expEarned = PlayerManager.Instance.sessionExp;

                winScript.Setup(playerWon, goldEarned, expEarned);
            }
        }
        else
        {
            Debug.LogError("Kein Canvas oder ResultWindowPrefab gefunden!");
        }
    }

    public void NextLevel()
    {
        Debug.Log("Level geschafft!");
        LoadLevel(currentLevelIndex + 1);
    }

    // --- NEU: Prüft ob wir gewonnen haben ---
    void CheckWinCondition()
    {
        bool won = false;

        if (currentLevel.winCondition == WinCondition.ClearAll)
        {
            int enemiesLeft = 0;
            foreach (var card in allCards.Values)
            {
                if (card.myData is EnemyData) enemiesLeft++;
            }
            if (enemiesLeft == 0) won = true;
        }
        else if (currentLevel.winCondition == WinCondition.KillQuota)
        {
            if (currentKillCount >= currentLevel.enemiesToKill) won = true;
        }
        else if (currentLevel.winCondition == WinCondition.ReachExit)
        {
            // Diese Prüfung passiert eigentlich in InteractWithCard, 
            // aber falls wir es hier brauchen:
            // (ReachExit wird meist sofort ausgelöst, wenn man drauf tritt)
        }

        if (won)
        {
            Debug.Log("Siegbedingung erfüllt! Zeige Fenster.");
            // ALT: NextLevel();  <-- Das löschen wir

            // NEU: Fenster anzeigen mit 'true' für gewonnen
            EndGame(true);
        }
    }

    // ... GenerateGridFromTilemap bleibt wie vorher ...
    void GenerateGridFromTilemap()
    {
        levelLayout.gameObject.SetActive(false);
        foreach (var card in allCards.Values) if (card != null) Destroy(card.gameObject);
        allCards.Clear();

        levelLayout.CompressBounds();
        BoundsInt bounds = levelLayout.cellBounds;
        List<Vector2Int> validPositions = new List<Vector2Int>();

        bool startFound = false;
        Vector2Int exitPos = new Vector2Int(-999, -999);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (levelLayout.HasTile(tilePos))
                {
                    validPositions.Add(new Vector2Int(x, y));
                    TileBase currentTile = levelLayout.GetTile(tilePos);
                    if (currentTile == startTile) { playerPos = new Vector2Int(x, y); startFound = true; }
                    else if (currentTile == exitTile) { exitPos = new Vector2Int(x, y); }
                }
            }
        }

        if (!startFound) playerPos = GetClosestToCenter(validPositions, bounds);

        foreach (Vector2Int pos in validPositions)
        {
            // Wenn "ClearAll" aktiv ist, spawnen wir KEINEN Ausgang, auch wenn einer gemalt wurde!
            bool isExit = (pos == exitPos) && (currentLevel.winCondition == WinCondition.ReachExit);
            SpawnSingleCard(pos.x, pos.y, pos == playerPos, isExit);
        }
    }

    void SpawnSingleCard(int x, int y, bool isPlayer, bool isExit)
    {
        float posX = x * spacingX;
        float posY = y * spacingY + gridOffsetY;
        Vector3 spawnPos = new Vector3(posX, posY, 0);

        GameObject newObj = Instantiate(cardPrefab, spawnPos, Quaternion.identity, this.transform);
        CardController newCard = newObj.GetComponent<CardController>();
        newCard.name = $"Card {x}_{y}";

        allCards[new Vector2Int(x, y)] = newCard;

        if (isPlayer)
        {
            newCard.SetupCard(x, y, null, true);
            var camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.target = newCard.transform;
        }
        else if (isExit && exitCardData != null)
        {
            newCard.SetupCard(x, y, exitCardData, false);
        }
        else
        {
            CardData randomData = GetWeightedRandomCard();
            newCard.SetupCard(x, y, randomData, false);
        }
    }

    // ... Hilfsfunktionen ...
    Vector2Int GetClosestToCenter(List<Vector2Int> positions, BoundsInt bounds)
    {
        if (positions.Count == 0) return Vector2Int.zero;
        Vector2 center = new Vector2(bounds.center.x, bounds.center.y);
        Vector2Int bestPos = positions[0];
        float bestDist = float.MaxValue;
        foreach (var pos in positions) { float dist = Vector2.Distance(pos, center); if (dist < bestDist) { bestDist = dist; bestPos = pos; } }
        return bestPos;
    }

    public void TryMovePlayer(CardController target)
    {
        if (CombatManager.Instance.isFighting || target.isPlayer) return;
        int dx = Mathf.Abs(target.gridX - playerPos.x);
        int dy = Mathf.Abs(target.gridY - playerPos.y);
        if (dx <= 1 && dy <= 1) InteractWithCard(target);
    }

    void InteractWithCard(CardController target)
    {
        CardData data = target.myData;

        // --- Leere Karte (Boden) ---
        if (data == currentLevel.emptyCard)
        {
            FinishMovement(target);
            return;
        }

        if (data == null) { FinishMovement(target); return; }

        // --- Sieg Typ A: Ausgang ---
        if (data == exitCardData)
        {
            if (currentLevel.winCondition == WinCondition.ReachExit)
            {
                // ALT: NextLevel();
                EndGame(true); // NEU
            }
            return;
        }

        if (data is EnemyData)
        {
            if (allCards.ContainsKey(playerPos)) CombatManager.Instance.StartCombat(allCards[playerPos], target);
            return;
        }
        else if (data is CurrencyData moneyDrop)
        {
            // Gold logic...
            int minVal = (int)moneyDrop.GetMinInCopper();
            int maxVal = (int)moneyDrop.GetMaxInCopper();
            int baseAmount = Random.Range(minVal, maxVal + 1);
            long finalAmount = (long)(baseAmount * currentLevel.goldMultiplier);
            PlayerManager.Instance.AddMoney(finalAmount);
            string formattedText = PlayerManager.FormatMoney(finalAmount);
            FloatingTextManager.Instance.Show(CombatTextType.Gold, formattedText, target.transform.position);
            FinishMovement(target);
        }
        else
        {
            // Item logic...
            PlayerManager.Instance.AddItemToInventory(data);
            if (data.artwork != null) FloatingTextManager.Instance.ShowIcon(data.artwork, "", target.transform.position);
            FinishMovement(target);
        }
    }

    public void FinishMovement(CardController target)
    {
        Vector2Int oldPos = playerPos;
        Vector2Int newPos = new Vector2Int(target.gridX, target.gridY);

        // --- Zählen für Sieg-Bedingungen ---
        if (target.myData is EnemyData)
        {
            currentKillCount++;
        }

        if (!allCards.ContainsKey(oldPos)) return;
        CardController playerCard = allCards[oldPos];

        Vector3 targetPosWorld = target.transform.position;
        if (target.gameObject != null) Destroy(target.gameObject);

        allCards.Remove(newPos);
        allCards.Remove(oldPos);

        playerCard.transform.position = targetPosWorld;
        playerCard.gridX = newPos.x;
        playerCard.gridY = newPos.y;

        allCards[newPos] = playerCard;
        playerPos = newPos;

        // --- NEU: Spawn-Logik ---
        if (currentLevel.respawnCards)
        {
            // Alte Logik: Neuer Gegner kommt nach
            SpawnNewCardAt(oldPos.x, oldPos.y, false);
        }
        else
        {
            // Neue Logik: Leere Karte spawnen
            SpawnNewCardAt(oldPos.x, oldPos.y, true);
        }

        // --- Nach jeder Bewegung prüfen ob gewonnen ---
        CheckWinCondition();
    }

    // Angepasste Funktion: Kann jetzt auch explizit "Leer" spawnen
    void SpawnNewCardAt(int x, int y, bool spawnEmpty)
    {
        float posX = x * spacingX;
        float posY = y * spacingY + gridOffsetY;
        Vector3 pos = new Vector3(posX, posY, 0);

        GameObject newObj = Instantiate(cardPrefab, pos, Quaternion.identity, this.transform);
        CardController card = newObj.GetComponent<CardController>();

        if (spawnEmpty && currentLevel.emptyCard != null)
        {
            card.SetupCard(x, y, currentLevel.emptyCard, false);
        }
        else
        {
            card.SetupCard(x, y, GetWeightedRandomCard(), false);
        }

        allCards[new Vector2Int(x, y)] = card;
    }

    CardData GetWeightedRandomCard()
    {
        if (currentLevel == null) return null;
        bool spawnEnemy = Random.value > 0.4f;
        List<SpawnRate> pool = spawnEnemy ? currentLevel.enemies : currentLevel.items;
        if (pool == null || pool.Count == 0) pool = currentLevel.items;
        if (pool == null || pool.Count == 0) return null;

        float totalWeight = 0;
        foreach (var entry in pool) totalWeight += entry.weight;
        float randomPoint = Random.Range(0, totalWeight);
        float currentWeight = 0;
        foreach (var entry in pool) { currentWeight += entry.weight; if (randomPoint <= currentWeight) return entry.card; }
        return pool[0].card;
    }

    // ... Gizmos & Rest bleiben gleich ...
    public Vector3 GetPlayerCardPosition() { if (allCards.ContainsKey(playerPos)) return allCards[playerPos].transform.position; return Vector3.zero; }
    private void OnDrawGizmos() { /* ... Dein Gizmo Code ... */ }
}