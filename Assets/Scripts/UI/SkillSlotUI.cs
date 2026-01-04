using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI Verknüpfungen")]
    public Image iconImage;       // Das Schwert-Icon
    public Image cooldownOverlay; // Der dunkle Kreis, der abläuft

    private SkillData mySkill;    // Welcher Skill liegt hier?

    // Diese Funktion wird gleich vom Manager aufgerufen
    public void Setup(SkillData skill)
    {
        mySkill = skill;

        if (mySkill != null)
        {
            // Skill vorhanden: Icon zeigen
            iconImage.sprite = mySkill.icon;
            iconImage.enabled = true;

            // Overlay resetten
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        }
        else
        {
            // Kein Skill (Slot leer): Icon verstecken
            iconImage.enabled = false;
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        }
    }

    void Update()
    {
        // Kümmert sich um die Animation des Cooldowns
        if (mySkill != null && cooldownOverlay != null)
        {
            float ratio = PlayerManager.Instance.GetSkillCooldownRatio(mySkill);
            cooldownOverlay.fillAmount = ratio;
        }
    }

    // Das rufen wir auf, wenn man draufklickt
    public void OnClick()
    {
        if (mySkill != null)
        {
            PlayerManager.Instance.TryUseSkill(mySkill);
        }
    }
}