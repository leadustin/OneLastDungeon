using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;
    public GameObject textPrefab; // Hier ziehen wir gleich das Prefab rein

    void Awake()
    {
        Instance = this;
    }

    public void Show(string text, Vector3 position, Color color)
    {
        if (textPrefab == null) return;

        // Leicht über der Karte spawnen (y + 1)
        Vector3 spawnPos = position + new Vector3(0, 1f, -2f); // -2 damit es VOR allem anderen ist

        GameObject go = Instantiate(textPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<FloatingText>().Setup(text, color);
    }
}