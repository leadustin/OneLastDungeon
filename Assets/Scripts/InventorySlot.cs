using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Wichtig für Pointer Events
using TMPro;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Referenzen")]
    public Image contentDisplay;
    public TextMeshProUGUI amountText;

    [Header("Steuerung")]
    [Tooltip("Dauer in Sekunden für Info-Fenster")]
    public float longPressDuration = 0.5f;

    // Interne Daten
    private InventoryItem myItem;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private GameObject dragObject;

    // Timer Variablen
    private bool isPressed = false;
    private float pressTimer = 0f;
    private bool longPressTriggered = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    // --- LOGIK FÜR LONG PRESS (UPDATE) ---
    void Update()
    {
        if (isPressed && !longPressTriggered)
        {
            pressTimer += Time.deltaTime;

            if (pressTimer >= longPressDuration)
            {
                // Zeit abgelaufen -> Info Fenster öffnen!
                longPressTriggered = true;
                if (myItem != null && myItem.data != null)
                {
                    if (ItemInfoUI.Instance != null)
                        ItemInfoUI.Instance.OpenInfo(myItem.data);
                }
            }
        }
    }

    // --- INPUT EVENTS (FINGER DRAUF/WEG) ---

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressTimer = 0f;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        // Wenn wir NICHT lange gedrückt haben und NICHT gezogen haben -> Ausrüsten
        if (!longPressTriggered && !eventData.dragging)
        {
            if (myItem != null && myItem.data != null)
            {
                PlayerManager.Instance.UseItem(myItem.data);
            }
        }
    }

    // --- STANDARD SLOT LOGIK (Setup & Drag) ---

    public void SetupSlot(InventoryItem item)
    {
        myItem = item;
        if (myItem != null && myItem.data != null)
        {
            contentDisplay.sprite = myItem.data.artwork;
            contentDisplay.preserveAspect = true; // Verhindert Verzerrung
            contentDisplay.color = Color.white;
            contentDisplay.enabled = true;

            if (amountText != null)
            {
                amountText.text = myItem.stackSize > 1 ? myItem.stackSize.ToString() : "";
            }
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

    // --- DRAG AND DROP ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (myItem == null) return;

        // Timer abbrechen beim Ziehen
        isPressed = false;

        // Fake Icon erstellen
        dragObject = new GameObject("DragIcon");
        dragObject.transform.SetParent(parentCanvas.transform);
        dragObject.transform.SetAsLastSibling();

        Image img = dragObject.AddComponent<Image>();
        img.sprite = contentDisplay.sprite;
        img.raycastTarget = false;

        RectTransform rt = dragObject.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80);

        contentDisplay.color = new Color(1, 1, 1, 0.5f);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragObject != null) dragObject.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragObject != null) Destroy(dragObject);
        canvasGroup.blocksRaycasts = true;
        if (myItem != null) contentDisplay.color = Color.white;
    }
}