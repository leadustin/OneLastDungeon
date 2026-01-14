using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleResultUI : MonoBehaviour
{
    [Header("UI Elemente")]
    public GameObject resultsPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI lootText;
    public Button continueButton;

    [Header("Szenen Navigation")]
    public string sceneToLoadOnContinue = "HubScene";

    [Header("Farben")]
    public Color winColor = Color.yellow;
    public Color loseColor = Color.red;

    private int pendingGold;
    private int pendingXP;
    private bool isWin;
    private List<DroppedItem> pendingItems;

    void Start()
    {
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
    }

    // UPDATE: Nimmt jetzt Liste von Items an
    public void ShowVictory(int gold, int xp, List<DroppedItem> items = null)
    {
        if (resultsPanel == null) return;

        isWin = true;
        pendingGold = gold;
        pendingXP = xp;
        pendingItems = items;

        resultsPanel.SetActive(true);
        if (titleText != null) { titleText.text = "SIEG!"; titleText.color = winColor; }

        // Text zusammenbauen
        string rewardText = $"BELOHNUNG:\n\n<color=#FFD700>+{gold} Gold</color>\n<color=#00FFFF>+{xp} XP</color>";

        // Items anzeigen?
        if (items != null && items.Count > 0)
        {
            rewardText += "\n\n<size=80%>ITEMS:</size>";
            foreach (var drop in items)
            {
                if (drop.item != null)
                {
                    rewardText += $"\n<color=white>{drop.amount}x {drop.item.itemName}</color>";
                }
            }
        }

        if (lootText != null) { lootText.text = rewardText; }
    }

    public void ShowDefeat()
    {
        if (resultsPanel == null) return;

        isWin = false;
        resultsPanel.SetActive(true);
        if (titleText != null) { titleText.text = "NIEDERLAGE"; titleText.color = loseColor; }
        if (lootText != null) { lootText.text = "Deine Helden sind gefallen..."; }
    }

    void OnContinuePressed()
    {
        // 1. Gold & XP verteilen (Dein alter Code)
        if (isWin && GameManager.Instance != null)
        {
            GameManager.Instance.AddRewards(pendingGold, pendingXP);
            if (BattlefieldManager.Instance.currentLevel != null)
            {
                GameManager.Instance.MarkLevelComplete(BattlefieldManager.Instance.currentLevel.levelID);
            }
        }

        // 2. NEU: Items ins Inventar legen!
        if (isWin && pendingItems != null && PlayerInventory.Instance != null)
        {
            foreach (var drop in pendingItems)
            {
                if (drop.item != null)
                {
                    // Wir erstellen ein echtes GameItem aus dem Template
                    GameItem newItem = new GameItem(drop.item);

                    // Wir setzen die Menge (falls Stackable)
                    newItem.stackCount = drop.amount;

                    // Ab in den Rucksack
                    PlayerInventory.Instance.AddItem(newItem);

                    Debug.Log($"RESULT: {drop.amount}x {drop.item.itemName} ins Inventar übertragen.");
                }
            }
        }

        // 3. Szene wechseln (Dein alter Code)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneToLoadOnContinue);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoadOnContinue);
        }
    }
}