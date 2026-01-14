using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButton : MonoBehaviour
{
    [Header("Referenzen")]
    public Button btn;
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI cooldownText; // "CD: 2"
    public GameObject disabledOverlay;   // Halbtransparentes Schwarz

    private RuntimeSkill linkedSkill;
    private System.Action<RuntimeSkill> onClickCallback;

    public void SetupSkill(RuntimeSkill skill, System.Action<RuntimeSkill> callback)
    {
        linkedSkill = skill;
        onClickCallback = callback;

        // UI füllen
        if (skill.template.icon != null) iconImage.sprite = skill.template.icon;
        nameText.text = skill.template.skillName;

        // Cooldown Check
        bool isReady = skill.IsReady();
        btn.interactable = isReady;

        if (disabledOverlay != null) disabledOverlay.SetActive(!isReady);

        if (!isReady && cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.text = $"{skill.currentCooldown}";
        }
        else if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }

        // Klick Event
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClickCallback?.Invoke(linkedSkill));
    }
}