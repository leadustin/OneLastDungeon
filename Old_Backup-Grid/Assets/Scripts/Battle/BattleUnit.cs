using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    [Header("Debug Infos")]
    public string unitName;
    public float currentHealth;
    public float maxHealth;
    public bool isPlayer;

    // Referenz auf die Optik (das Skript, das wir vorher erstellt haben)
    private UnitVisuals visuals;

    void Awake()
    {
        visuals = GetComponent<UnitVisuals>();
    }

    // --- OPTION A: Setup für den Spieler (Liest aus PlayerManager) ---
    public void SetupPlayer()
    {
        isPlayer = true;

        // 1. Daten aus dem PlayerManager holen (Singelton)
        if (PlayerManager.Instance != null)
        {
            unitName = PlayerManager.Instance.playerName;
            maxHealth = PlayerManager.Instance.maxHealth;
            currentHealth = PlayerManager.Instance.currentHealth;
        }
        else
        {
            Debug.LogError("Kein PlayerManager in der Szene gefunden!");
            unitName = "Unknown Hero";
        }

        gameObject.name = "Hero_Token";

        // 2. Bild setzen (Aus dem GameManager -> Selected Class)
        if (visuals != null)
        {
            Sprite heroIcon = null;

            // Versuchen, das Icon aus der gewählten Klasse zu holen
            if (GameManager.Instance != null && GameManager.Instance.selectedClass != null)
            {
                heroIcon = GameManager.Instance.selectedClass.classIcon;
            }

            // Fallback, falls man direkt in der Szene startet ohne Menü
            if (heroIcon == null && PlayerManager.Instance != null && PlayerManager.Instance.debugStartClass != null)
            {
                heroIcon = PlayerManager.Instance.debugStartClass.classIcon;
            }

            visuals.SetData(heroIcon);

            // WICHTIG: Helden schauen bei dir nach links -> also Spiegeln, damit sie nach rechts gucken
            visuals.FlipOrientation(true);
        }
    }

    // --- OPTION B: Setup für Gegner (Liest aus deiner EnemyData) ---
    public void SetupEnemy(EnemyData data)
    {
        isPlayer = false;

        if (data == null)
        {
            Debug.LogError("SetupEnemy wurde ohne EnemyData aufgerufen!");
            return;
        }

        // 1. Werte aus deiner EnemyData
        unitName = data.cardName; // Erbt von CardData
        maxHealth = data.health;
        currentHealth = maxHealth;

        gameObject.name = "Enemy_" + unitName;

        // 2. Bild setzen (EnemyData erbt von CardData -> hat 'artwork')
        if (visuals != null)
        {
            visuals.SetData(data.artwork);

            // WICHTIG: Deine Gegner-Bilder schauen nach links -> Passt perfekt für die rechte Seite
            visuals.FlipOrientation(false);
        }
    }
}