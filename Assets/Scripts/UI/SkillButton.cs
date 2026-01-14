using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    [Header("UI Referenzen")]
    public Image iconImage;
    public Button btn;
    public TextMeshProUGUI cooldownText;

    // WICHTIG: Das muss ein Image sein, kein GameObject, damit wir .fillAmount nutzen können
    public Image cooldownOverlay;

    private RuntimeSkill mySkill;

    public void Setup(RuntimeSkill skill, System.Action<RuntimeSkill> onClickAction)
    {
        mySkill = skill;

        // Icon setzen
        if (skill.template.icon != null) iconImage.sprite = skill.template.icon;

        // Button Klick
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickAction(mySkill));

        UpdateState();
    }

    public void UpdateState()
    {
        // 1. Maximale Dauer ermitteln (Recharge oder Initial Delay, je nachdem was größer ist, um Division durch 0 zu vermeiden)
        // Wir nehmen im Zweifel einfach den Recharge-Wert als Basis für die Anzeige.
        float maxDuration = Mathf.Max(mySkill.template.rechargeTurns, 1);

        // 2. Zustand prüfen
        if (mySkill.currentCooldown > 0)
        {
            // --- SKILL IST AUF COOLDOWN ---
            btn.interactable = false;

            // Text anzeigen
            if (cooldownText)
            {
                cooldownText.gameObject.SetActive(true);
                cooldownText.text = mySkill.currentCooldown.ToString();
            }

            // Overlay berechnen (Prozent)
            if (cooldownOverlay)
            {
                cooldownOverlay.gameObject.SetActive(true);

                // Beispiel: 2 Runden Rest bei 3 Runden Max = 0.66 (66% gefüllt)
                float fillValue = (float)mySkill.currentCooldown / maxDuration;

                // Falls wir im "Initial Delay" sind (Startverzögerung), kann der Wert > 1 sein.
                // Wir klammern ihn auf maximal 1.
                cooldownOverlay.fillAmount = Mathf.Clamp01(fillValue);
            }
        }
        else
        {
            // --- SKILL IST BEREIT ---
            btn.interactable = true;

            if (cooldownText) cooldownText.gameObject.SetActive(false);

            if (cooldownOverlay)
            {
                cooldownOverlay.fillAmount = 0; // Overlay weg
                cooldownOverlay.gameObject.SetActive(false);
            }
        }
    }
}