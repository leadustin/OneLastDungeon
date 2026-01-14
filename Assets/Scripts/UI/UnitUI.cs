using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

// --- KONFIGURATIONEN ---

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

// --- HAUPT KLASSE ---

public class UnitUI : MonoBehaviour
{
    [Header("Verknüpfungen")]
    public GameObject damageTextPrefab;
    public GameObject damageTextSpawnPoint;

    [Header("UI Elemente")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI nameText;

    [Header("Configs")]
    public List<StatusTextConfig> statusConfigs;
    public List<HitTypeConfig> hitConfigs;

    private BattleUnit linkedUnit;

    public void SetupUI(BattleUnit unit)
    {
        linkedUnit = unit;

        if (nameText != null)
        {
            nameText.text = unit.unitName;
        }

        UpdateHealthBar(unit.currentHP, unit.maxHP, true);
    }

    public void UpdateHealthBar(float current, float max, bool instant = false)
    {
        if (healthBarFill != null)
        {
            float targetFill = max > 0 ? current / max : 0;
            healthBarFill.fillAmount = targetFill;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(current)}/{Mathf.Ceil(max)}";
        }
    }

    public void SpawnDamageText(float amount, HitType hitType, StatusEffectType statusType = StatusEffectType.None, Sprite skillIcon = null)
    {
        // 1. Basis-Werte berechnen
        string finalText = Mathf.CeilToInt(amount).ToString();
        Color finalColor = Color.white;
        float finalSize = 1f;
        TMP_FontAsset finalFont = null;
        Sprite finalIcon = skillIcon; // Standardmäßig das Skill-Icon nutzen

        // 2. Status Check (Gift, Feuer etc.)
        StatusTextConfig sConf = statusConfigs.Find(x => x.type == statusType);
        if (sConf != null)
        {
            finalColor = sConf.color;
            if (!string.IsNullOrEmpty(sConf.prefixText)) finalText = sConf.prefixText + finalText;
            if (sConf.fontOverride != null) finalFont = sConf.fontOverride;

            // Wenn kein Skill-Icon da ist, nimm das Status-Icon (z.B. Totenkopf für Gift)
            if (finalIcon == null) finalIcon = sConf.defaultIcon;
        }

        // 3. HitType Check (Crit, Miss etc.)
        HitTypeConfig hConf = hitConfigs.Find(x => x.type == hitType);
        if (hConf != null)
        {
            // Farbe nur überschreiben, wenn es kein Status-Effekt ist (damit Gift lila bleibt, auch bei Crits)
            if (statusType == StatusEffectType.None) finalColor = hConf.color;

            finalSize = hConf.sizeMult;

            if (!string.IsNullOrEmpty(hConf.textOverride))
            {
                finalText = hConf.textOverride; // z.B. "MISS" statt "0"
                if (hitType == HitType.Miss) finalIcon = null; // Kein Icon bei Miss
            }
        }

        // --- SPAWNEN ---
        Vector3 spawnPos = transform.position;
        if (damageTextSpawnPoint != null)
        {
            spawnPos = damageTextSpawnPoint.transform.position;
        }

        if (damageTextPrefab != null)
        {
            GameObject go = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

            // WICHTIG: Hier verbinden wir uns mit deinem DamagePopup Skript!
            DamagePopup popup = go.GetComponent<DamagePopup>();
            if (popup != null)
            {
                // Hier rufen wir DEINE Methode mit den richtigen Parametern auf
                popup.Setup(finalText, finalColor, finalSize, finalIcon, finalFont);
            }
        }
    }
}