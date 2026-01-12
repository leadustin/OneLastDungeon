using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultWindow : MonoBehaviour
{
    [Header("UI Verknüpfungen")]
    public TMP_Text titleText;
    public TMP_Text goldText;
    public TMP_Text expText;

    [Header("Buttons")]
    public Button restartButton; // Für Game Over
    public Button hubButton;     // Immer da
    public Button nextLevelButton; // NEU: Für Sieg

    [Header("Szenen Namen")]
    public string hubSceneName = "HubScene";

    void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (hubButton != null) hubButton.onClick.AddListener(OnHubClicked);

        // NEU: Listener für den Weiter-Button
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    public void Setup(bool isWin, int sessionGold, int sessionExp)
    {
        // 1. Texte setzen
        if (titleText != null)
        {
            titleText.text = isWin ? "Level Geschafft!" : "Gescheitert";
            titleText.color = isWin ? Color.green : Color.red;
        }

        if (goldText != null) goldText.text = $"+ {sessionGold} Gold";
        if (expText != null) expText.text = $"+ {sessionExp} EXP";

        // 2. Buttons anpassen (Sieg vs. Niederlage)
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(isWin);     // Nur bei Sieg
        if (restartButton != null) restartButton.gameObject.SetActive(!isWin);       // Nur bei Niederlage

        // Hub Button ist immer da (oder optional auch nur bei Win, wie du magst)
        if (hubButton != null) hubButton.gameObject.SetActive(true);
    }

    void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnNextLevelClicked()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.NextLevel();
        }
        Destroy(this.gameObject);
    }

    void OnHubClicked()
    {
        // 1. Die Hub-Szene laden (Der Name muss im Inspector eingetragen sein!)
        SceneManager.LoadScene(hubSceneName);

        // 2. Dieses Fenster zerstören, damit es nicht in der neuen Szene herumgeistert
        Destroy(this.gameObject);
    }
}