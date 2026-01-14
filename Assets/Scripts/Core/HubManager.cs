using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject blacksmithPanel;
    public GameObject merchantPanel;
    public GameObject tavernPanel;
    public GameObject characterScreenPanel;

    [Header("Navigation")]
    // Wir nennen das um, damit klar ist, dass es zur Karte geht
    // WICHTIG: In Unity im Inspector hier "MapScene" eintragen!
    public string mapSceneName = "MapScene";

    // --- Button Funktionen ---

    public void OnInventoryClicked()
    {
        if (characterScreenPanel != null)
        {
            bool isActive = characterScreenPanel.activeSelf;
            characterScreenPanel.SetActive(!isActive);
        }
    }
    public void OnBlacksmithClicked()
    {
        if (blacksmithPanel != null) blacksmithPanel.SetActive(true);
    }

    public void OnMerchantClicked()
    {
        if (merchantPanel != null) merchantPanel.SetActive(true);
    }

    public void OnTavernClicked()
    {
        if (tavernPanel != null) tavernPanel.SetActive(true);
    }

    public void OnDungeonClicked()
    {
        // ALT: GridManager Reset (Entfernt, da nicht mehr existent)

        // NEU: Wir laden einfach die Weltkarte
        Debug.Log($"Gehe zum Dungeon... Lade Szene: {mapSceneName}");
        SceneManager.LoadScene(mapSceneName);
    }

    // --- Allgemeine Schlieﬂen-Funktion ---

    public void ClosePanel(GameObject panelToClose)
    {
        if (panelToClose != null) panelToClose.SetActive(false);
    }
}