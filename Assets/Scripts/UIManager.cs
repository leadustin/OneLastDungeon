using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Fenster Referenzen")]
    public GameObject characterPanel;
    public GameObject inventoryPanel;

    void Awake()
    {
        // Singleton Pattern (Standard)
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // Beim Start sicherstellen, dass beide Fenster zu sind
        if (characterPanel != null) characterPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    // --- CHARAKTER FENSTER ---
    // Diese Funktion kommt auf den HUD-Button UND auf das "X" im Charakter-Fenster
    public void ToggleCharacterScreen()
    {
        if (characterPanel != null)
        {
            // Kehrt den Zustand um: Wenn AN -> AUS. Wenn AUS -> AN.
            bool isActive = characterPanel.activeSelf;
            characterPanel.SetActive(!isActive);
        }
    }

    // --- INVENTAR FENSTER ---
    // Diese Funktion kommt auf den Rucksack-Button UND auf das "X" im Inventar
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            // Kehrt den Zustand um: Wenn AN -> AUS. Wenn AUS -> AN.
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
        }
    }
}