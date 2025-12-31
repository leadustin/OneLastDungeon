using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Einstellungen")]
    public CardType allowedType;
    public Image iconDisplay;
    public Sprite defaultIcon;

    private CardData currentItem;
    private Canvas mainCanvas;

    private Transform originalParent;
    private RectTransform iconRect;
    private Vector3 originalScale;

    void Awake()
    {
        mainCanvas = GetComponentInParent<Canvas>();

        if (iconDisplay != null)
        {
            iconRect = iconDisplay.GetComponent<RectTransform>();
            originalParent = iconDisplay.transform.parent;
            originalScale = iconDisplay.transform.localScale;
        }
    }

    // --- 1. EMPFANGEN (Hier lag der Fehler) ---
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null) return;

        InventorySlot invSlot = droppedObj.GetComponent<InventorySlot>();
        if (invSlot != null)
        {
            InventoryItem itemStack = invSlot.GetItem();

            // WICHTIG: Wir prüfen nur den cardType. 
            // Da PurchasableCardData von CardData erbt, ist itemStack.data immer noch ein CardData!
            if (itemStack != null && itemStack.data != null && itemStack.data.cardType == allowedType)
            {
                EquipItem(itemStack.data);
            }
            else
            {
                Debug.Log($"Falscher Typ! Slot erwartet: {allowedType}, Item ist: {itemStack?.data?.cardType}");
            }
        }
    }

    public void EquipItem(CardData item)
    {
        CardData previousItem = currentItem;
        currentItem = item;

        if (PlayerManager.Instance != null)
        {
            // PlayerManager verarbeitet CardData, also egal ob Waffe oder Rüstung
            PlayerManager.Instance.EquipGear(currentItem, previousItem);
        }
        UpdateDisplay();
    }

    public void Unequip()
    {
        if (currentItem == null) return;

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.UnequipGear(currentItem);
        }

        currentItem = null;
        UpdateDisplay();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null) return;

        originalParent = transform;

        // Bild an den Canvas hängen für freies Draggen
        iconDisplay.transform.SetParent(mainCanvas.transform);
        iconDisplay.transform.SetAsLastSibling();
        iconDisplay.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (currentItem != null && iconDisplay != null)
        {
            iconDisplay.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (iconDisplay != null)
        {
            iconDisplay.transform.SetParent(originalParent);
            iconRect.anchoredPosition = Vector2.zero;
            iconDisplay.transform.localScale = Vector3.one;
            iconDisplay.raycastTarget = false;
        }
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (currentItem != null)
        {
            iconDisplay.sprite = currentItem.artwork;
            iconDisplay.color = Color.white;
            iconDisplay.gameObject.SetActive(true);
            iconDisplay.transform.localScale = Vector3.one;
        }
        else
        {
            if (defaultIcon != null)
            {
                iconDisplay.sprite = defaultIcon;
                iconDisplay.color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                iconDisplay.sprite = null;
                iconDisplay.color = new Color(0, 0, 0, 0);
            }
            iconDisplay.transform.localScale = Vector3.one;
        }
    }
}