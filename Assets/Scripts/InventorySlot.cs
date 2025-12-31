using UnityEngine;
using UnityEngine.UI;
using TMPro;                // Für den Text (Menge)
using UnityEngine.EventSystems; // WICHTIG: Für IDropHandler (Empfangen)

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("UI Verknüpfung")]
    public Image contentDisplay;       // Das 'Content_Image' (wo das Icon hinkommt)
    public TextMeshProUGUI amountText; // Der Text für die Menge (z.B. unten rechts)

    [Header("Einstellungen")]
    public Sprite defaultBackgroundSprite; // Das Bild, wenn der Slot leer ist (dunkler Hintergrund)

    private InventoryItem myItem;      // Die Daten des aktuellen Items (inkl. Anzahl)

    // --- SETUP: SLOT FÜLLEN ---
    public void SetupSlot(InventoryItem item)
    {
        myItem = item;

        if (myItem != null && myItem.data != null)
        {
            // 1. Bild setzen
            if (myItem.data.artwork != null)
            {
                contentDisplay.sprite = myItem.data.artwork;
                contentDisplay.color = Color.white;
            }

            // 2. Zahl anzeigen (nur wenn Stapel > 1)
            if (amountText != null)
            {
                if (myItem.stackSize > 1)
                {
                    amountText.text = myItem.stackSize.ToString();
                    amountText.gameObject.SetActive(true);
                }
                else
                {
                    // Bei 1 zeigen wir keine Zahl (sieht sauberer aus)
                    amountText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            ClearSlot();
        }
    }

    // --- CLEAR: SLOT LEEREN ---
    public void ClearSlot()
    {
        myItem = null;

        // Zurücksetzen auf Hintergrundbild oder Transparent
        if (defaultBackgroundSprite != null)
        {
            contentDisplay.sprite = defaultBackgroundSprite;
            contentDisplay.color = Color.white;
        }
        else
        {
            contentDisplay.sprite = null;
            contentDisplay.color = new Color(0, 0, 0, 0); // Komplett unsichtbar
        }

        // Zahl ausblenden
        if (amountText != null) amountText.gameObject.SetActive(false);
    }

    // --- KLICK LOGIK ---
    public void OnClick()
    {
        if (myItem != null)
        {
            // Versuchen, das Item zu benutzen
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.UseItem(myItem.data);
            }
        }
    }

    // --- DROP LOGIK (Wenn man etwas AUF diesen Slot fallen lässt) ---
    public void OnDrop(PointerEventData eventData)
    {
        // 1. Wer wurde fallen gelassen? (Das Drag-Objekt)
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        // 2. War es ein Ausrüstungs-Slot? (Wir wollen Ausrüstung hier ablegen)
        EquipmentSlot equipSlot = droppedObj.GetComponent<EquipmentSlot>();

        if (equipSlot != null)
        {
            Debug.Log("Ausrüstung auf Inventar-Slot abgelegt -> Ausziehen!");
            equipSlot.Unequip();
        }

        // (Hier könnte man später auch Logik einfügen, um Items innerhalb des Inventars zu tauschen)
    }

    // --- HELPER METHODE ---
    // Wird vom EquipmentSlot gebraucht, wenn wir ein Item AUS dem Inventar in die Ausrüstung ziehen
    public InventoryItem GetItem()
    {
        return myItem;
    }
}