using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System; // Wichtig für Actions

public class MissionDisplay : MonoBehaviour
{
    [Header("UI Verknüpfungen")]
    public TMP_Text titleText;
    public TMP_Text objectiveText;
    public Button fightButton; // Dein "Fight!" Button

    // Ein "Event", in das sich der GridManager einklinken kann
    public Action onFightClicked;

    void Start()
    {
        if (fightButton != null)
        {
            fightButton.onClick.AddListener(OnFightButtonPress);
        }
    }

    public void SetMissionInfo(string levelName, string objective)
    {
        if (titleText != null) titleText.text = levelName;
        if (objectiveText != null) objectiveText.text = objective;
    }

    void OnFightButtonPress()
    {
        // 1. Dem Manager Bescheid sagen: "Es geht los!"
        onFightClicked?.Invoke();

        // 2. Fenster zerstören
        Destroy(gameObject);
    }
}