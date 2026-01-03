using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemComparisonUI : MonoBehaviour
{
    public static ItemComparisonUI Instance;

    [Header("Container & Hintergrund")]
    public GameObject comparisonRoot;
    public GameObject equipmentSlotsContainer;
    public Button backgroundCloseButton;

    [Header("Linke Seite (Vergleich)")]
    public GameObject leftSidePanel;
    public Image currentItemIcon;
    public TextMeshProUGUI currentItemName;
    public TextMeshProUGUI currentItemStats;
    public TextMeshProUGUI currentItemDescription;

    [Header("Rechte Seite (Fokus)")]
    public GameObject rightSidePanel;
    public Image newItemIcon;
    public TextMeshProUGUI newItemName;
    public TextMeshProUGUI newItemStats;
    public TextMeshProUGUI newItemDescription;

    [Header("Buttons")]
    public Button equipButton;
    public Button dropButton;

    private CardData pendingItem;
    private bool isInspectingEquippedItem = false;

    void Awake()
    {
        Instance = this;

        if (comparisonRoot != null) comparisonRoot.SetActive(false);

        if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        if (dropButton != null) dropButton.onClick.AddListener(OnDropClicked);
        if (backgroundCloseButton != null) backgroundCloseButton.onClick.AddListener(CloseComparison);
    }

    // --- MODUS A: VERGLEICH (Aus dem Inventar) ---
    public void OpenComparison(CardData newItem)
    {
        pendingItem = newItem;
        isInspectingEquippedItem = false;

        // 1. UI aktivieren
        if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(false);
        if (comparisonRoot != null) comparisonRoot.SetActive(true);

        // 3. LINKS: Prüfen, was wir aktuell tragen
        CardData currentItem = GetEquippedItemByType(newItem);

        if (currentItem != null)
        {
            leftSidePanel.SetActive(true);
            // Links zeigen wir keine Differenz an (Vergleich mit sich selbst ist sinnlos)
            SetupPanel(currentItem, currentItemIcon, currentItemName, currentItemStats, currentItemDescription, null);
        }
        else
        {
            leftSidePanel.SetActive(false);
        }

        // 2. RECHTS: Das neue Item anzeigen
        // HIER passiert die Magie: Wir übergeben 'currentItem' als Vergleichsobjekt!
        rightSidePanel.SetActive(true);
        SetupPanel(newItem, newItemIcon, newItemName, newItemStats, newItemDescription, currentItem);

        // 4. Buttons setzen
        UpdateButtonsForComparison(newItem);
    }

    // --- MODUS B: DETAILANSICHT (Aus dem Ausrüstungs-Slot) ---
    public void OpenEquippedItemDetail(CardData item)
    {
        pendingItem = item;
        isInspectingEquippedItem = true;

        // 1. UI aktivieren
        if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(false);
        if (comparisonRoot != null) comparisonRoot.SetActive(true);

        // 2. RECHTS: Das Item als Detail anzeigen (Kein Vergleichsobjekt, da wir es ja schon tragen)
        rightSidePanel.SetActive(true);
        SetupPanel(item, newItemIcon, newItemName, newItemStats, newItemDescription, null);

        // 3. LINKS: Ausblenden 
        leftSidePanel.SetActive(false);

        // 4. BUTTONS
        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (dropButton != null) dropButton.gameObject.SetActive(true);
    }

    // --- Button Logik ---
    private void UpdateButtonsForComparison(CardData item)
    {
        bool isEquipment = (item is WeaponData || item is ArmorData || item is AccessoryData);

        if (isEquipment)
        {
            if (equipButton != null) equipButton.gameObject.SetActive(true);
            if (dropButton != null) dropButton.gameObject.SetActive(false);
        }
        else
        {
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            if (dropButton != null) dropButton.gameObject.SetActive(false);
        }
    }

    void OnEquipClicked()
    {
        if (pendingItem != null && !isInspectingEquippedItem)
        {
            PlayerManager.Instance.EquipCard(pendingItem);
        }
        CloseComparison();
    }

    void OnDropClicked()
    {
        if (pendingItem != null && isInspectingEquippedItem)
        {
            PlayerManager.Instance.UnequipGear(pendingItem);
        }
        CloseComparison();
    }

    public void CloseComparison()
    {
        comparisonRoot.SetActive(false);
        if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(true);
        pendingItem = null;
    }

    // --- HELPER ---

    // Aktualisiert: Nimmt jetzt optional "compareWith" entgegen
    private void SetupPanel(CardData item, Image icon, TextMeshProUGUI name, TextMeshProUGUI stats, TextMeshProUGUI desc, CardData compareWith)
    {
        icon.sprite = item.artwork;
        icon.preserveAspect = true;
        name.text = item.cardName;

        // Hier geben wir das Vergleichs-Item weiter
        stats.text = GetBaseStatsText(item, compareWith);

        if (desc != null) desc.text = GetBonusAndLoreText(item);
    }

    // --- NEU: BERECHNUNG DER DIFFERENZ ---
    private string GetBaseStatsText(CardData item, CardData compareWith)
    {
        string s = "";

        // 1. WAFFEN (Vergleich Schaden)
        if (item is WeaponData w)
        {
            int myDmg = w.damageAmount;
            s += $"{myDmg}";

            // Haben wir eine Vergleichswaffe?
            if (compareWith is WeaponData wOld)
            {
                int diff = myDmg - wOld.damageAmount;
                s += FormatDifference(diff);
            }
            s += "\n";
        }
        // 2. RÜSTUNG (Vergleich Verteidigung)
        else if (item is ArmorData a)
        {
            int myDef = a.defenseAmount;
            s += $"{myDef}";

            if (compareWith is ArmorData aOld)
            {
                int diff = myDef - aOld.defenseAmount;
                s += FormatDifference(diff);
            }
            s += "\n";
        }
        // 3. ACCESSOIRES (Vergleich beides)
        else if (item is AccessoryData acc)
        {
            // Schaden Zeile
            if (acc.bonusDamage > 0)
            {
                s += $"Schaden: +{acc.bonusDamage}";
                if (compareWith is AccessoryData accOld)
                {
                    int diff = acc.bonusDamage - accOld.bonusDamage;
                    s += FormatDifference(diff);
                }
                s += "\n";
            }

            // Rüstung Zeile
            if (acc.bonusDefense > 0)
            {
                s += $"Rüstung: +{acc.bonusDefense}";
                if (compareWith is AccessoryData accOld)
                {
                    int diff = acc.bonusDefense - accOld.bonusDefense;
                    s += FormatDifference(diff);
                }
                s += "\n";
            }
        }
        return s;
    }

    // Hilfsfunktion für die Farben
    private string FormatDifference(int diff)
    {
        if (diff == 0) return ""; // Keine Änderung -> Nichts anzeigen

        if (diff > 0)
        {
            // Besser -> Grün mit Plus
            return $" <color=#00FF00>(+{diff})</color>";
        }
        else
        {
            // Schlechter -> Rot mit Minus (das Minus bringt 'diff' schon mit)
            return $" <color=#FF0000>({diff})</color>";
        }
    }

    private string GetBonusAndLoreText(CardData item)
    {
        string text = "";
        if (!string.IsNullOrEmpty(item.description))
            text += $"<color=#CCCCCC><i>{item.description}</i></color>\n\n";

        if (item.itemBonuses != null && item.itemBonuses.Count > 0)
        {
            text += "<color=#FFD700>Gegenstandsboni:</color>\n";
            foreach (ItemBonus bonus in item.itemBonuses)
            {
                string sign = bonus.amount > 0 ? "+" : "";
                string suffix = IsPercentageAttribute(bonus.statType) ? "%" : "";
                text += $"{sign}{bonus.amount}{suffix} {bonus.statType}\n";
            }
        }
        return text;
    }

    private bool IsPercentageAttribute(StatType type)
    {
        return (type == StatType.CritChance || type == StatType.AttackSpeed || type == StatType.DamageReduction);
    }

    private CardData GetEquippedItemByType(CardData newItem)
    {
        EquipmentType type = EquipmentType.Head;
        if (newItem is ArmorData a) type = a.equipmentType;
        else if (newItem is WeaponData w) type = w.equipmentType;
        else if (newItem is AccessoryData acc) type = acc.equipmentType;

        return PlayerManager.Instance.GetEquippedItem(type);
    }
}