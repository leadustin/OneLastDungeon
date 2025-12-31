using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1f;

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
    }

    void Update()
    {
        // Nach oben schweben
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Langsam ausblenden
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / destroyTime);
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (timer >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}