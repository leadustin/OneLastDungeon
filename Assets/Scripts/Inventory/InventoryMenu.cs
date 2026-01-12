using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryMenu : MonoBehaviour
{
    public static InventoryMenu Instance;

    [Header("Filter Konfiguration & UI")]
    public List<FilterDefinition> filters;
    public Transform filterBarTop;   // ZIEH HIER "FilterBar_Top" REIN
    public Transform filterBarSide;  // ZIEH HIER "FilterBar_Side" REIN
    public GameObject filterButtonPrefab; // ZIEH HIER DEIN "UI_FilterButton" PREFAB REIN

    [Header("Referenzen - Grid (Rucksack)")]
    public Transform inventoryGridContent;
    public GameObject itemSlotPrefab;
    public int minSlotsToDraw = 36;

    [Header("Referenzen - Charakter (Links)")]
    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI statsText;
    public List<EquipmentSlotUI> equipmentSlots;

    [Header("Referenzen - Popups")]
    public UI_ComparisonWindow comparisonWindow;

    // --- Runtime Status ---
    public HeroRuntimeData selectedHero;
    private int currentHeroIndex = 0;

    // Filter Status (Welcher Tab ist gerade aktiv?)
    private ItemCategory currentCategory = ItemCategory.All;
    private ItemSubType currentSubType = ItemSubType.None;

    // Listen für die Button-Instanzen (um sie später zu färben/löschen)
    private List<UI_FilterButton> topButtons = new List<UI_FilterButton>();
    private List<UI_FilterButton> sideButtons = new List<UI_FilterButton>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("DIAGNOSE: InventoryMenu gestartet.");
        LoadPartyData();

        // Buttons einmalig beim Start erstellen (basierend auf deiner Liste)
        GenerateMainFilterButtons();

        RefreshUI();
    }

    void LoadPartyData()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogError("DIAGNOSE FEHLER: PartyManager.Instance ist NULL! Ist der PartyManager in der Szene?");
            return;
        }

        if (PartyManager.Instance.activeParty.Count > 0)
        {
            selectedHero = PartyManager.Instance.activeParty[currentHeroIndex];
        }
        else
        {
            Debug.LogError("DIAGNOSE FEHLER: PartyManager hat KEINE Helden in 'activeParty'!");
        }
    }

    public void RefreshUI()
    {
        if (selectedHero == null) return;

        // 1. Text Infos
        if (heroNameText != null) heroNameText.text = $"{selectedHero.heroName} (Lvl {selectedHero.currentLevel})";
        UpdateStatsDisplay();

        // 2. Ausrüstung
        foreach (var slotUI in equipmentSlots)
        {
            slotUI.Setup(selectedHero, this);
        }

        // 3. Inventar (jetzt gefiltert)
        UpdateInventoryGrid();
    }

    void UpdateStatsDisplay()
    {
        if (statsText == null) return;
        string s = "";
        s += $"HP: {selectedHero.GetTotalStat(StatType.MaxHealth)}\n";
        s += $"Dmg: {selectedHero.GetTotalStat(StatType.Damage)}\n";
        s += $"Def: {selectedHero.GetTotalStat(StatType.Defense)}\n";
        statsText.text = s;
    }

    // --- ANGEPASST: Filter-Logik integriert ---
    void UpdateInventoryGrid()
    {
        if (inventoryGridContent == null || itemSlotPrefab == null || PlayerInventory.Instance == null) return;

        // Alte Slots löschen
        foreach (Transform child in inventoryGridContent) Destroy(child.gameObject);

        List<GameItem> allItems = PlayerInventory.Instance.items;

        // FILTERUNG: Wir erstellen eine temporäre Liste nur mit passenden Items
        List<GameItem> filteredItems = new List<GameItem>();

        foreach (var item in allItems)
        {
            // A. Hauptkategorie prüfen
            // Wenn "All" gewählt ist, zeigen wir alles. Sonst muss Category exakt passen.
            if (currentCategory != ItemCategory.All && item.template.category != currentCategory)
            {
                continue; // Passt nicht -> Raus
            }

            // B. Unterkategorie prüfen
            // Nur prüfen, wenn ein SubType aktiv ist (None = Zeige alles in der Kategorie)
            if (currentSubType != ItemSubType.None && item.template.subType != currentSubType)
            {
                continue; // Passt nicht -> Raus
            }

            // Wenn wir hier sind, passt das Item!
            filteredItems.Add(item);
        }

        // Grid zeichnen (mit den gefilterten Items)
        int totalSlots = Mathf.Max(filteredItems.Count, minSlotsToDraw);

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(itemSlotPrefab, inventoryGridContent);
            UI_InventorySlot slotScript = go.GetComponent<UI_InventorySlot>();

            if (slotScript != null)
            {
                // WICHTIG: filteredItems statt allItems nutzen!
                if (i < filteredItems.Count) slotScript.Setup(filteredItems[i], this);
                else slotScript.Setup(null, this);
            }
        }
    }

    // ------------------------------------------------------------------------
    // NEU: Filter Button Logik
    // ------------------------------------------------------------------------

    void GenerateMainFilterButtons()
    {
        // Aufräumen
        foreach (Transform child in filterBarTop) Destroy(child.gameObject);
        topButtons.Clear();

        if (filters == null) return;

        foreach (var filterDef in filters)
        {
            if (filterButtonPrefab == null) continue;

            GameObject go = Instantiate(filterButtonPrefab, filterBarTop);
            UI_FilterButton btnScript = go.GetComponent<UI_FilterButton>();

            // Button konfigurieren
            // Wenn geklickt -> OnMainFilterClicked rufen
            bool isSelected = (filterDef.category == currentCategory);
            btnScript.Setup(filterDef.icon, isSelected, () => OnMainFilterClicked(filterDef));

            topButtons.Add(btnScript);
        }
    }

    void OnMainFilterClicked(FilterDefinition filterDef)
    {
        // 1. Logischen Zustand ändern
        currentCategory = filterDef.category;
        currentSubType = ItemSubType.None; // Reset Subfilter beim Tab-Wechsel

        // 2. Visuelles Feedback (Welcher Tab ist aktiv?)
        foreach (var btn in topButtons) btn.SetSelected(false);

        // Den Button finden, der zu dieser Definition gehört (via Index)
        int index = filters.IndexOf(filterDef);
        if (index >= 0 && index < topButtons.Count)
        {
            topButtons[index].SetSelected(true);
        }

        // 3. Linke Leiste neu aufbauen
        GenerateSubFilterButtons(filterDef);

        // 4. Grid neu zeichnen
        UpdateInventoryGrid();
    }

    void GenerateSubFilterButtons(FilterDefinition filterDef)
    {
        // Aufräumen
        foreach (Transform child in filterBarSide) Destroy(child.gameObject);
        sideButtons.Clear();

        // Abbruch, wenn keine Subfilter definiert sind (z.B. bei "Alle")
        if (filterDef.subFilters == null || filterDef.subFilters.Count == 0) return;

        foreach (var subDef in filterDef.subFilters)
        {
            GameObject go = Instantiate(filterButtonPrefab, filterBarSide);
            UI_FilterButton btnScript = go.GetComponent<UI_FilterButton>();

            bool isSelected = (subDef.subType == currentSubType);

            btnScript.Setup(subDef.icon, isSelected, () =>
            {
                // Subfilter Klick-Logik
                currentSubType = subDef.subType;

                // Visuals updaten (Nur in der linken Leiste)
                foreach (var b in sideButtons) b.SetSelected(false);
                btnScript.SetSelected(true);

                // Grid neu zeichnen
                UpdateInventoryGrid();
            });

            sideButtons.Add(btnScript);
        }
    }

    // Wird aufgerufen, wenn man auf ein Item im Grid klickt
    public void OnItemClicked(GameItem clickedItem)
    {
        if (clickedItem == null) return;

        // 1. Suche nach einem alten Item (zum Vergleichen)
        GameItem oldItem = null;

        // Ist es Ausrüstung?
        if (clickedItem.IsEquipment())
        {
            EquipmentTemplate template = clickedItem.template as EquipmentTemplate;

            // Trägt der Held schon was in diesem Slot?
            if (selectedHero != null && selectedHero.equipment.ContainsKey(template.slot))
            {
                oldItem = selectedHero.equipment[template.slot];
            }
        }

        // 2. Öffne das Vergleichsfenster
        if (comparisonWindow != null)
        {
            comparisonWindow.OpenComparison(clickedItem, oldItem, this);
        }
        else
        {
            // Fallback, falls Fenster vergessen wurde: Direkt anziehen
            TryEquipItem(clickedItem);
        }
    }

    // --- Stapel Logik & Equip (Deine bestehende, sichere Logik) ---
    public void TryEquipItem(GameItem itemToEquip)
    {
        if (selectedHero == null || !PlayerInventory.Instance.items.Contains(itemToEquip)) return;
        if (!itemToEquip.IsEquipment()) return;

        EquipmentSlot slotType = (itemToEquip.template as EquipmentTemplate).slot;
        List<GameItem> inventoryList = PlayerInventory.Instance.items;

        int slotIndex = inventoryList.IndexOf(itemToEquip);

        GameItem oldItem = null;
        if (selectedHero.equipment.ContainsKey(slotType))
        {
            oldItem = selectedHero.equipment[slotType];
        }

        GameItem itemForHero = itemToEquip;

        if (itemToEquip.stackCount > 1)
        {
            itemToEquip.stackCount--;
            itemForHero = new GameItem(itemToEquip.template);
            itemForHero.stackCount = 1;
            slotIndex = -1;
        }
        else
        {
            inventoryList.Remove(itemToEquip);
        }

        if (oldItem != null)
        {
            selectedHero.UnequipItem(slotType);
        }

        selectedHero.EquipItem(itemForHero, slotType);

        if (oldItem != null)
        {
            if (!inventoryList.Contains(oldItem))
            {
                if (slotIndex != -1 && slotIndex <= inventoryList.Count)
                {
                    inventoryList.Insert(slotIndex, oldItem);
                }
                else
                {
                    PlayerInventory.Instance.AddItem(oldItem);
                }
                Debug.Log("TAUSCH: Altes Item zurück in den Rucksack gelegt.");
            }
            else
            {
                Debug.LogWarning("ACHTUNG: Das alte Item war schon im Inventar!");
            }
        }

        RefreshUI();
        if (comparisonWindow != null) comparisonWindow.CloseWindow();
    }

    public void TryUnequipItem(EquipmentSlot slot)
    {
        if (selectedHero == null) return;

        if (selectedHero.equipment.ContainsKey(slot))
        {
            GameItem itemToUnequip = selectedHero.equipment[slot];

            selectedHero.UnequipItem(slot);

            if (!PlayerInventory.Instance.items.Contains(itemToUnequip))
            {
                PlayerInventory.Instance.AddItem(itemToUnequip);
                Debug.Log($"FIX: InventoryMenu hat {itemToUnequip.template.itemName} manuell zurückgelegt.");
            }
            else
            {
                Debug.Log($"FIX: Das Item wurde bereits durch UnequipItem zurückgelegt. Keine Aktion nötig.");
            }
        }

        RefreshUI();
        if (comparisonWindow != null) comparisonWindow.CloseWindow();
    }
}

// Hilfsklassen müssen außerhalb der Klasse stehen
[System.Serializable]
public class FilterDefinition
{
    public string name;
    public ItemCategory category;
    public Sprite icon;
    public List<SubFilterDefinition> subFilters;
}

[System.Serializable]
public class SubFilterDefinition
{
    public string name;
    public ItemSubType subType;
    public Sprite icon;
}