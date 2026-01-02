using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

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

    [Header("Finale Werte (Für Item-Vergleich)")]
    public int attackDamage;
    public int defense;

    [Header("Finanzen (in Kupfer)")]
    public long currentMoney = 0; // Auf long umgestellt für große Beträge

    [Header("Inventar")]
    public List<InventoryItem> inventory = new List<InventoryItem>();

    [Header("Ausrüstung (Aktuell getragen)")]
    public CardData headItem;
    public CardData chestItem;
    public CardData handsItem;
    public CardData legsItem;
    public CardData feetItem;
    public CardData neckItem;
    public CardData ring1Item;
    public CardData ring2Item;
    public CardData weaponItem;
    public CardData offhandItem;

    [Header("UI Referenzen")]
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
        CalculateStats();
        UpdateUI();
    }

    // --- NEU: KAMPF & SCHADEN ---
    public void TakeDamage(int amount)
    {
        // Berücksichtigt die Verteidigung beim Schaden
        int finalDamage = Mathf.Max(1, amount - defense);
        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        // Hier könnte später eine Game Over Logik hin
        if (currentHealth <= 0) Debug.Log("Spieler ist besiegt!");
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

    // --- NEU: ABLEGEN (UnequipGear) ---
    public void UnequipGear(CardData item)
    {
        if (item == null) return;

        bool removed = false;
        if (headItem == item) { headItem = null; removed = true; }
        else if (chestItem == item) { chestItem = null; removed = true; }
        else if (handsItem == item) { handsItem = null; removed = true; }
        else if (legsItem == item) { legsItem = null; removed = true; }
        else if (feetItem == item) { feetItem = null; removed = true; }
        else if (neckItem == item) { neckItem = null; removed = true; }
        else if (ring1Item == item) { ring1Item = null; removed = true; }
        else if (ring2Item == item) { ring2Item = null; removed = true; }
        else if (weaponItem == item) { weaponItem = null; removed = true; }
        else if (offhandItem == item) { offhandItem = null; removed = true; }

        if (removed)
        {
            AddItemToInventory(item);
            CalculateStats();
            UpdateUI();
        }
    }

    void SwapItem(ref CardData currentSlot, CardData newItem)
    {
        if (currentSlot != null) AddItemToInventory(currentSlot);
        RemoveItemFromInventory(newItem);
        currentSlot = newItem;
    }

    // --- FINANZEN & FORMATIERUNG ---
    public void AddMoney(long copperAmount)
    {
        currentMoney += copperAmount;
        UpdateUI();
    }

    public bool TrySpendMoney(long copperAmount)
    {
        if (currentMoney >= copperAmount)
        {
            currentMoney -= copperAmount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public static string FormatMoney(long totalCopper)
    {
        long gold = totalCopper / 10000;

        // Ab einer Million Gold: 1.x M Anzeige
        if (gold >= 1000000)
        {
            float millionGold = gold / 1000000f;
            return millionGold.ToString("F1") + "M <sprite name=\"coins_2\">";
        }

        int silver = (int)((totalCopper % 10000) / 100);
        int copper = (int)(totalCopper % 100);

        string result = "";
        if (gold > 0) result += $"{gold}<sprite name=\"coins_2\"> ";
        if (silver > 0) result += $"{silver}<sprite name=\"coins_1\"> ";
        if (copper > 0 || (gold == 0 && silver == 0))
            result += $"{copper}<sprite name=\"coins_0\">";

        return result.Trim();
    }

    public string GetFormattedMoney() => FormatMoney(currentMoney);

    // --- STATS ---
    void CalculateStats()
    {
        attackDamage = baseAttackDamage;
        defense = baseDefense;

        AddStatsFrom(headItem);
        AddStatsFrom(chestItem);
        AddStatsFrom(handsItem);
        AddStatsFrom(legsItem);
        AddStatsFrom(feetItem);
        AddStatsFrom(neckItem);
        AddStatsFrom(ring1Item);
        AddStatsFrom(ring2Item);
        AddStatsFrom(weaponItem);
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

    // --- INVENTAR ---
    public void AddItemToInventory(CardData item)
    {
        bool found = false;
        foreach (var stack in inventory)
        {
            if (stack.data == item) { stack.stackSize++; found = true; break; }
        }
        if (!found) inventory.Add(new InventoryItem(item, 1));
        UpdateUI();
    }

    public void RemoveItemFromInventory(CardData item)
    {
        InventoryItem toRemove = null;
        foreach (var stack in inventory)
        {
            if (stack.data == item)
            {
                stack.stackSize--;
                if (stack.stackSize <= 0) toRemove = stack;
                break;
            }
        }
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
        else if (item is ArmorData || item is WeaponData || item is AccessoryData)
        {
            EquipCard(item);
        }
    }

    public void UpdateUI()
    {
        if (healthText != null) healthText.text = $"HP: {currentHealth}/{maxHealth}";
        if (moneyText != null) moneyText.text = GetFormattedMoney();
        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();

        CharacterScreenUI charUI = Object.FindFirstObjectByType<CharacterScreenUI>();
        if (charUI != null && charUI.gameObject.activeSelf) charUI.UpdateVisuals();
    }

    public void Heal(int amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); UpdateUI(); }
    public void RefillMana(int amount) { currentMana = Mathf.Min(currentMana + amount, maxMana); UpdateUI(); }
}