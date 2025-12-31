using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // 1. Wer wurde fallengelassen?
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        // 2. War es ein Equipment-Slot?
        EquipmentSlot equipSlot = droppedObj.GetComponent<EquipmentSlot>();

        if (equipSlot != null)
        {
            // JA! Wir ziehen das Item aus
            equipSlot.Unequip();
        }
    }
}