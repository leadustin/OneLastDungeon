using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_ItemDetails : MonoBehaviour
{
    [Header("Anzeige Elemente")]
    public GameObject windowRoot; // Das ganze Fenster
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public TextMeshProUGUI itemStatsText;
    public TextMeshProUGUI itemDescriptionText;

    [Header("Buttons")]
    public Button equipButton;
    public Button closeButton;

    private GameItem currentItem;
    // HIER WAR DER FEHLER: Es heiﬂt jetzt InventoryMenu
    private InventoryMenu mainManager;

    void Start()
    {
        if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseWindow);
        CloseWindow();
    }

    // Manager Typ angepasst
    public void OpenDetails(GameItem item, InventoryMenu manager)
    {
        currentItem = item;
        mainManager = manager;

        if (item == null || item.template == null) return;

        windowRoot.SetActive(true);

        if (itemIcon != null) itemIcon.sprite = item.template.icon;
        if (itemNameText != null)
        {
            itemNameText.text = item.template.itemName;
            itemNameText.color = item.template.GetRarityColor();
        }
        if (itemDescriptionText != null) itemDescriptionText.text = item.template.description;
        if (itemTypeText != null) itemTypeText.text = item.template.type.ToString();

        if (itemStatsText != null) itemStatsText.text = BuildStatString(item);
    }

    string BuildStatString(GameItem item)
    {
        string s = "";
        foreach (var pair in item.rolledStats)
        {
            s += $"+{pair.Value} {pair.Key}\n";
        }
        return s;
    }

    void OnEquipClicked()
    {
        if (mainManager != null && currentItem != null)
        {
            mainManager.TryEquipItem(currentItem);
            CloseWindow();
        }
    }

    public void CloseWindow()
    {
        if (windowRoot != null) windowRoot.SetActive(false);
    }
}