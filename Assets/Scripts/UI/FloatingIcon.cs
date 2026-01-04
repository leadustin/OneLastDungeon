using UnityEngine;
using TMPro;

public class FloatingIcon : MonoBehaviour
{
    [Header("Referenzen")]
    public SpriteRenderer iconRenderer; // Das Bild
    public TextMeshPro amountText;      // Optional: Zahl daneben (z.B. "+1")

    [Header("Animation")]
    public float moveSpeed = 2.0f;
    public float destroyTime = 2.5f;
    public Vector3 endScale = new Vector3(1.5f, 1.5f, 1.5f); // Soll es größer werden?

    private float timer;
    private Vector3 startScale;
    private Color startColor;

    public void Setup(Sprite icon, string text, Color color)
    {
        // 1. Bild setzen
        if (iconRenderer != null)
        {
            iconRenderer.sprite = icon;
            // Wir setzen die Farbe auf Weiß (damit das Bild original aussieht), 
            // oder 'color', wenn du es einfärben willst. Meistens ist Weiß besser.
            iconRenderer.color = Color.white;
        }

        // 2. Text setzen (falls vorhanden)
        if (amountText != null)
        {
            amountText.text = text;
            amountText.color = color;
        }

        startScale = transform.localScale;
        timer = 0;
    }

    void Update()
    {
        // Nach oben schweben
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        float progress = timer / destroyTime;

        // Leicht vergrößern für Effekt
        transform.localScale = Vector3.Lerp(startScale, endScale, progress);

        // Ausblenden (Alpha reduzieren)
        float alpha = Mathf.Lerp(1f, 0f, progress);

        if (iconRenderer != null)
        {
            Color c = iconRenderer.color;
            iconRenderer.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (amountText != null)
        {
            amountText.alpha = alpha;
        }

        if (timer >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}