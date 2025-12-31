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
        // Singleton, damit der PlayerManager uns einfach findet
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // Optional: Am Anfang einmal Standardwerte setzen
        UpdateLevel(1);
        SetName("Held"); // Oder den echten Namen laden
    }

    // --- Diese Methoden werden von auﬂen aufgerufen ---

    public void UpdateHP(float currentHP, float maxHP)
    {
        if (hpBarFill != null)
        {
            // Verhindert Teilen durch 0
            if (maxHP <= 0) maxHP = 1;
            hpBarFill.fillAmount = currentHP / maxHP;
        }
    }

    public void UpdateMana(float currentMana, float maxMana)
    {
        if (manaBarFill != null)
        {
            if (maxMana <= 0) maxMana = 1;
            manaBarFill.fillAmount = currentMana / maxMana;
        }
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }
    }

    public void SetName(string name)
    {
        if (nameText != null) nameText.text = name;
    }
}