using UnityEngine;
using UnityEngine.UI; // Wichtig für Image/Slider
using TMPro;

public class HeroStatusPanel : MonoBehaviour
{
    [Header("UI Referenzen")]
    public Image heroIcon;
    public Image borderImage; // Optional: Leuchtet wenn dran
    public Slider hpSlider;
    public Slider atbSlider;
    public Slider limitSlider;

    // Optional: Textanzeigen
    public TextMeshProUGUI hpText;

    private BattleUnit linkedUnit;

    public void Setup(BattleUnit unit)
    {
        linkedUnit = unit;

        // Icon laden (Sicherheitscheck, falls Icon fehlt)
        if (unit.Visuals != null && unit.Visuals.unitRenderer != null)
        {
            heroIcon.sprite = unit.Visuals.unitRenderer.sprite;
        }

        // Werte initial setzen
        UpdateUI();
    }

    void Update()
    {
        if (linkedUnit == null) return;
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. HP Aktualisieren
        if (hpSlider != null)
        {
            hpSlider.maxValue = linkedUnit.maxHP;
            hpSlider.value = linkedUnit.currentHP;
        }
        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(linkedUnit.currentHP)}/{linkedUnit.maxHP}";
        }

        // 2. ATB Aktualisieren (Das Herzstück!)
        if (atbSlider != null)
        {
            atbSlider.maxValue = linkedUnit.maxATB;
            atbSlider.value = linkedUnit.currentATB;

            // Visuelles Feedback wenn voll (z.B. Farbe ändern)
            Image fill = atbSlider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                if (linkedUnit.currentATB >= linkedUnit.maxATB)
                    fill.color = Color.yellow; // Bereit!
                else
                    fill.color = Color.cyan;   // Lädt...
            }
        }

        // 3. Limit Aktualisieren
        if (limitSlider != null)
        {
            limitSlider.maxValue = linkedUnit.maxLimit;
            limitSlider.value = linkedUnit.limitGauge;
        }

        // 4. Rahmen leuchten lassen wenn aktiv
        if (borderImage != null)
        {
            // Ist diese Unit gerade vom BattleSystem als "activeUnit" gesetzt?
            bool isActive = (BattleSystem.Instance.ActiveUnit == linkedUnit);
            borderImage.color = isActive ? Color.white : Color.gray;
        }
    }
}