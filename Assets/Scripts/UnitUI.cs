using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// Konfiguration für Status-Effekte (DoT)
[System.Serializable]
public class StatusTextConfig
{
    [InspectorName("Effekt Typ")]
    public StatusEffectType type;

    [InspectorName("Präfix Text")]
    public string prefixText = "";

    [InspectorName("Text Farbe")]
    public Color color = Color.red;

    [InspectorName("Schriftart (Optional)")]
    public TMP_FontAsset fontOverride;

    [InspectorName("Standard Icon (Optional)")]
    public Sprite defaultIcon;
}

// Konfiguration für Treffer-Arten
[System.Serializable]
public class HitTypeConfig
{
    [InspectorName("Treffer Typ")]
    public HitType type;

    [InspectorName("Text Farbe")]
    public Color color = Color.white;

    [InspectorName("Größen Multiplier")]
    public float sizeMult = 1.0f;

    [InspectorName("Text Override")]
    public string textOverride = "";
}

public class UnitUI : MonoBehaviour
{
    [Header("Verknüpfungen")]
    public GameObject damageTextPrefab;

    // HIER GEÄNDERT: GameObject statt Transform (löst das Drag & Drop Problem)
    public GameObject damageTextSpawnPoint;

    public Image healthBarFill;

    [Header("Einstellungen")]
    public float barChangeSpeed = 5f;

    [Header("Styles & Farben")]
    public List<HitTypeConfig> hitConfigs;
    public List<StatusTextConfig> statusConfigs;

    private float targetFillAmount = 1f;

    void Update()
    {
        if (healthBarFill != null)
        {
            if (Mathf.Abs(healthBarFill.fillAmount - targetFillAmount) > 0.001f)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * barChangeSpeed);
            }
        }
    }

    public void UpdateHealthBar(float currentHP, float maxHP, bool instant = false)
    {
        if (healthBarFill == null) return;
        if (maxHP <= 0) { targetFillAmount = 0; return; }

        targetFillAmount = currentHP / maxHP;
        if (instant) healthBarFill.fillAmount = targetFillAmount;
    }

    public void SpawnDamageText(float amount, HitType hitType, StatusEffectType statusType = StatusEffectType.None, Sprite iconOverride = null)
    {
        if (damageTextPrefab == null) return;

        // --- Daten vorbereiten (Farben, Icons etc.) ---
        Color finalColor = Color.white;
        float finalSize = 1f;
        string finalText = Mathf.RoundToInt(amount).ToString();
        Sprite finalIcon = iconOverride;
        TMP_FontAsset finalFont = null;

        // 1. Status Check
        StatusTextConfig sConf = statusConfigs.Find(x => x.type == statusType);
        if (sConf != null)
        {
            finalColor = sConf.color;
            if (!string.IsNullOrEmpty(sConf.prefixText)) finalText = sConf.prefixText + finalText;
            if (sConf.fontOverride != null) finalFont = sConf.fontOverride;
            if (finalIcon == null) finalIcon = sConf.defaultIcon;
        }

        // 2. HitType Check
        HitTypeConfig hConf = hitConfigs.Find(x => x.type == hitType);
        if (hConf != null)
        {
            if (statusType == StatusEffectType.None) finalColor = hConf.color;
            finalSize = hConf.sizeMult;
            if (!string.IsNullOrEmpty(hConf.textOverride))
            {
                finalText = hConf.textOverride;
                if (hitType == HitType.Miss) finalIcon = null;
            }
        }

        // --- SPAWNEN ---
        // Sicherheitscheck: Haben wir einen SpawnPoint?
        Vector3 spawnPos = transform.position; // Fallback: Position der Unit selbst
        if (damageTextSpawnPoint != null)
        {
            // HIER GEÄNDERT: Wir greifen auf .transform.position zu, da die Variable jetzt ein GameObject ist
            spawnPos = damageTextSpawnPoint.transform.position;
        }

        GameObject go = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        DamagePopup popup = go.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(finalText, finalColor, finalSize, finalIcon, finalFont);
        }
    }
}