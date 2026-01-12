using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spieler Fortschritt")]
    public int currentGold;
    public int currentXP;

    // Liste der IDs von geschafften Levels (z.B. "1-1", "1-2")
    public List<string> completedLevels = new List<string>();

    [Header("Navigation")]
    // Welches Level soll die Battle-Szene laden?
    public LevelTemplate levelToLoad;

    void Awake()
    {
        // Singleton Pattern: Es darf nur einen geben!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Überlebt Szenenwechsel
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRewards(int gold, int xp)
    {
        currentGold += gold;
        currentXP += xp;
        Debug.Log($"Gespeichert: {currentGold} Gold, {currentXP} XP");
    }

    public void MarkLevelComplete(string levelID)
    {
        if (!completedLevels.Contains(levelID))
        {
            completedLevels.Add(levelID);
        }
    }

    public bool IsLevelCompleted(string levelID)
    {
        return completedLevels.Contains(levelID);
    }

    // Hilfsmethode zum Szenenwechsel
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToHub()
    {
        Debug.Log("Gehe zurück zum Hub...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
    }

    public void GoToMap()
    {
        Debug.Log("Gehe zur Karte...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
    }
}