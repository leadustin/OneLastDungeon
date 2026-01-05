using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Wichtig für Szenenwechsel

public class ResultWindow : MonoBehaviour
{
    [Header("UI Verknüpfungen")]
    public TMP_Text titleText;       // "Level beendet" oder "Besiegt"
    public TMP_Text goldText;        // "+ 150 Gold"
    public TMP_Text expText;         // "+ 500 EXP"

    [Header("Buttons")]
    public Button restartButton;
    public Button hubButton;

    [Header("Szenen Namen")]
    public string gameSceneName = "GameScene"; // Wie heißt deine Level-Szene?
    public string hubSceneName = "HubScene";   // Wie heißt deine Hub-Szene?

    void Start()
    {
        // Buttons verknüpfen
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (hubButton != null) hubButton.onClick.AddListener(OnHubClicked);
    }

    // Diese Funktion rufen wir auf, wenn das Fenster aufgeht
    public void Setup(bool isWin, int sessionGold, int sessionExp)
    {
        // Titel setzen
        if (titleText != null)
        {
            titleText.text = isWin ? "Level Geschafft!" : "Expedition Beendet";
            titleText.color = isWin ? Color.green : Color.white; // Optional: Farbe ändern
        }

        // Belohnungen anzeigen
        if (goldText != null) goldText.text = $"+ {sessionGold} Gold";
        if (expText != null) expText.text = $"+ {sessionExp} EXP";
    }

    void OnRestartClicked()
    {
        // Level neu laden
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnHubClicked()
    {
        // Zurück zum Hub (muss in den Build Settings sein!)
        // Falls du noch keine Hub-Szene hast, laden wir erstmal das Menü oder Level neu
        SceneManager.LoadScene(hubSceneName);
    }
}