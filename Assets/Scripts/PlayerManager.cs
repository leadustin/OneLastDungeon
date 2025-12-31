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

    [Header("Kampf Werte")]
    public int attackDamage = 10;
    public int defense = 2;

    [Header("Finanzen (in Kupfer)")]
    public int currentMoney = 0;

    [Header("Inventar")]
    public List<InventoryItem> inventory = new List<InventoryItem>();

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
        UpdateUI();
    }

    // --- 1. WÄHRUNGS SYSTEM ---

    public void AddMoney(int copperAmount)
    {
        currentMoney += copperAmount;
        UpdateUI();
    }

    public bool TrySpendMoney(int copperAmount)
    {
        if (currentMoney >= copperAmount)
        {
            currentMoney -= copperAmount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public static string FormatMoney(int totalCopper)
    {
        int gold = totalCopper / 10000;
        int silver = (totalCopper % 10000) / 100;
        int copper = totalCopper % 100;

        string result = "";
        if (gold > 0) result += $"{gold}g ";
        if (silver > 0) result += $"{silver}s ";
        if (copper > 0 || (gold == 0 && silver == 0)) result += $"{copper}c";

        return result.Trim();
    }

    public string GetFormattedMoney()
    {
        return FormatMoney(currentMoney);
    }

    // --- 2. SHOP & ITEMS ---

    public void BuyItem(CardData item)
    {
        if (item is PurchasableCardData buyable)
        {
            int cost = buyable.GetPriceInCopper();
            if (TrySpendMoney(cost))
            {
                AddItemToInventory(item);
            }
        }
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
    }

    // --- 3. INVENTAR LOGIK ---

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

    public void EquipGear(CardData newItem, CardData oldItem)
    {
        if (oldItem != null)
        {
            AddItemToInventory(oldItem);
            if (oldItem is WeaponData w) attackDamage -= w.damageAmount;
            else if (oldItem is ArmorData a) defense -= a.defenseAmount;
        }
        if (newItem != null)
        {
            RemoveItemFromInventory(newItem);
            if (newItem is WeaponData w) attackDamage += w.damageAmount;
            else if (newItem is ArmorData a) defense += a.defenseAmount;
        }
        UpdateUI();
    }

    public void UnequipGear(CardData item) { EquipGear(null, item); }

    // --- 4. GAMEPLAY & UI ---

    // DIESE METHODE HAT GEFEHLT
    public void TakeDamage(int amount)
    {
        // Rüstung abziehen (Verteidigung)
        int finalDamage = Mathf.Max(1, amount - defense);
        currentHealth -= finalDamage;

        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("GAME OVER!");
            // Hier könnte später ein Game Over Screen aufgerufen werden
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public void RefillMana(int amount)
    {
        currentMana = Mathf.Min(currentMana + amount, maxMana);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHP(currentHealth, maxHealth);
            PlayerHUD.Instance.UpdateMana(currentMana, maxMana);
            PlayerHUD.Instance.UpdateLevel(level);
            PlayerHUD.Instance.SetName(playerName);
        }

        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();

        if (healthText != null) healthText.text = $"HP: {currentHealth}/{maxHealth}";
        if (moneyText != null) moneyText.text = GetFormattedMoney();
    }
}

[System.Serializable]
public class InventoryItem
{
    public CardData data;
    public int stackSize;
    public InventoryItem(CardData d, int s) { data = d; stackSize = s; }
}