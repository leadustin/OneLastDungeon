using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referenzen")]
    public Image iconImage;
    public Image frameImage;
    public TextMeshProUGUI amountText;

    private GameItem myItem;
    // HIER WAR DER FEHLER: Typ angepasst
    private InventoryMenu uiManager;

    // Methode angepasst
    public void Setup(GameItem item, InventoryMenu manager)
    {
        myItem = item;
        uiManager = manager;

        if (myItem != null && myItem.template != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = myItem.template.icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
            }

            if (amountText != null)
            {
                amountText.text = myItem.stackCount > 1 ? myItem.stackCount.ToString() : "";
            }

            if (frameImage != null)
            {
                frameImage.color = myItem.template.GetRarityColor();
                frameImage.enabled = true;
            }
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (amountText != null) amountText.text = "";
            if (frameImage != null) frameImage.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Wir leiten den Klick an den Manager weiter.
        // Der Manager entscheidet jetzt: "Sofort anziehen" oder "Vergleichsfenster öffnen".

        if (uiManager != null)
        {
            // WICHTIG: Hier rufen wir die NEUE Methode auf, die wir vorhin erstellt haben
            uiManager.OnItemClicked(myItem);
        }
    }
}