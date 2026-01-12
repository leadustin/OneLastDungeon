using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    [Header("UI Haupt-Panel (Kombi)")]
    public GameObject mainUIPanel;

    [Header("DEBUG: Start-Klasse hier reinziehen zum Testen")]
    public CharacterClassData debugStartClass;

    [Header("Spieler Werte")]
    public string playerName = "Held";
    public float maxHealth = 100;
    public float currentHealth;
    public float maxMana = 50;
    public float currentMana;
    public int level = 1;

    [Header("Aktive Effekte")]
    public List<ActiveStatusEffect> currentStatusEffects = new List<ActiveStatusEffect>();

    [Header("Level & Erfahrung")]
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public float xpScaleFactor = 1.5f;
    public int skillPoints = 0;

    [Header("Session Stats")]
    public long sessionMoney = 0;
    public int sessionExp = 0;

    [Header("Skills & Magie")]
    // Die Liste der Skills, die der Spieler aktuell beherrscht
    public List<SkillData> learnedSkills = new List<SkillData>();
    // Speichert, wann ein Skill zuletzt genutzt wurde: "Feuerball" -> Zeitstempel
    private Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();

    [Header("Basis Stats (Kommen aus der Klasse)")]
    public int baseMinDamage = 2;
    public int baseMaxDamage = 4;
    public int baseDefense = 0;
    public float baseCritChance = 5.0f;
    public float baseDodgeChance = 0.0f;
    public float baseBlockChance = 0.0f;

    [Header("Finale Werte (Berechnet)")]
    public int minAttackDamage;
    public int maxAttackDamage;
    public int defense;
    public float critChance;
    public float critDamageBonus;
    public float dodgeChance;
    public float blockChance;

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
        if (GameManager.Instance != null && GameManager.Instance.selectedClass != null)
        {
            Initialize(GameManager.Instance.selectedClass);
        }
        else if (debugStartClass != null)
        {
            Initialize(debugStartClass);
        }
        else
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            CalculateStats();
        }

        if (mainUIPanel != null) mainUIPanel.SetActive(false);
        UpdateUI();
    }

    // --- INITIALISIERUNG ---
    public void Initialize(CharacterClassData classData)
    {
        if (classData == null) return;

        Debug.Log($"Initialisiere Klasse: {classData.className}");

        // 1. Basis-Werte & Name laden
        playerName = PlayerPrefs.GetString("PlayerName", classData.className);
        string loadedRace = PlayerPrefs.GetString("PlayerRace", "Human");
        string loadedGender = PlayerPrefs.GetString("PlayerGender", "Female");

        maxHealth = classData.maxHealth;
        currentHealth = maxHealth;
        maxMana = classData.maxMana;
        currentMana = maxMana;

        baseDefense = classData.baseDefense;
        baseMinDamage = classData.baseMinDamage;
        baseMaxDamage = classData.baseMaxDamage;

        baseCritChance = classData.baseCritChance;
        baseDodgeChance = classData.baseDodgeChance;
        baseBlockChance = classData.baseBlockChance;

        // 2. Inventar & Ausrüstung leeren
        inventory.Clear();
        UnequipAll();

        // 3. Start-Items anziehen
        if (classData.startingEquipment != null)
        {
            foreach (var item in classData.startingEquipment)
            {
                if (item != null) EquipCard(item);
            }
        }

        // 4. Skills lernen (NEU!)
        learnedSkills.Clear();
        skillCooldowns.Clear();
        if (classData.startingSkills != null)
        {
            foreach (var skill in classData.startingSkills)
            {
                if (skill != null) learnedSkills.Add(skill);
            }
        }

        CalculateStats();
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

    // --- SKILL SYSTEM (MODULAR / BAUKASTEN) ---

    public void TryUseSkill(SkillData skill)
    {
        if (skill == null) return;

        // 1. Cooldown prüfen
        if (IsSkillOnCooldown(skill))
        {
            Debug.Log($"Skill '{skill.skillName}' ist noch nicht bereit!");
            return;
        }


        // 2. Cooldown setzen
        skillCooldowns[skill.skillName] = Time.time;
        UpdateUI();

        // 3. Effekte abfeuern
        if (skill.skillSteps != null) // War früher 'effects'
        {
            foreach (var step in skill.skillSteps)
            {
                if (step.effectLogic != null)
                {
                    // Hier übergeben wir den Caster UND die Daten aus dem Schritt!
                    step.effectLogic.Activate(this.gameObject, step);
                }
            }
        }

        if (skill.visualEffectPrefab != null)
        {
            Instantiate(skill.visualEffectPrefab, this.transform.position, Quaternion.identity);
        }
    }

    // Hilfsfunktion: Wie viel % Cooldown ist noch übrig? (0.0 bis 1.0)
    public float GetSkillCooldownRatio(SkillData skill)
    {
        if (skill == null || !skillCooldowns.ContainsKey(skill.skillName)) return 0f;

        float lastUsed = skillCooldowns[skill.skillName];
        float timePassed = Time.time - lastUsed;

        if (timePassed >= skill.cooldownTime) return 0f; // Fertig

        // Berechne Prozentwert für das Overlay (z.B. 0.5 = halb voll)
        return 1f - (timePassed / skill.cooldownTime);
    }

    public bool IsSkillOnCooldown(SkillData skill)
    {
        return GetSkillCooldownRatio(skill) > 0;
    }


    // --- LEVEL SYSTEM ---

    public void AddExperience(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        level++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScaleFactor);

        float healthIncrease = maxHealth * 0.1f;
        maxHealth += Mathf.RoundToInt(healthIncrease);
        currentHealth = maxHealth;

        maxMana += 5;
        currentMana = maxMana;

        baseMinDamage += 1;
        baseMaxDamage += 2;
        skillPoints++;

        Debug.Log($"LEVEL UP! Level {level}. HP: {maxHealth}");
        CalculateStats();
        UpdateUI();
    }

    // --- STATUS SYSTEM: Methode zum Hinzufügen ---
    public void AddStatusEffect(ActiveStatusEffect effect)
    {
        // Einfaches Hinzufügen zur Liste
        currentStatusEffects.Add(effect);

        // Optional: Visuelles Feedback (Text über dem Spieler)
        if (GridManager.Instance != null && FloatingTextManager.Instance != null)
        {
            Vector3 pos = GridManager.Instance.GetPlayerCardPosition();
            // Zeigt den Namen des Effekts (z.B. "Regeneration") in der Skill-Farbe an
            FloatingTextManager.Instance.Show(CombatTextType.SkillName, effect.effectName, pos + Vector3.up);
        }

        Debug.Log($"Status '{effect.effectName}' auf Spieler angewendet ({effect.remainingRounds} Runden).");
    }

    // Diese Funktion ruft der CombatManager gleich zu Beginn jeder Runde auf
    public void ProcessStatusEffects()
    {
        // Wir gehen rückwärts durch die Liste, damit wir sicher löschen können
        for (int i = currentStatusEffects.Count - 1; i >= 0; i--)
        {
            ActiveStatusEffect effect = currentStatusEffects[i];

            // 1. Effekt ausführen
            if (effect.type == StatusType.HealOverTime)
            {
                Heal(effect.amountPerRound);
                // Grüner Text über dem Spieler (nutzt GridManager Position)
                Vector3 pos = Vector3.zero;
                if (GridManager.Instance != null) pos = GridManager.Instance.GetPlayerCardPosition();

                FloatingTextManager.Instance.Show(CombatTextType.Heal, effect.amountPerRound.ToString(), pos + Vector3.up);
            }
            // (Hier könnte man auch Self-Damage hinzufügen)

            // 2. Zeit abziehen
            effect.remainingRounds--;

            // 3. Wenn abgelaufen, löschen
            if (effect.remainingRounds <= 0)
            {
                currentStatusEffects.RemoveAt(i);
            }
        }
    }

    // Reset bei Tod oder Kampfende (optional)
    public void ClearStatusEffects()
    {
        currentStatusEffects.Clear();
    }

    // --- KAMPF LOGIK ---

    public int GetAttackDamageRoll()
    {
        return Random.Range(minAttackDamage, maxAttackDamage + 1);
    }

    public void TakeDamage(int amount)
    {
        int finalDamage = Mathf.Max(1, amount - defense);
        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
        if (currentHealth <= 0) Debug.Log("Spieler ist besiegt!");
    }

    public void ResetSessionStats()
    {
        sessionMoney = 0;
        sessionExp = 0;
    }

    public void AddExp(int amount)
    {
        // currentExp += amount; 
        sessionExp += amount;
    }

    public void AddMoney(long amount)
    {
        currentMoney += amount;
        sessionMoney += amount;

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

    void UnequipAll()
    {
        headItem = null; chestItem = null; handsItem = null;
        legsItem = null; feetItem = null; neckItem = null;
        ring1Item = null; ring2Item = null; weaponItem = null; offhandItem = null;
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
        minAttackDamage = baseMinDamage;
        maxAttackDamage = baseMaxDamage;
        defense = baseDefense;
        critChance = baseCritChance;
        critDamageBonus = 0.0f;
        dodgeChance = baseDodgeChance;
        blockChance = baseBlockChance;

        AddStatsFrom(headItem); AddStatsFrom(chestItem); AddStatsFrom(handsItem);
        AddStatsFrom(legsItem); AddStatsFrom(feetItem); AddStatsFrom(neckItem);
        AddStatsFrom(ring1Item); AddStatsFrom(ring2Item); AddStatsFrom(weaponItem);
        AddStatsFrom(offhandItem);

        CalculateSetBonuses();
    }

    void CalculateSetBonuses()
    {
        List<CardData> equipped = new List<CardData> {
            headItem, chestItem, handsItem, legsItem, feetItem,
            neckItem, ring1Item, ring2Item, weaponItem, offhandItem
        };

        Dictionary<ItemSetData, int> setCounts = new Dictionary<ItemSetData, int>();

        foreach (var item in equipped)
        {
            if (item != null && item.itemSet != null)
            {
                if (!setCounts.ContainsKey(item.itemSet)) setCounts[item.itemSet] = 0;
                setCounts[item.itemSet]++;
            }
        }

        foreach (var entry in setCounts)
        {
            ItemSetData set = entry.Key;
            int count = entry.Value;
            foreach (var step in set.setBonuses)
            {
                if (count >= step.itemsRequired) ApplyBonusList(step.bonuses);
            }
        }
    }

    void AddStatsFrom(CardData item)
    {
        if (item == null) return;
        if (item is ArmorData a) defense += a.defenseAmount;
        if (item is WeaponData w)
        {
            minAttackDamage += w.minDamage;
            maxAttackDamage += w.maxDamage;
        }
        if (item is AccessoryData acc)
        {
            defense += acc.bonusDefense;
            minAttackDamage += acc.bonusDamage;
            maxAttackDamage += acc.bonusDamage;
        }

        if (item.itemBonuses != null) ApplyBonusList(item.itemBonuses);
    }

    void ApplyBonusList(List<ItemBonus> bonuses)
    {
        foreach (var bonus in bonuses)
        {
            switch (bonus.statType)
            {
                case StatType.AttackDamage:
                    minAttackDamage += (int)bonus.amount;
                    maxAttackDamage += (int)bonus.amount;
                    break;
                case StatType.Armor:
                    defense += (int)bonus.amount;
                    break;
                case StatType.CritChance:
                    critChance += bonus.amount;
                    break;
                case StatType.CritDamage:
                    critDamageBonus += bonus.amount;
                    break;
                case StatType.DodgeChance:
                    dodgeChance += bonus.amount;
                    break;
                case StatType.BlockChance:
                    blockChance += bonus.amount;
                    break;
            }
        }
    }

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

    // --- HELPER & UI ---
    public static string FormatMoney(long totalCopper)
    {
        long gold = totalCopper / 10000;
        if (gold >= 1000000) return (gold / 1000000f).ToString("F1") + "M <sprite name=\"coins_2\">";
        int silver = (int)((totalCopper % 10000) / 100);
        int copper = (int)(totalCopper % 100);
        string result = "";
        if (gold > 0) result += $"{gold}<sprite name=\"coins_2\"> ";
        if (silver > 0) result += $"{silver}<sprite name=\"coins_1\"> ";
        if (copper > 0 || (gold == 0 && silver == 0)) result += $"{copper}<sprite name=\"coins_0\">";
        return result.Trim();
    }
    public string GetFormattedMoney() => FormatMoney(currentMoney);
    public void Heal(int amount) { currentHealth = Mathf.Min(currentHealth + amount, maxHealth); UpdateUI(); }
    public void RefillMana(int amount) { currentMana = Mathf.Min(currentMana + amount, maxMana); UpdateUI(); }

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            AddExperience(50);
            Debug.Log("50 XP erhalten! Aktuell: " + currentXP + "/" + xpToNextLevel);
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

    public int GetSetItemCount(ItemSetData set)
    {
        if (set == null) return 0;
        int count = 0;
        List<CardData> equipped = new List<CardData> { headItem, chestItem, handsItem, legsItem, feetItem, neckItem, ring1Item, ring2Item, weaponItem, offhandItem };
        foreach (var item in equipped) if (item != null && item.itemSet == set) count++;
        return count;
    }

    public bool IsItemEquipped(CardData item)
    {
        if (item == null) return false;
        if (headItem == item) return true;
        if (chestItem == item) return true;
        if (handsItem == item) return true;
        if (legsItem == item) return true;
        if (feetItem == item) return true;
        if (neckItem == item) return true;
        if (ring1Item == item) return true;
        if (ring2Item == item) return true;
        if (weaponItem == item) return true;
        if (offhandItem == item) return true;
        return false;
    }
}