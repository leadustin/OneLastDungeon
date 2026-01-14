using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryMenu : MonoBehaviour
{
    public static InventoryMenu Instance;

    [Header("Filter Konfiguration & UI")]
    public List<FilterDefinition> filters;
    public Transform filterBarTop;
    public Transform filterBarSide;
    public GameObject filterButtonPrefab;

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

    // Filter Status
    private ItemCategory currentCategory = ItemCategory.All;
    private ItemSubType currentSubType = ItemSubType.None;

    private List<UI_FilterButton> topButtons = new List<UI_FilterButton>();
    private List<UI_FilterButton> sideButtons = new List<UI_FilterButton>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadPartyData();
        GenerateMainFilterButtons();
        RefreshUI();
    }

    void LoadPartyData()
    {
        if (PartyManager.Instance == null || PartyManager.Instance.activeParty.Count == 0) return;
        selectedHero = PartyManager.Instance.activeParty[currentHeroIndex];
    }

    public void RefreshUI()
    {
        if (selectedHero == null) return;

        if (heroNameText != null) heroNameText.text = $"{selectedHero.heroName} (Lvl {selectedHero.currentLevel})";
        UpdateStatsDisplay();

        foreach (var slotUI in equipmentSlots)
        {
            slotUI.Setup(selectedHero, this);
        }

        UpdateInventoryGrid();
    }

    void UpdateStatsDisplay()
    {
        if (statsText == null) return;
        string s = "";
        s += $"HP: {selectedHero.GetTotalStat(StatType.MaxHealth)}\n";
        s += $"Dmg: {selectedHero.GetTotalStat(StatType.PhysicalDamage)}\n";
        // FIX: Defense -> Armor
        s += $"Armor: {selectedHero.GetTotalStat(StatType.Armor)}\n";
        // Optional: Hier könnten auch Resistenzen angezeigt werden
        statsText.text = s;
    }

    // --- Filter-Logik ---
    void UpdateInventoryGrid()
    {
        if (inventoryGridContent == null || itemSlotPrefab == null || PlayerInventory.Instance == null) return;

        foreach (Transform child in inventoryGridContent) Destroy(child.gameObject);

        List<GameItem> allItems = PlayerInventory.Instance.items;
        List<GameItem> filteredItems = new List<GameItem>();

        foreach (var item in allItems)
        {
            if (currentCategory != ItemCategory.All && item.template.category != currentCategory) continue;
            if (currentSubType != ItemSubType.None && item.template.subType != currentSubType) continue;
            filteredItems.Add(item);
        }

        int totalSlots = Mathf.Max(filteredItems.Count, minSlotsToDraw);

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject go = Instantiate(itemSlotPrefab, inventoryGridContent);
            UI_InventorySlot slotScript = go.GetComponent<UI_InventorySlot>();

            if (slotScript != null)
            {
                if (i < filteredItems.Count) slotScript.Setup(filteredItems[i], this);
                else slotScript.Setup(null, this);
            }
        }
    }

    // --- Filter Buttons ---

    void GenerateMainFilterButtons()
    {
        foreach (Transform child in filterBarTop) Destroy(child.gameObject);
        topButtons.Clear();

        if (filters == null) return;

        foreach (var filterDef in filters)
        {
            if (filterButtonPrefab == null) continue;
            GameObject go = Instantiate(filterButtonPrefab, filterBarTop);
            UI_FilterButton btnScript = go.GetComponent<UI_FilterButton>();

            bool isSelected = (filterDef.category == currentCategory);
            btnScript.Setup(filterDef.icon, isSelected, () => OnMainFilterClicked(filterDef));
            topButtons.Add(btnScript);
        }
    }

    void OnMainFilterClicked(FilterDefinition filterDef)
    {
        currentCategory = filterDef.category;
        currentSubType = ItemSubType.None;

        foreach (var btn in topButtons) btn.SetSelected(false);
        int index = filters.IndexOf(filterDef);
        if (index >= 0 && index < topButtons.Count) topButtons[index].SetSelected(true);

        GenerateSubFilterButtons(filterDef);
        UpdateInventoryGrid();
    }

    void GenerateSubFilterButtons(FilterDefinition filterDef)
    {
        foreach (Transform child in filterBarSide) Destroy(child.gameObject);
        sideButtons.Clear();

        if (filterDef.subFilters == null || filterDef.subFilters.Count == 0) return;

        foreach (var subDef in filterDef.subFilters)
        {
            GameObject go = Instantiate(filterButtonPrefab, filterBarSide);
            UI_FilterButton btnScript = go.GetComponent<UI_FilterButton>();

            bool isSelected = (subDef.subType == currentSubType);
            btnScript.Setup(subDef.icon, isSelected, () =>
            {
                currentSubType = subDef.subType;
                foreach (var b in sideButtons) b.SetSelected(false);
                btnScript.SetSelected(true);
                UpdateInventoryGrid();
            });
            sideButtons.Add(btnScript);
        }
    }

    public void OnItemClicked(GameItem clickedItem)
    {
        if (clickedItem == null) return;
        GameItem oldItem = null;

        if (clickedItem.IsEquipment())
        {
            EquipmentTemplate template = clickedItem.template as EquipmentTemplate;
            if (selectedHero != null && selectedHero.equipment.ContainsKey(template.slot))
            {
                oldItem = selectedHero.equipment[template.slot];
            }
        }

        if (comparisonWindow != null) comparisonWindow.OpenComparison(clickedItem, oldItem, this);
        else TryEquipItem(clickedItem);
    }

    public void TryEquipItem(GameItem itemToEquip)
    {
        if (selectedHero == null || !PlayerInventory.Instance.items.Contains(itemToEquip)) return;
        if (!itemToEquip.IsEquipment()) return;

        EquipmentSlot slotType = (itemToEquip.template as EquipmentTemplate).slot;
        List<GameItem> inventoryList = PlayerInventory.Instance.items;
        int slotIndex = inventoryList.IndexOf(itemToEquip);

        GameItem oldItem = null;
        if (selectedHero.equipment.ContainsKey(slotType)) oldItem = selectedHero.equipment[slotType];

        GameItem itemForHero = itemToEquip;
        if (itemToEquip.stackCount > 1)
        {
            itemToEquip.stackCount--;
            itemForHero = new GameItem(itemToEquip.template);
            itemForHero.stackCount = 1;
            slotIndex = -1;
        }
        else inventoryList.Remove(itemToEquip);

        if (oldItem != null) selectedHero.UnequipItem(slotType);
        selectedHero.EquipItem(itemForHero, slotType);

        if (oldItem != null)
        {
            if (!inventoryList.Contains(oldItem))
            {
                if (slotIndex != -1 && slotIndex <= inventoryList.Count) inventoryList.Insert(slotIndex, oldItem);
                else PlayerInventory.Instance.AddItem(oldItem);
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
            if (!PlayerInventory.Instance.items.Contains(itemToUnequip)) PlayerInventory.Instance.AddItem(itemToUnequip);
        }
        RefreshUI();
        if (comparisonWindow != null) comparisonWindow.CloseWindow();
    }
}

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