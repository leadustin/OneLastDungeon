using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Daten")]
    public LevelTemplate levelData; // Zieh hier Level 1-1 rein
    public string requiredLevelID;  // Welches Level muss man vorher schaffen? (z.B. "1-1" für Button 1-2). Leer lassen für Level 1.

    [Header("UI")]
    public Button myButton;
    public Image lockIcon; // Optional: Ein Schloss-Symbol

    void Start()
    {
        if (myButton == null) myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnLevelClicked);

        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (GameManager.Instance == null) return;

        // Prüfen ob gesperrt
        bool isLocked = false;

        // Wenn es eine Bedingung gibt (requiredLevelID nicht leer)
        if (!string.IsNullOrEmpty(requiredLevelID))
        {
            // Ist das Vor-Level NICHT geschafft? -> Sperren
            if (!GameManager.Instance.IsLevelCompleted(requiredLevelID))
            {
                isLocked = true;
            }
        }

        myButton.interactable = !isLocked;
        if (lockIcon != null) lockIcon.enabled = isLocked;
    }

    void OnLevelClicked()
    {
        if (GameManager.Instance != null && levelData != null)
        {
            // 1. Dem Manager sagen, was wir spielen wollen
            GameManager.Instance.levelToLoad = levelData;

            // 2. Kampfszene laden (Achte darauf, dass deine Szene exakt so heißt!)
            GameManager.Instance.LoadScene("BattleScene");
        }
    }
}