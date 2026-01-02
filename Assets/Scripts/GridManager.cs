using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Level Daten")]
    public LevelData currentLevel;

    [Header("Layout")]
    public float spacingX = 1.6f;
    public float spacingY = 1.6f;
    public float gridOffsetY = 1.0f;
    public GameObject cardPrefab;

    private CardController[,] gridArray;
    private Vector2Int playerPos;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (currentLevel != null) GenerateGrid();
    }

    void GenerateGrid()
    {
        int width = currentLevel.gridWidth;
        int height = currentLevel.gridHeight;
        gridArray = new CardController[width, height];

        float startX = -((width - 1) * spacingX) / 2;
        float startY = ((height - 1) * spacingY) / 2;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(startX + (x * spacingX), startY - (y * spacingY) + gridOffsetY, 0);
                GameObject newObj = Instantiate(cardPrefab, pos, Quaternion.identity, this.transform);
                CardController card = newObj.GetComponent<CardController>();

                if (x == width / 2 && y == height / 2)
                {
                    playerPos = new Vector2Int(x, y);
                    card.SetupCard(x, y, null, true);
                }
                else
                {
                    card.SetupCard(x, y, GetWeightedRandomCard(), false);
                }
                gridArray[x, y] = card;
            }
        }
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

        if (data == null)
        {
            FinishMovement(target);
            return;
        }

        if (data is EnemyData)
        {
            CombatManager.Instance.StartCombat(gridArray[playerPos.x, playerPos.y], target);
            return;
        }
        else if (data is CurrencyData moneyDrop)
        {
            // --- GELD LOGIK (KORRIGIERT FÜR LONG) ---
            // Wir wandeln long-Werte explizit in int um für Random.Range
            int minVal = (int)moneyDrop.GetMinInCopper();
            int maxVal = (int)moneyDrop.GetMaxInCopper();

            int baseAmount = Random.Range(minVal, maxVal + 1);

            // Expliziter Cast zu long nach der Multiplikation mit dem float Multiplier
            long finalAmount = (long)(baseAmount * currentLevel.goldMultiplier);

            PlayerManager.Instance.AddMoney(finalAmount);

            string formattedText = PlayerManager.FormatMoney(finalAmount);
            FloatingTextManager.Instance.Show("+" + formattedText, Vector3.zero, Color.yellow, true);
        }
        else
        {
            // --- ITEM LOGIK ---
            PlayerManager.Instance.AddItemToInventory(data);

            // Wir zeigen das Icon statt Text
            FloatingTextManager.Instance.ShowIcon(data.artwork, "", target.transform.position, false);
        }

        FinishMovement(target);
    }

    public void FinishMovement(CardController target)
    {
        int oldX = playerPos.x;
        int oldY = playerPos.y;
        CardController playerCard = gridArray[oldX, oldY];

        Vector3 targetPosWorld = target.transform.position;
        if (target.gameObject != null) Destroy(target.gameObject);

        playerCard.transform.position = targetPosWorld;
        playerCard.gridX = target.gridX;
        playerCard.gridY = target.gridY;
        gridArray[target.gridX, target.gridY] = playerCard;
        playerPos = new Vector2Int(target.gridX, target.gridY);

        SpawnNewCardAt(oldX, oldY);
    }

    void SpawnNewCardAt(int x, int y)
    {
        float startX = -((currentLevel.gridWidth - 1) * spacingX) / 2;
        float startY = ((currentLevel.gridHeight - 1) * spacingY) / 2;
        Vector3 pos = new Vector3(startX + (x * spacingX), startY - (y * spacingY) + gridOffsetY, 0);

        GameObject newObj = Instantiate(cardPrefab, pos, Quaternion.identity, this.transform);
        CardController card = newObj.GetComponent<CardController>();
        card.SetupCard(x, y, GetWeightedRandomCard(), false);
        gridArray[x, y] = card;
    }

    CardData GetWeightedRandomCard()
    {
        bool spawnEnemy = Random.value > 0.4f;
        List<SpawnRate> pool = spawnEnemy ? currentLevel.enemies : currentLevel.items;

        if (pool == null || pool.Count == 0) pool = currentLevel.items;
        if (pool == null || pool.Count == 0) return null;

        float totalWeight = 0;
        foreach (var entry in pool) totalWeight += entry.weight;

        float randomPoint = Random.Range(0, totalWeight);
        float currentWeight = 0;
        foreach (var entry in pool)
        {
            currentWeight += entry.weight;
            if (randomPoint <= currentWeight) return entry.card;
        }
        return pool[0].card;
    }
}