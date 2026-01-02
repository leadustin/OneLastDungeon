using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// WICHTIG: Wir haben IBeginDragHandler, IDragHandler und IEndDragHandler hinzugefügt
public class EquipmentSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Welcher Slot ist das?")]
    public EquipmentType allowedType;

    [Header("UI Referenzen")]
    public Image iconDisplay;
    public Sprite defaultIcon;

    private CardData currentItem;

    // Variablen für das Ziehen (Drag & Drop)
    private GameObject dragObject;
    private Canvas mainCanvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Wir brauchen eine CanvasGroup, damit die Maus "durchklicken" kann beim Ziehen
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Canvas foundCanvas = GetComponentInParent<Canvas>();
        if (foundCanvas != null) mainCanvas = foundCanvas.rootCanvas;
    }

    // --- VISUALS UPDATE ---
    public void UpdateSlotUI(CardData item)
    {
        currentItem = item;

        if (currentItem != null)
        {
            iconDisplay.sprite = currentItem.artwork;
            iconDisplay.preserveAspect = true;
            iconDisplay.color = Color.white;
            iconDisplay.enabled = true;
        }
        else
        {
            if (defaultIcon != null)
            {
                iconDisplay.sprite = defaultIcon;
                iconDisplay.color = new Color(1, 1, 1, 0.2f);
                iconDisplay.enabled = true;
            }
            else
            {
                iconDisplay.sprite = null;
                iconDisplay.color = Color.clear;
                iconDisplay.enabled = false;
            }
        }
    }

    // --- DROP LOGIK (Item kommt HIER an -> Anziehen) ---
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        InventorySlot invSlot = droppedObj.GetComponent<InventorySlot>();
        if (invSlot != null)
        {
            InventoryItem itemStack = invSlot.GetItem();
            if (itemStack != null && itemStack.data != null)
            {
                if (CheckItemType(itemStack.data))
                {
                    PlayerManager.Instance.EquipCard(itemStack.data);
                }
            }
        }
    }

    // --- DRAG START (Wir ziehen das Item HIER RAUS) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return; // Leeren Slot kann man nicht ziehen

        // 1. Visuellen Klon erstellen
        dragObject = new GameObject("EquipDragIcon");
        dragObject.transform.SetParent(mainCanvas.transform);
        dragObject.transform.SetAsLastSibling();

        Image img = dragObject.AddComponent<Image>();
        img.sprite = iconDisplay.sprite;
        img.raycastTarget = false;

        RectTransform rt = dragObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80); // Feste Größe beim Ziehen

        // 2. Original unsichtbar machen (optional, sieht oft besser aus)
        iconDisplay.color = new Color(1, 1, 1, 0.5f); // Halbtransparent

        // 3. WICHTIG: Raycasts blockieren ausschalten, damit wir den DropZone treffen
        canvasGroup.blocksRaycasts = false;
    }

    // --- DRAG UPDATE ---
    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null)
            dragObject.transform.position = Input.mousePosition;
    }

    // --- DRAG ENDE ---
    public void OnEndDrag(PointerEventData eventData)
    {
        // Aufräumen
        if (dragObject != null) Destroy(dragObject);

        // Original wieder normal machen
        canvasGroup.blocksRaycasts = true;
        if (currentItem != null) iconDisplay.color = Color.white;

        // Hinweis: Wenn wir über der DropZone losgelassen haben, 
        // hat die DropZone schon "Unequip" aufgerufen.
    }

    // --- UNEQUIP LOGIK ---
    public void Unequip()
    {
        if (currentItem != null)
        {
            PlayerManager.Instance.UnequipGear(currentItem);
        }
    }

    private bool CheckItemType(CardData item)
    {
        if (item is ArmorData armor) return armor.equipmentType == allowedType;
        if (item is WeaponData weapon) return weapon.equipmentType == allowedType;
        if (item is AccessoryData acc) return acc.equipmentType == allowedType;
        return false;
    }

    // Rechtsklick zum Ausziehen (als Alternative)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Option A: PC Rechtsklick
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Unequip();
        }
        // Option B: Handy/Tablet Doppeltippen (ClickCount zählt das automatisch)
        else if (eventData.clickCount >= 2)
        {
            Unequip();
        }
    }
}