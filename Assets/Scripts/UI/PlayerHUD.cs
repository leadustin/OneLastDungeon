using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("Balken (Filled Images aus dem Prefab ziehen)")]
    public Image hpBarFill;
    public Image manaBarFill;

    [Header("Texte (TMP aus dem Prefab ziehen)")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void UpdateHP(float currentHP, float maxHP)
    {
        if (hpBarFill != null)
        {
            float fill = (maxHP > 0) ? currentHP / maxHP : 0;
            hpBarFill.fillAmount = fill;
        }
    }

    public void UpdateMana(float currentMana, float maxMana)
    {
        if (manaBarFill != null)
        {
            float fill = (maxMana > 0) ? currentMana / maxMana : 0;
            manaBarFill.fillAmount = fill;
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null) levelText.text = level.ToString();
    }

    public void SetName(string name)
    {
        if (nameText != null) nameText.text = name;
    }
}