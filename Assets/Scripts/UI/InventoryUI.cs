using UnityEngine;
using System.Collections.Generic;
using TMPro; // WICHTIG: Damit wir auf TextMeshPro zugreifen können

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Design Referenzen")]
    public Transform itemsParent;   // Dein 'Slots'-Grid
    public GameObject slotPrefab;   // Dein 'ItemSlot'-Prefab

    // --- NEU: Das Textfeld für dein Gold ---
    public TextMeshProUGUI coinText;
    // -------------------------------------

    [Header("Grid Einstellungen")]
    public int columns = 6;         // Spalten pro Reihe
    public int minSlots = 36;       // Mindestanzahl Slots

    [Header("Debug / Testen")]
    public CardData testItem;       // Zum Testen mit Taste 'K'

    void Awake()
    {
        Instance = this;
    }

    // Feuert jedes Mal, wenn das Fenster geöffnet wird
    void OnEnable()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        // Sicherheitscheck: Wenn wichtige Teile fehlen, abbrechen
        if (PlayerManager.Instance == null || itemsParent == null || slotPrefab == null) return;

        // --- TEIL 1: GELD AKTUALISIEREN ---
        // Wenn ein Textfeld zugewiesen ist, schreibe den formatierten Geldbetrag rein
        if (coinText != null)
        {
            coinText.text = PlayerManager.Instance.GetFormattedMoney();
        }

        // --- TEIL 2: ALTE SLOTS LÖSCHEN ---
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        // --- TEIL 3: NEUE SLOTS BERECHNEN ---
        List<InventoryItem> inventory = PlayerManager.Instance.inventory;
        int itemCount = inventory.Count;

        // Formel: Item-Anzahl durch Spalten teilen und aufrunden
        int rowsNeeded = Mathf.CeilToInt((float)itemCount / columns);
        int dynamicSlots = rowsNeeded * columns;

        // Nimm mindestens 36, oder mehr wenn nötig
        int totalSlotsToSpawn = Mathf.Max(minSlots, dynamicSlots);

        // --- TEIL 4: SLOTS ERSTELLEN ---
        for (int i = 0; i < totalSlotsToSpawn; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, itemsParent);
            InventorySlot slotScript = newSlotObj.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                if (i < itemCount)
                {
                    // Echter Inhalt aus dem Inventar
                    slotScript.SetupSlot(inventory[i]);
                }
                else
                {
                    // Leerer Platzhalter
                    slotScript.ClearSlot();
                }
            }
        }
    }

    // --- CHEAT TASTE (Nur zum Testen) ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (testItem != null && PlayerManager.Instance != null)
            {
                Debug.Log("Cheat: Füge Item hinzu: " + testItem.name);
                PlayerManager.Instance.AddItemToInventory(testItem);
                // Automatische Aktualisierung passiert durch AddItemToInventory -> UpdateUI
            }
        }
    }
}