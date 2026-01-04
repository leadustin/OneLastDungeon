using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Anzeige-Einstellungen")]
    public float moveSpeed = 1.5f;     // Geschwindigkeit des Aufstiegs
    public float destroyTime = 3.0f;   // Sichtbarkeitsdauer in Sekunden

    private TextMeshPro textMesh;
    private Color startColor;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(string text, Color color)
    {
        textMesh.text = text;
        textMesh.color = color;
        startColor = color;
        timer = 0;
    }

    void Update()
    {
        // Nach oben schweben
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        // Sanftes Ausblenden über die Lebenszeit
        float alpha = Mathf.Lerp(1f, 0f, timer / destroyTime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= destroyTime)
        {
            Destroy(gameObject); //
        }
    }
}