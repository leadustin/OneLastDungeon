using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    public GameObject textPrefab; // Dein altes Text-Prefab (für Schaden)

    [Header("Neu: Das Icon Prefab")]
    public GameObject iconPrefab; // <--- HIER DEIN NEUES PREFAB REINZIEHEN

    [Header("Feste Position (Relativ zur Kamera)")]
    public Vector3 fixedSpawnOffset = new Vector3(0, 1.5f, 5f);

    void Awake()
    {
        Instance = this;
    }

    // --- ALTE METHODE (Bleibt für Schadenstexte) ---
    public void Show(string text, Vector3 position, Color color, bool useFixedScreenPos = false)
    {
        if (textPrefab == null) return;
        Vector3 spawnPos = useFixedScreenPos ? GetFixedPos() : position + new Vector3(0, 1f, -2f);

        GameObject go = Instantiate(textPrefab, spawnPos, Quaternion.identity);
        go.GetComponent<FloatingText>().Setup(text, color);
    }

    // --- NEUE METHODE (Für Items) ---
    public void ShowIcon(Sprite icon, string optionalText, Vector3 position, bool useFixedScreenPos = false)
    {
        if (iconPrefab == null) return;

        Vector3 spawnPos = useFixedScreenPos ? GetFixedPos() : position + new Vector3(0, 1f, -2f);

        GameObject go = Instantiate(iconPrefab, spawnPos, Quaternion.identity);

        // Wir übergeben das Sprite an das neue Skript
        go.GetComponent<FloatingIcon>().Setup(icon, optionalText, Color.white);
    }

    private Vector3 GetFixedPos()
    {
        Vector3 camPos = Camera.main.transform.position;
        return new Vector3(camPos.x, camPos.y, 0) + fixedSpawnOffset;
    }
}