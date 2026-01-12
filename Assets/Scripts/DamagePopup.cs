using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Referenzen")]
    public TextMeshPro textMesh;
    public SpriteRenderer iconRenderer;

    [Header("Einstellungen")]
    public float moveSpeed = 2f;
    public float disappearSpeed = 3f;
    public float lifeTime = 1f;

    // NEU: Hier kannst du die Größe im Inspector einstellen (z.B. 0.3)
    [Header("Icon Größe")]
    public float globalIconScale = 0.3f;

    private Color textColor;
    private float disappearTimer;
    private Vector3 originalScale;

    void Awake()
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        originalScale = transform.localScale;
    }

    public void Setup(string text, Color color, float sizeMultiplier, Sprite icon = null, TMP_FontAsset font = null)
    {
        // Text Setup
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = 6f * sizeMultiplier;
        if (font != null) textMesh.font = font;

        textColor = color;
        transform.localScale = originalScale;

        // Icon Setup
        if (icon != null && iconRenderer != null)
        {
            iconRenderer.sprite = icon;
            iconRenderer.gameObject.SetActive(true);

            // NEU: Wir erzwingen die kleine Skalierung
            iconRenderer.transform.localScale = new Vector3(globalIconScale, globalIconScale, 1f);

            // Position anpassen (Icon links vom Text)
            // Je nach Größe musst du hier evtl. mit der -1.5f spielen
            iconRenderer.transform.localPosition = new Vector3(-1.0f - (globalIconScale * 0.5f), 0, 0);
        }
        else if (iconRenderer != null)
        {
            iconRenderer.gameObject.SetActive(false);
        }

        disappearTimer = lifeTime;
    }

    void Update()
    {
        transform.position += new Vector3(0, moveSpeed, 0) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (iconRenderer != null && iconRenderer.gameObject.activeSelf)
            {
                Color iconColor = iconRenderer.color;
                iconColor.a = textColor.a;
                iconRenderer.color = iconColor;
            }

            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}