using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject blacksmithPanel;
    public GameObject merchantPanel;
    public GameObject tavernPanel;

    [Header("Game Scene Name")]
    public string gameSceneName = "GameScene"; // Wie heißt deine Dungeon-Szene?

    // --- Button Funktionen ---

    public void OnBlacksmithClicked()
    {
        blacksmithPanel.SetActive(true);
    }

    public void OnMerchantClicked()
    {
        merchantPanel.SetActive(true);
    }

    public void OnTavernClicked()
    {
        tavernPanel.SetActive(true);
    }

    public void OnDungeonClicked()
    {
        // Reset für neuen Run
        if (GridManager.Instance != null)
        {
            // GridManager.savedLevelIndex = 0; // Falls wir das statisch gemacht haben
        }

        // Szene laden
        SceneManager.LoadScene(gameSceneName);
    }

    // --- Allgemeine Schließen-Funktion ---

    public void ClosePanel(GameObject panelToClose)
    {
        panelToClose.SetActive(false);
    }
}