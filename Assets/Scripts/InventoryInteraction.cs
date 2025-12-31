using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Einstellungen")]
    public float longPressDuration = 0.5f;
    public Vector2 dragSize = new Vector2(80, 80); // Feste Größe für das Icon an der Maus

    private Canvas mainCanvas;
    private InventorySlot mySlot;
    private GameObject dragObject;
    private CanvasGroup canvasGroup;

    // Klick & Press Variablen
    private bool isDragging = false;
    private bool isPressing = false;
    private float pressTimer = 0f;

    void Awake()
    {
        mySlot = GetComponent<InventorySlot>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Den obersten Canvas finden (Wichtig für die Suche!)
        Canvas foundCanvas = GetComponentInParent<Canvas>();
        if (foundCanvas != null) mainCanvas = foundCanvas.rootCanvas;

        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnDisable()
    {
        // Wenn das Inventar zugeht: Sofort alles löschen
        CleanupDrag();
    }

    // --- CLEANUP (DIE WICHTIGSTE NEUERUNG) ---
    // Diese Methode sucht aggressiv nach Resten und löscht sie.
    private void CleanupDrag()
    {
        // 1. Variable löschen
        if (dragObject != null)
        {
            Destroy(dragObject);
            dragObject = null;
        }

        // 2. Sicherheits-Suche: Hängt noch ein Zombie im Canvas?
        if (mainCanvas != null)
        {
            Transform zombie = mainCanvas.transform.Find("DragIcon_Manual");
            if (zombie != null) Destroy(zombie.gameObject);
        }

        // 3. Original im Slot wieder sichtbar machen
        if (mySlot != null && mySlot.contentDisplay != null)
        {
            mySlot.contentDisplay.enabled = true;
        }

        // 4. Klicks wieder aktivieren
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        isDragging = false;
    }

    // --- KLICK LOGIK ---
    void Update()
    {
        if (isPressing && !isDragging)
        {
            pressTimer += Time.deltaTime;
            if (pressTimer >= longPressDuration)
            {
                isPressing = false;
                Debug.Log("Kontext Menü");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        pressTimer = 0f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        if (!isDragging && pressTimer < longPressDuration)
        {
            mySlot.OnClick();
        }
    }

    // --- DRAG START ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mySlot.GetItem() == null) return;

        // Erstmal aufräumen, falls noch Müll da ist
        CleanupDrag();

        isPressing = false;
        isDragging = true;

        // Geisterbild erstellen
        CreateDragVisualSafe();

        // Original verstecken (damit es so aussieht, als wäre es weg)
        if (mySlot.contentDisplay != null)
        {
            mySlot.contentDisplay.enabled = false;
        }

        canvasGroup.blocksRaycasts = false;
    }

    // --- DRAG UPDATE ---
    public void OnDrag(PointerEventData eventData)
    {
        // Wenn das Objekt existiert, an die Maus kleben
        if (dragObject != null)
        {
            dragObject.transform.position = Input.mousePosition;
        }
    }

    // --- DRAG ENDE ---
    public void OnEndDrag(PointerEventData eventData)
    {
        // Hier rufen wir jetzt IMMER die aggressive Säuberung auf
        CleanupDrag();
    }

    // --- VISUALISIERUNG ---
    void CreateDragVisualSafe()
    {
        if (mySlot.contentDisplay == null) return;

        // Neues Objekt
        dragObject = new GameObject("DragIcon_Manual");

        // An den Canvas hängen
        if (mainCanvas != null) dragObject.transform.SetParent(mainCanvas.transform);
        dragObject.transform.SetAsLastSibling(); // Ganz nach vorne

        // Bild-Komponente
        Image img = dragObject.AddComponent<Image>();
        img.sprite = mySlot.contentDisplay.sprite;
        img.raycastTarget = false; // Maus muss durchklicken können
        img.preserveAspect = true;

        // Größe erzwingen (gegen Riesen-Bilder)
        RectTransform rt = dragObject.GetComponent<RectTransform>();
        rt.sizeDelta = dragSize; // Nimm die Größe aus dem Inspector (z.B. 80x80)

        // Reset Scale
        dragObject.transform.localScale = Vector3.one;

        // Startposition
        dragObject.transform.position = Input.mousePosition;
    }
}