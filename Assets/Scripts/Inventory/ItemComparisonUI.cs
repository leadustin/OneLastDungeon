using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

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
    public TextMeshProUGUI currentItemType;
    public TextMeshProUGUI currentItemStatLabel;
    public TextMeshProUGUI currentItemStats;
    public TextMeshProUGUI currentItemDescription;
    public Button leftSetDetailsButton;

    [Header("Rechte Seite (Fokus)")]
    public GameObject rightSidePanel;
    public Image newItemIcon;
    public TextMeshProUGUI newItemName;
    public TextMeshProUGUI newItemType;
    public TextMeshProUGUI newItemStatLabel;
    public TextMeshProUGUI newItemStats;
    public TextMeshProUGUI newItemDescription;
    public Button rightSetDetailsButton;

    [Header("Buttons")]
    public Button equipButton;
    public Button dropButton;

    [Header("Set Detail Popup")]
    public GameObject setDetailPanel;
    public TextMeshProUGUI setDetailTitle;
    public Button setDetailCloseButton;

    [Header("Set Liste Konfiguration")]
    public Transform setListContainer;
    public GameObject setItemRowPrefab;

    private CardData pendingItem;
    private bool isInspectingEquippedItem = false;

    private ItemSetData currentLeftSet;
    private ItemSetData currentRightSet;

    void Awake()
    {
        Instance = this;
        if (comparisonRoot != null) comparisonRoot.SetActive(false);
        if (setDetailPanel != null) setDetailPanel.SetActive(false);

        if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        if (dropButton != null) dropButton.onClick.AddListener(OnDropClicked);
        if (backgroundCloseButton != null) backgroundCloseButton.onClick.AddListener(CloseComparison);

        if (leftSetDetailsButton != null) leftSetDetailsButton.onClick.AddListener(() => OpenSetDetailPopup(currentLeftSet));
        if (rightSetDetailsButton != null) rightSetDetailsButton.onClick.AddListener(() => OpenSetDetailPopup(currentRightSet));
        if (setDetailCloseButton != null) setDetailCloseButton.onClick.AddListener(CloseSetDetailPopup);
    }

    public void OpenComparison(CardData newItem)
    {
        pendingItem = newItem;
        isInspectingEquippedItem = false;

        if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(false);
        if (comparisonRoot != null) comparisonRoot.SetActive(true);

        CardData currentItem = GetEquippedItemByType(newItem);

        rightSidePanel.SetActive(true);
        currentRightSet = newItem.itemSet;
        SetupPanel(newItem, newItemIcon, newItemName, newItemType, newItemStatLabel, newItemStats, newItemDescription, rightSetDetailsButton, currentItem);

        if (currentItem != null)
        {
            leftSidePanel.SetActive(true);
            currentLeftSet = currentItem.itemSet;
            SetupPanel(currentItem, currentItemIcon, currentItemName, currentItemType, currentItemStatLabel, currentItemStats, currentItemDescription, leftSetDetailsButton, null);
        }
        else
        {
            leftSidePanel.SetActive(false);
            currentLeftSet = null;
        }

        UpdateButtonsForComparison(newItem);
    }

    public void OpenEquippedItemDetail(CardData item)
    {
        pendingItem = item;
        isInspectingEquippedItem = true;

        if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(false);
        if (comparisonRoot != null) comparisonRoot.SetActive(true);

        rightSidePanel.SetActive(true);
        currentRightSet = item.itemSet;
        SetupPanel(item, newItemIcon, newItemName, newItemType, newItemStatLabel, newItemStats, newItemDescription, rightSetDetailsButton, null);

        leftSidePanel.SetActive(false);

        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (dropButton != null) dropButton.gameObject.SetActive(true);
    }

    private void SetupPanel(CardData item, Image icon, TextMeshProUGUI name,
                            TextMeshProUGUI typeText, TextMeshProUGUI statLabel,
                            TextMeshProUGUI stats, TextMeshProUGUI desc,
                            Button detailsBtn, CardData compareWith)
    {
        icon.sprite = item.artwork;
        icon.preserveAspect = true;
        name.text = item.cardName;

        if (typeText != null) typeText.text = GetItemTypeString(item);
        if (statLabel != null) statLabel.text = GetStatLabelString(item);

        stats.text = GetBaseStatsText(item, compareWith);

        if (desc != null) desc.text = GetDescriptionAndSetList(item);

        if (detailsBtn != null) detailsBtn.gameObject.SetActive(item.itemSet != null);
    }

    private string GetDescriptionAndSetList(CardData item)
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
                string suffix = bonus.isPercentage ? "%" : "";
                text += $"{sign}{bonus.amount}{suffix} {bonus.statType}\n";
            }
            text += "\n";
        }

        if (item.itemSet != null)
        {
            ItemSetData set = item.itemSet;
            int currentCount = PlayerManager.Instance.GetSetItemCount(set);

            bool isEquipped = PlayerManager.Instance.IsItemEquipped(item);
            if (!isEquipped && !isInspectingEquippedItem) currentCount++;

            int totalItems = (set.itemsInSet != null) ? set.itemsInSet.Count : 0;

            text += $"<color=#FFD700>{set.setName} ({currentCount}/{totalItems})</color>\n";

            if (set.itemsInSet != null)
            {
                foreach (CardData setItem in set.itemsInSet)
                {
                    if (setItem == null) continue;

                    bool wearingThisPart = PlayerManager.Instance.IsItemEquipped(setItem);
                    if (setItem == item) wearingThisPart = true;

                    if (wearingThisPart)
                        text += $"  <color=#00FF00>{setItem.cardName}</color>\n";
                    else
                        text += $"  <color=#888888>{setItem.cardName}</color>\n";
                }
            }
        }
        return text;
    }

    public void OpenSetDetailPopup(ItemSetData set)
    {
        if (set == null) return;

        setDetailPanel.SetActive(true);
        if (setDetailTitle != null) setDetailTitle.text = set.setName;

        if (setListContainer != null)
        {
            foreach (Transform child in setListContainer) Destroy(child.gameObject);

            if (set.itemsInSet != null && setItemRowPrefab != null)
            {
                for (int i = 0; i < set.itemsInSet.Count; i++)
                {
                    CardData item = set.itemsInSet[i];
                    if (item == null) continue;

                    GameObject rowObj = Instantiate(setItemRowPrefab, setListContainer);
                    SetItemRow rowScript = rowObj.GetComponent<SetItemRow>();

                    if (rowScript != null)
                    {
                        bool isEquipped = PlayerManager.Instance.IsItemEquipped(item);

                        string bonusString = "";

                        if (set.setBonuses != null && i < set.setBonuses.Count)
                        {
                            var step = set.setBonuses[i];
                            foreach (var b in step.bonuses)
                            {
                                string suffix = b.isPercentage ? "%" : "";
                                string sign = b.amount > 0 ? "+" : "";
                                bonusString += $"{sign}{b.amount}{suffix} {b.statType} ";
                            }
                        }
                        else
                        {
                            bonusString = "-";
                        }

                        rowScript.Setup(item, isEquipped, bonusString);
                    }
                }
            }
        }
    }

    public void CloseSetDetailPopup()
    {
        setDetailPanel.SetActive(false);
    }

    private string GetItemTypeString(CardData item)
    {
        if (item is WeaponData) return "Waffe";
        if (item is ArmorData a)
        {
            switch (a.equipmentType)
            {
                case EquipmentType.Head: return "Helm";
                case EquipmentType.Chest: return "Brust";
                case EquipmentType.Hands: return "Handschuhe";
                case EquipmentType.Legs: return "Hose";
                case EquipmentType.Feet: return "Schuhe";
                default: return "Rüstung";
            }
        }
        if (item is AccessoryData acc)
        {
            if (acc.equipmentType == EquipmentType.Neck) return "Amulett";
            if (acc.equipmentType == EquipmentType.Ring) return "Ring";
            return "Schmuck";
        }
        if (item is ConsumableData) return "Trank";
        return "Gegenstand";
    }

    private string GetStatLabelString(CardData item)
    {
        if (item is WeaponData) return "Schaden:";
        if (item is ArmorData) return "Rüstung:";
        return "";
    }

    private void UpdateButtonsForComparison(CardData item)
    {
        bool isEquipment = (item is WeaponData || item is ArmorData || item is AccessoryData);
        if (isEquipment) { if (equipButton != null) equipButton.gameObject.SetActive(true); if (dropButton != null) dropButton.gameObject.SetActive(false); }
        else { if (equipButton != null) equipButton.gameObject.SetActive(false); if (dropButton != null) dropButton.gameObject.SetActive(false); }
    }
    void OnEquipClicked() { if (pendingItem != null && !isInspectingEquippedItem) PlayerManager.Instance.EquipCard(pendingItem); CloseComparison(); }
    void OnDropClicked() { if (pendingItem != null && isInspectingEquippedItem) PlayerManager.Instance.UnequipGear(pendingItem); CloseComparison(); }
    public void CloseComparison() { comparisonRoot.SetActive(false); if (equipmentSlotsContainer != null) equipmentSlotsContainer.SetActive(true); pendingItem = null; }

    // --- BERECHNUNG DER STATS & DIFFERENZ (Pfeilfarbe angepasst) ---
    private string GetBaseStatsText(CardData item, CardData compareWith)
    {
        string s = "";

        // --- Hilfsvariable für Formatierung ---
        string diffPrefix = "\n<size=80%>"; // Neue Zeile und 80% Größe
        string diffSuffix = "</size>";

        // 1. WAFFEN
        if (item is WeaponData w)
        {
            s += $"{w.minDamage} - {w.maxDamage}";
            if (compareWith is WeaponData wOld)
            {
                int diffMin = w.minDamage - wOld.minDamage;
                int diffMax = w.maxDamage - wOld.maxDamage;

                // Neue Zeile für die Differenz
                s += $"{diffPrefix}{FormatSingleDiff(diffMin)} {FormatSingleDiff(diffMax)}{diffSuffix}";
            }
            s += "\n";
        }
        // 2. RÜSTUNG
        else if (item is ArmorData a)
        {
            s += $"{a.defenseAmount}";
            if (compareWith is ArmorData aOld)
            {
                s += $"{diffPrefix}{FormatSingleDiff(a.defenseAmount - aOld.defenseAmount)}{diffSuffix}";
            }
            s += "\n";
        }
        // 3. SCHMUCK
        else if (item is AccessoryData acc)
        {
            if (acc.bonusDamage > 0)
            {
                s += $"Schaden: +{acc.bonusDamage}";
                if (compareWith is AccessoryData accOld)
                {
                    s += $"{diffPrefix}{FormatSingleDiff(acc.bonusDamage - accOld.bonusDamage)}{diffSuffix}";
                }
                s += "\n";
            }
            // Falls Schaden UND Rüstung drauf sind, setzen wir Rüstung in eine ganz neue Zeile
            if (acc.bonusDefense > 0)
            {
                s += $"Rüstung: +{acc.bonusDefense}";
                if (compareWith is AccessoryData accOld)
                {
                    s += $"{diffPrefix}{FormatSingleDiff(acc.bonusDefense - accOld.bonusDefense)}{diffSuffix}";
                }
                s += "\n";
            }
        }
        return s;
    }

    // Hilfsfunktion: Pfeil in Standardfarbe, Zahl gefärbt
    private string FormatSingleDiff(int diff)
    {
        if (diff == 0) return "";

        if (diff > 0)
        {
            // Pfeil weiß (oder normaler Text), Zahl GRÜN
            return $" ↑ <color=#00FF00>{diff}</color>";
        }
        else
        {
            // Pfeil weiß (oder normaler Text), Zahl ROT (positiver Betrag)
            return $" ↓ <color=#FF0000>{Mathf.Abs(diff)}</color>";
        }
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