using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Verknüpfungen")]
    public Transform itemsParent;   // Das 'Item_Grid'
    public GameObject slotPrefab;   // Das 'Inv_Slot' Prefab

    void Awake()
    {
        Instance = this;
    }

    public void RefreshInventory()
    {
        // 1. Alte Slots löschen
        foreach (Transform child in itemsParent)
        {
            Destroy(child.gameObject);
        }

        if (PlayerManager.Instance == null) return;

        // Wir holen die neue Liste (InventoryItem statt CardData)
        List<InventoryItem> inventory = PlayerManager.Instance.inventory;

        // 2. Gefüllte Slots erstellen
        foreach (InventoryItem itemStack in inventory)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemsParent);
            InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                slotScript.SetupSlot(itemStack);
            }
        }

        // 3. Leere Slots auffüllen (damit es schön aussieht)
        int currentSlots = inventory.Count;
        int minSlots = 20; // Mindestens 20 Kästchen anzeigen

        for (int i = currentSlots; i < minSlots; i++)
        {
            GameObject emptySlot = Instantiate(slotPrefab, itemsParent);
            InventorySlot slotScript = emptySlot.GetComponent<InventorySlot>();

            if (slotScript != null)
            {
                slotScript.ClearSlot();
            }
        }
    }
}