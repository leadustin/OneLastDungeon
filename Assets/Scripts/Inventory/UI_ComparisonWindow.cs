using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class UI_ComparisonWindow : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject rootPanel;
    public GameObject leftSidePanel;
    public GameObject rightSidePanel;

    [Header("Linke Seite (Aktuell)")]
    public Image currentIcon;
    public TextMeshProUGUI currentName;
    public TextMeshProUGUI currentType;
    public TextMeshProUGUI currentStats;
    public TextMeshProUGUI currentDescription;

    [Header("Rechte Seite (Neu)")]
    public Image newIcon;
    public TextMeshProUGUI newName;
    public TextMeshProUGUI newType;
    public TextMeshProUGUI newStats;
    public TextMeshProUGUI newDescription;
    public Button viewSetButton; // <-- NEU: Button um Set-Details zu sehen

    [Header("Set Detail Popup")] // --- NEU: SET LOGIK START ---
    public GameObject setDetailPanel;     // Das Panel selbst
    public TextMeshProUGUI setDetailTitle; // Titel "Dragon Set (2/5)"
    public TextMeshProUGUI setDetailList;  // Textfeld für die Boni
    public Button setDetailCloseButton;    // Schließen Button im Set Panel
    // --- NEU: SET LOGIK ENDE ---

    [Header("Main Buttons")]
    public Button equipButton;
    public Button closeButton;
    public TextMeshProUGUI equipButtonText;

    // Interne Daten
    private GameItem itemToEquip;
    private InventoryMenu inventoryManager;

    void Start()
    {
        // Listener hinzufügen
        if (closeButton) closeButton.onClick.AddListener(CloseWindow);
        if (equipButton) equipButton.onClick.AddListener(OnEquipClicked);

        // --- NEU ---
        if (viewSetButton) viewSetButton.onClick.AddListener(OpenSetDetails);
        if (setDetailCloseButton) setDetailCloseButton.onClick.AddListener(CloseSetDetails);

        // Am Anfang alles verstecken
        CloseWindow();
        CloseSetDetails();
    }

    public void OpenComparison(GameItem newItem, GameItem oldItem, InventoryMenu manager)
    {
        itemToEquip = newItem;
        inventoryManager = manager;
        rootPanel.SetActive(true);
        CloseSetDetails(); // Sicherstellen, dass das Sub-Popup zu ist

        // 1. Das NEUE Item (Rechts)
        FillPanel(newItem, newIcon, newName, newType, newDescription);

        // --- NEU: Prüfen ob Set-Button angezeigt werden soll ---
        if (viewSetButton != null)
        {
            EquipmentTemplate eq = newItem.template as EquipmentTemplate;
            // Button nur zeigen, wenn es Ausrüstung UND Teil eines Sets ist
            bool isSetItem = (eq != null && eq.itemSet != null);
            viewSetButton.gameObject.SetActive(isSetItem);
        }
        // -------------------------------------------------------

        // 2. Das ALTE Item (Links)
        if (oldItem != null)
        {
            leftSidePanel.SetActive(true);
            FillPanel(oldItem, currentIcon, currentName, currentType, currentDescription);
            currentStats.text = GetStatsText(oldItem, null);
            newStats.text = GetStatsText(newItem, oldItem);
        }
        else
        {
            leftSidePanel.SetActive(false);
            newStats.text = GetStatsText(newItem, null);
        }

        if (equipButtonText != null)
        {
            equipButtonText.text = newItem.IsEquipment() ? "Equip" : "Use";
        }
    }

    // --- NEU: SET LOGIK METHODEN ---
    void OpenSetDetails()
    {
        if (itemToEquip == null) return;
        EquipmentTemplate eq = itemToEquip.template as EquipmentTemplate;
        if (eq == null || eq.itemSet == null) return;

        ItemSetTemplate set = eq.itemSet;

        setDetailPanel.SetActive(true);

        // 1. Zählen, wie viele Teile der Held schon trägt
        int equippedCount = CountEquippedSetItems(set, inventoryManager.selectedHero);

        // Wenn wir das Item noch NICHT tragen (es ist ja im Vergleichsfenster), 
        // zählen wir +1 dazu, damit der Spieler sieht "Wenn ich das anziehe, habe ich X Teile".
        bool alreadyWearingThisItem = (inventoryManager.selectedHero.equipment.ContainsKey(eq.slot)
                                       && inventoryManager.selectedHero.equipment[eq.slot].template == eq);

        if (!alreadyWearingThisItem) equippedCount++;

        // 2. Titel setzen
        if (setDetailTitle != null)
            setDetailTitle.text = $"{set.setName} <size=70%>({equippedCount} active)</size>";

        // 3. Liste Generieren
        StringBuilder sb = new StringBuilder();
        foreach (var bonus in set.bonuses)
        {
            // Ist dieser Bonus aktiv?
            bool isActive = equippedCount >= bonus.piecesRequired;

            string colorHex = isActive ? "#00FF00" : "#808080"; // Grün oder Grau

            sb.Append($"<color={colorHex}>");
            sb.Append($"({bonus.piecesRequired}) {bonus.statBonus} +{bonus.value}");

            if (bonus.passiveSkillEffect != null)
            {
                sb.Append($" & {bonus.passiveSkillEffect.skillName}");
            }
            sb.Append("</color>\n");
        }

        if (setDetailList != null) setDetailList.text = sb.ToString();
    }

    void CloseSetDetails()
    {
        if (setDetailPanel) setDetailPanel.SetActive(false);
    }

    int CountEquippedSetItems(ItemSetTemplate set, HeroRuntimeData hero)
    {
        if (hero == null) return 0;
        int count = 0;
        foreach (var kvp in hero.equipment)
        {
            GameItem item = kvp.Value;
            if (item.template is EquipmentTemplate eqTemp && eqTemp.itemSet == set)
            {
                count++;
            }
        }
        return count;
    }
    // -------------------------------

    void FillPanel(GameItem item, Image icon, TextMeshProUGUI name, TextMeshProUGUI type, TextMeshProUGUI desc)
    {
        if (icon) icon.sprite = item.template.icon;
        if (name) name.text = item.template.itemName;
        if (desc) desc.text = item.template.description;

        if (type)
        {
            if (item.template is EquipmentTemplate eq)
                type.text = $"{eq.rarity} {eq.subType}";
            else
                type.text = $"{item.template.rarity} {item.template.category}";
        }

        if (name) name.color = item.template.GetRarityColor();
    }

    string GetStatsText(GameItem item, GameItem compareWith)
    {
        StringBuilder sb = new StringBuilder();

        if (item.rolledStats != null && item.rolledStats.Count > 0)
        {
            foreach (var kvp in item.rolledStats)
            {
                StatType type = kvp.Key;
                int value = kvp.Value;

                sb.Append($"{type}: {value}");

                if (compareWith != null)
                {
                    int oldValue = 0;
                    if (compareWith.rolledStats != null && compareWith.rolledStats.ContainsKey(type))
                        oldValue = compareWith.rolledStats[type];

                    int diff = value - oldValue;
                    sb.Append(FormatDiff(diff));
                }
                sb.AppendLine();
            }
        }

        if (item.template is ConsumableTemplate cons)
        {
            sb.AppendLine($"Effect: {cons.effect}");
            sb.AppendLine($"Amount: {cons.amount}");
        }

        return sb.ToString();
    }

    string FormatDiff(int diff)
    {
        if (diff == 0) return "";
        if (diff > 0) return $" <color=#00FF00>(+{diff})</color>";
        else return $" <color=#FF0000>({diff})</color>";
    }

    void OnEquipClicked()
    {
        if (inventoryManager != null && itemToEquip != null)
        {
            inventoryManager.TryEquipItem(itemToEquip);
        }
        CloseWindow();
    }

    public void CloseWindow()
    {
        rootPanel.SetActive(false);
    }
}