using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Bewegung")]
    public float moveSpeed = 2.0f;
    public float destroyTime = 2.0f;
    public Vector3 motionDirection = Vector3.up; // Standard: Nach oben

    private TextMeshPro textMesh;
    private float timer;
    private Vector3 startScale;
    private Color definedColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // --- SETUP-FUNKTION (Jetzt mit Größe und Style) ---
    public void Setup(string text, Color color, float fontSize, bool isBold)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = fontSize;
        definedColor = color;

        // Fettgedruckt?
        if (isBold) textMesh.fontStyle = FontStyles.Bold;
        else textMesh.fontStyle = FontStyles.Normal;

        // Damit es immer gut aussieht: Vorne rendern
        textMesh.sortingOrder = 500;

        // Start-Größe merken für Animationen
        startScale = transform.localScale;
        timer = 0;
    }

    void Update()
    {
        // 1. Bewegung
        transform.position += motionDirection * moveSpeed * Time.deltaTime;

        // 2. Timer
        timer += Time.deltaTime;
        float progress = timer / destroyTime;

        // 3. Ausblenden (Alpha)
        float alpha = Mathf.Lerp(1f, 0f, progress);
        textMesh.color = new Color(definedColor.r, definedColor.g, definedColor.b, alpha);

        // Optional: Kleiner "Pop"-Effekt am Anfang
        if (progress < 0.2f)
        {
            float scale = Mathf.Lerp(1f, 1.5f, progress * 5f);
            transform.localScale = startScale * scale;
        }
        else
        {
            transform.localScale = startScale;
        }

        if (timer >= destroyTime) Destroy(gameObject);
    }
}