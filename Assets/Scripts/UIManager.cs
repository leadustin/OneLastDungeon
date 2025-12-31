using UnityEngine;
using UnityEngine.UI; // Wichtig für UI Elemente

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Fenster")]
    public GameObject characterPanel; // Ziehe hier dein 'Panel_CharacterScreen' rein

    void Awake()
    {
        // Singleton Pattern (damit wir von überall darauf zugreifen können)
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // Sicherstellen, dass das Inventar beim Spielstart ZU ist
        if (characterPanel != null)
            characterPanel.SetActive(false);
    }

    // Diese Methode rufen wir mit dem Rucksack-Button auf
    public void OpenCharacterScreen()
    {
        characterPanel.SetActive(true);

        // --- DIESE ZEILEN SIND ENTSCHEIDEND! ---
        if (InventoryUI.Instance != null)
        {
            Debug.Log("UIManager: Rufe RefreshInventory auf..."); // Zum Testen
            InventoryUI.Instance.RefreshInventory();
        }
        else
        {
            Debug.LogError("UIManager: InventoryUI Instance nicht gefunden!");
        }
    }

    // Diese Methode rufen wir mit dem X-Button auf
    public void CloseCharacterScreen()
    {
        characterPanel.SetActive(false);

        // Optional: Spiel weiterlaufen lassen
        // Time.timeScale = 1f;
    }
}