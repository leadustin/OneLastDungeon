using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHUD : MonoBehaviour
{
    public static EnemyHUD Instance; // Singleton für einfachen Zugriff

    [Header("UI Elemente")]
    public GameObject contentRoot; // Das Elternobjekt (um alles an/aus zu schalten)
    public Image enemyIcon;
    public Image hpBar;
    public TMP_Text nameText;
    public TMP_Text levelText; // Falls du das hast

    private int maxHP;

    void Awake()
    {
        // Singleton setzen
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        // HUD am Anfang verstecken
        HideHUD();
    }

    // Wird beim Kampfstart aufgerufen
    public void SetupEnemy(CardData enemyData, int currentHealth, int maxHealth)
    {
        // 1. Visuelles Zeug setzen
        if (enemyData != null)
        {
            if (nameText != null) nameText.text = enemyData.cardName;
            if (enemyIcon != null) enemyIcon.sprite = enemyData.artwork;
            // Falls du Level hast: levelText.text = "Lvl " + enemyData.level;
        }

        // 2. HP Balken initialisieren
        maxHP = maxHealth;
        UpdateHP(currentHealth);

        // 3. Sichtbar machen
        if (contentRoot != null) contentRoot.SetActive(true);
    }

    // Wird bei jedem Treffer aufgerufen
    public void UpdateHP(int currentHealth)
    {
        if (hpBar != null && maxHP > 0)
        {
            float ratio = (float)currentHealth / maxHP;
            hpBar.fillAmount = ratio;
        }
    }

    public void HideHUD()
    {
        if (contentRoot != null) contentRoot.SetActive(false);
    }
}