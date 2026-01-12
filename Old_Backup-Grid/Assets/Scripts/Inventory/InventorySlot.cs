using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Nur noch IPointerClickHandler nötig - kein Drag mehr
public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referenzen")]
    public Image contentDisplay; // Dein Icon
    public TextMeshProUGUI amountText;

    // Interne Daten
    private InventoryItem myItem;

    // --- SETUP (Wird vom InventoryUI aufgerufen) ---
    public void SetupSlot(InventoryItem item)
    {
        myItem = item;
        if (myItem != null && myItem.data != null)
        {
            contentDisplay.sprite = myItem.data.artwork;
            contentDisplay.preserveAspect = true;
            contentDisplay.color = Color.white;
            contentDisplay.enabled = true;

            if (amountText != null)
            {
                amountText.text = myItem.stackSize > 1 ? myItem.stackSize.ToString() : "";
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        myItem = null;
        contentDisplay.sprite = null;
        contentDisplay.enabled = false;
        if (amountText != null) amountText.text = "";
    }

    public InventoryItem GetItem() { return myItem; }

    // --- NEU: TAP LOGIK ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (myItem == null || myItem.data == null) return;

        // Hier rufen wir jetzt die Vergleichs-Logik auf statt direkt "UseItem"
        // (Stelle sicher, dass du RequestEquipItem im PlayerManager hinzugefügt hast)
        PlayerManager.Instance.RequestEquipItem(myItem.data);
    }
}