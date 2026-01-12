using UnityEngine;
using System.Collections.Generic;

// Die verschiedenen Arten von Texten
public enum CombatTextType
{
    NormalDamage,
    CriticalHit,
    Heal,
    SkillName,
    Miss,
    Block,
    Gold // <--- NEU
}

// Einstellungen für den Inspector
[System.Serializable]
public struct CombatTextStyle
{
    public CombatTextType type;
    public Color color;
    public float fontSize;
    public bool isBold;
    public string prefix; // z.B. "+" oder "-"
}

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [Header("Prefabs")]
    public GameObject textPrefab;
    public GameObject iconPrefab; // <--- WICHTIG: Hier dein FloatingIcon_Prefab reinziehen!

    [Header("Styles")]
    public List<CombatTextStyle> styles;

    private Dictionary<CombatTextType, CombatTextStyle> styleLookup;

    void Awake()
    {
        Instance = this;
        // Styles in Dictionary laden für schnelleren Zugriff
        styleLookup = new Dictionary<CombatTextType, CombatTextStyle>();
        foreach (var style in styles)
        {
            if (!styleLookup.ContainsKey(style.type))
                styleLookup.Add(style.type, style);
        }
    }

    // --- TEXT ANZEIGEN (Das neue System) ---
    public void Show(CombatTextType type, string text, Vector3 worldPos)
    {
        if (textPrefab == null) return;

        // 1. Style suchen (oder Default nehmen)
        CombatTextStyle style = styleLookup.ContainsKey(type) ? styleLookup[type] : GetDefaultStyle();

        // 2. Text bauen (Prefix + Text)
        string finalText = style.prefix + text;

        // 3. Position leicht variieren
        Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.5f, 1f), 0);
        Vector3 spawnPos = worldPos + offset;

        // 4. Spawnen
        GameObject go = Instantiate(textPrefab, spawnPos, Quaternion.identity);

        // 5. Konfigurieren
        go.GetComponent<FloatingText>().Setup(finalText, style.color, style.fontSize, style.isBold);
    }

    // --- ICON ANZEIGEN (Wiederhergestellt!) ---
    public void ShowIcon(Sprite icon, string optionalText, Vector3 worldPos)
    {
        if (iconPrefab == null) return;

        Vector3 offset = new Vector3(0, 1f, 0);
        GameObject go = Instantiate(iconPrefab, worldPos + offset, Quaternion.identity);

        // Wir nutzen Weiß als Standardfarbe für Icons
        go.GetComponent<FloatingIcon>().Setup(icon, optionalText, Color.white);
    }

    private CombatTextStyle GetDefaultStyle()
    {
        return new CombatTextStyle { color = Color.white, fontSize = 6, isBold = false, prefix = "" };
    }
}