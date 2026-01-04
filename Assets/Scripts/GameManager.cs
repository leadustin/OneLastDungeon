using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gewählte Daten")]
    public CharacterClassData selectedClass; // Hier merken wir uns die Wahl

    void Awake()
    {
        // Singleton Pattern: Es darf nur einen geben!
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Überlebt den Szenenwechsel
    }

    // Wird vom Startbildschirm aufgerufen
    public void StartGame(CharacterClassData characterClass, string sceneName)
    {
        selectedClass = characterClass;
        SceneManager.LoadScene(sceneName);
    }
}