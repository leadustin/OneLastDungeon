using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("UI Haupt-Panel (Kombi)")]
    public GameObject mainUIPanel;

    [Header("Spieler Werte")]
    public string playerName = "Held";
    public float maxHealth = 100;
    public float currentHealth;
    public float maxMana = 50;
    public float currentMana;
    public int level = 1;

    [Header("Basis Stats")]
    public int baseAttackDamage = 10;
    public int baseDefense = 2;

    [Header("Finale Werte")]
    public int attackDamage;
    public int defense;

    [Header("Finanzen")]
    public long currentMoney = 0;

    [Header("Inventar & Ausrüstung")]
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public CardData headItem, chestItem, handsItem, legsItem, feetItem, neckItem, ring1Item, ring2Item, weaponItem, offhandItem;

    [Header("UI Referenzen (Optional)")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this.gameObject);
        else Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;

        if (mainUIPanel != null) mainUIPanel.SetActive(false);

        CalculateStats();
        UpdateUI();
    }

    public void ToggleMainUI()
    {
        if (mainUIPanel != null)
        {
            bool isActive = !mainUIPanel.activeSelf;
            mainUIPanel.SetActive(isActive);
            if (isActive) UpdateUI();
        }
    }

    // --- LOGIK ---
    public void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(1, amount - defense);
        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
    }

    public void AddMoney(long amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    public bool TrySpendMoney(long amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    // --- AUSRÜSTUNG ---
    public void EquipCard(CardData newItem)
    {
        EquipmentType type = EquipmentType.Head;
        if (newItem is ArmorData armor) type = armor.equipmentType;
        else if (newItem is WeaponData weapon) type = weapon.equipmentType;
        else if (newItem is AccessoryData acc) type = acc.equipmentType;
        else return;

        switch (type)
        {
            case EquipmentType.Head: SwapItem(ref headItem, newItem); break;
            case EquipmentType.Chest: SwapItem(ref chestItem, newItem); break;
            case EquipmentType.Hands: SwapItem(ref handsItem, newItem); break;
            case EquipmentType.Legs: SwapItem(ref legsItem, newItem); break;
            case EquipmentType.Feet: SwapItem(ref feetItem, newItem); break;
            case EquipmentType.Neck: SwapItem(ref neckItem, newItem); break;
            case EquipmentType.Weapon: SwapItem(ref weaponItem, newItem); break;
            case EquipmentType.Offhand: SwapItem(ref offhandItem, newItem); break;
            case EquipmentType.Ring:
                if (ring1Item != null && ring2Item == null) SwapItem(ref ring2Item, newItem);
                else SwapItem(ref ring1Item, newItem);
                break;
        }
        CalculateStats();
        UpdateUI();
    }

    public void UnequipGear(CardData item)
    {
        if (item == null) return;
        if (headItem == item) headItem = null;
        else if (chestItem == item) chestItem = null;
        else if (handsItem == item) handsItem = null;
        else if (legsItem == item) legsItem = null;
        else if (feetItem == item) feetItem = null;
        else if (neckItem == item) neckItem = null;
        else if (ring1Item == item) ring1Item = null;
        else if (ring2Item == item) ring2Item = null;
        else if (weaponItem == item) weaponItem = null;
        else if (offhandItem == item) offhandItem = null;

        AddItemToInventory(item);
        CalculateStats();
        UpdateUI();
    }

    void SwapItem(ref CardData currentSlot, CardData newItem)
    {
        if (currentSlot != null) AddItemToInventory(currentSlot);
        RemoveItemFromInventory(newItem);
        currentSlot = newItem;
    }

    // --- STATS BERECHNUNG ---
    void CalculateStats()
    {
        attackDamage = baseAttackDamage;
        defense = baseDefense;

        AddStatsFrom(headItem); AddStatsFrom(chestItem); AddStatsFrom(handsItem);
        AddStatsFrom(legsItem); AddStatsFrom(feetItem); AddStatsFrom(neckItem);
        AddStatsFrom(ring1Item); AddStatsFrom(ring2Item); AddStatsFrom(weaponItem);
        AddStatsFrom(offhandItem);
    }

    void AddStatsFrom(CardData item)
    {
        if (item == null) return;
        if (item is ArmorData a) defense += a.defenseAmount;
        if (item is WeaponData w) attackDamage += w.damageAmount;
        if (item is AccessoryData acc)
        {
            defense += acc.bonusDefense;
            attackDamage += acc.bonusDamage;
        }
    }

    // --- INVENTAR VERWALTUNG ---

    public void AddItemToInventory(CardData item)
    {
        bool found = false;
        foreach (var stack in inventory) { if (stack.data == item) { stack.stackSize++; found = true; break; } }
        if (!found) inventory.Add(new InventoryItem(item, 1));
        UpdateUI();
    }

    public void RemoveItemFromInventory(CardData item)
    {
        InventoryItem toRemove = null;
        foreach (var stack in inventory) { if (stack.data == item) { stack.stackSize--; if (stack.stackSize <= 0) toRemove = stack; break; } }
        if (toRemove != null) inventory.Remove(toRemove);
        UpdateUI();
    }

    public void UseItem(CardData item)
    {
        if (item is ConsumableData potion)
        {
            int amount = Random.Range(potion.minAmount, potion.maxAmount + 1);
            if (potion.potionType == PotionType.Health) Heal(amount);
            else if (potion.potionType == PotionType.Mana) RefillMana(amount);
            RemoveItemFromInventory(item);
        }
        else if (item is ArmorData || item is WeaponData || item is AccessoryData) EquipCard(item);
    }

    // --- HELPER & FORMATTING ---

    // HIER WAR DER FEHLER: Jetzt wieder mit Sprites!
    public static string FormatMoney(long totalCopper)
    {
        long gold = totalCopper / 10000;
        // Bei sehr großen Beträgen abkürzen (z.B. 1.5M Gold)
        if (gold >= 1000000) return (gold / 1000000f).ToString("F1") + "M <sprite name=\"coins_2\">";

        int silver = (int)((totalCopper % 10000) / 100);
        int copper = (int)(totalCopper % 100);

        string result = "";

        // Nur anzeigen, was auch da ist (außer bei 0, da zeigen wir Kupfer)
        if (gold > 0) result += $"{gold}<sprite name=\"coins_2\"> ";
        if (silver > 0) result += $"{silver}<sprite name=\"coins_1\"> ";
        if (copper > 0 || (gold == 0 && silver == 0)) result += $"{copper}<sprite name=\"coins_0\">";

        return result.Trim();
    }

    public string GetFormattedMoney() => FormatMoney(currentMoney);
    public void Heal(int amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); UpdateUI(); }
    public void RefillMana(int amount) { currentMana = Mathf.Min(currentMana + amount, maxMana); UpdateUI(); }


    // --- UI UPDATES ---
    public void UpdateUI()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHP(currentHealth, maxHealth);
            PlayerHUD.Instance.UpdateMana(currentMana, maxMana);
            PlayerHUD.Instance.UpdateLevel(level);
            PlayerHUD.Instance.SetName(playerName);
        }

        if (mainUIPanel != null && mainUIPanel.activeInHierarchy)
        {
            if (healthText != null) healthText.text = $"HP: {(int)currentHealth}/{(int)maxHealth}";
            if (moneyText != null) moneyText.text = GetFormattedMoney();

            if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();

            CharacterScreenUI charUI = mainUIPanel.GetComponentInChildren<CharacterScreenUI>(true);
            if (charUI != null) charUI.UpdateVisuals();
        }
    }

    public CardData GetEquippedItem(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Head: return headItem;
            case EquipmentType.Chest: return chestItem;
            case EquipmentType.Hands: return handsItem;
            case EquipmentType.Legs: return legsItem;
            case EquipmentType.Feet: return feetItem;
            case EquipmentType.Neck: return neckItem;
            case EquipmentType.Weapon: return weaponItem;
            case EquipmentType.Offhand: return offhandItem;
            case EquipmentType.Ring: return ring1Item;
            default: return null;
        }
    }

    public void RequestEquipItem(CardData item)
    {
        if (item is ConsumableData) UseItem(item);
        else
        {
            if (ItemComparisonUI.Instance != null) ItemComparisonUI.Instance.OpenComparison(item);
            else EquipCard(item);
        }
    }
}