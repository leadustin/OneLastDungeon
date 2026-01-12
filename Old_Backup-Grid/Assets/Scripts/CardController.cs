using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour
{
    [Header("Grid Daten")]
    public int gridX, gridY;
    public bool isPlayer = false;
    public CardData myData;

    [Header("Visuelle Referenzen (Im Prefab verknüpfen!)")]
    // Der Renderer auf dem Hauptobjekt (Standard: Knochen-Rahmen)
    public SpriteRenderer backgroundRenderer;

    // Der Renderer auf dem Kind-Objekt 'IconArtwork' (Für Schwert, Gegner, etc.)
    public SpriteRenderer iconRenderer;

    // Das Textfeld
    public TextMeshPro nameText;

    [Header("Helden Design")]
    // ZIEHE HIER DEINEN NEUEN HELDEN-RAHMEN REIN
    public Sprite heroFrame;

    // ZIEHE HIER DEIN HELDEN-GESICHT REIN (Optional)
    public Sprite heroIcon;

    // Speichert den ursprünglichen Knochen-Rahmen
    private Sprite defaultBoneFrame;

    void Awake()
    {
        // Wir merken uns beim Start, was im Hintergrund-Renderer drin war (der Knochen-Rahmen)
        if (backgroundRenderer != null)
        {
            defaultBoneFrame = backgroundRenderer.sprite;
        }

        if (nameText == null) nameText = GetComponentInChildren<TextMeshPro>();
    }

    public void SetupCard(int x, int y, CardData data, bool playerStatus)
    {
        gridX = x;
        gridY = y;
        isPlayer = playerStatus;
        myData = data;

        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // 1. Reset (Alles säubern)
        if (nameText != null) nameText.text = "";
        if (iconRenderer != null) iconRenderer.sprite = null;
        if (backgroundRenderer != null) backgroundRenderer.color = Color.white;
        if (iconRenderer != null) iconRenderer.color = Color.white;

        // 2. Logik: Held oder Karte?
        if (isPlayer)
        {
            // --- HELD ---
            if (nameText != null) nameText.text = "Hero";

            // A) Hintergrund austauschen (Helden-Rahmen nutzen)
            if (backgroundRenderer != null && heroFrame != null)
            {
                backgroundRenderer.sprite = heroFrame;
            }

            // B) Icon setzen (Helden-Gesicht)
            if (iconRenderer != null && heroIcon != null)
            {
                iconRenderer.sprite = heroIcon;
            }
        }
        else
        {
            // --- GEGNER / ITEM ---

            // A) Hintergrund zurücksetzen (Knochen-Rahmen nutzen)
            if (backgroundRenderer != null && defaultBoneFrame != null)
            {
                backgroundRenderer.sprite = defaultBoneFrame;
            }

            // B) Daten befüllen
            if (myData != null)
            {
                if (nameText != null) nameText.text = myData.cardName;

                // Icon setzen (Schwert, Trank, Monster)
                if (myData.artwork != null && iconRenderer != null)
                {
                    iconRenderer.sprite = myData.artwork;
                }
            }
        }
    }

    // Hilfsfunktion für externe Skripte (z.B. UI), falls sie das Bild brauchen
    public Sprite GetDisplaySprite()
    {
        if (isPlayer) return heroIcon;
        if (myData != null) return myData.artwork;
        return null;
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        GridManager.Instance.TryMovePlayer(this);
    }
}