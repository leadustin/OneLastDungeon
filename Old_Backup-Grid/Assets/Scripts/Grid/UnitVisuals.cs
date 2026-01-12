using UnityEngine;

public class UnitVisuals : MonoBehaviour
{
    [Header("Verknüpfungen")]
    // Zieh hier den SpriteRenderer rein, der das Gesicht anzeigt (Child-Objekt)
    public SpriteRenderer iconRenderer;
    public SpriteRenderer baseRenderer; // Der Sockel (optional, für Farben)

    // Diese Funktion rufen wir später auf, um das Bild zu ändern
    public void SetData(Sprite faceIcon)
    {
        if (iconRenderer != null)
        {
            iconRenderer.sprite = faceIcon;
        }
    }

    // Hilfsfunktion: Um die Blickrichtung zu ändern (Gegner schauen nach Links)
    public void FlipOrientation(bool doFlip)
    {
        if (iconRenderer != null)
        {
            iconRenderer.flipX = doFlip;
        }
    }
}