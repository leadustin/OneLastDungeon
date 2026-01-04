using UnityEngine;
using UnityEngine.UI;  // Wichtig für Toggle und Image
using TMPro;           // Wichtig für TextMeshPro

public class MainMenuUI : MonoBehaviour
{
    [Header("--- PANEL REFERENZEN ---")]
    public GameObject mainMenuPanel;
    public GameObject characterCreationRoot;

    [Header("--- INPUTS & BILD ---")]
    public TMP_InputField nameInputField; // Dein Eingabefeld
    public Image classIconDisplay;        // Das große Bild in der Mitte
    public Toggle maleToggle;             // ZIEHE HIER DEIN "ToggleMale" REIN!

    [Header("--- RECHTE LEISTE (Klasse) ---")]
    public TextMeshProUGUI classNameText;        // Der Text ZWISCHEN den Pfeilen rechts
    public TextMeshProUGUI classDescriptionText; // Der große Text darunter
    public CharacterClassData[] availableClasses; // Hier deine ScriptableObjects reinziehen (Warrior, Rogue...)

    [Header("--- LINKE LEISTE (Optionen) ---")]
    public TextMeshProUGUI raceText;       // Text zwischen den Rassen-Pfeilen
    public TextMeshProUGUI backgroundText; // Text zwischen den Herkunft-Pfeilen
    public TextMeshProUGUI keepsakeText;   // Text zwischen den Bonus-Pfeilen

    // --- INTERNE DATEN-LISTEN (String Arrays) ---
    private string[] races = { "Human", "Elf", "Dwarf", "Orc" };
    private string[] backgrounds = { "Noble", "Commoner", "Mercenary", "Outcast" };
    private string[] keepsakes = { "None", "Health Potion", "Ring of Life", "Old Key" };

    // --- MERKER FÜR AKTUELLE AUSWAHL ---
    private int currentClassIndex = 0;
    private int currentRaceIndex = 0;
    private int currentBackgroundIndex = 0;
    private int currentKeepsakeIndex = 0;

    [Header("--- SZENEN NAME ---")]
    public string gameSceneName = "Dungeon";

    void Start()
    {
        // Beim Start sicherstellen, dass das Menü offen ist und Char-Creation zu
        ShowMainMenu();
    }

    // --- MENÜ STEUERUNG ---

    void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (characterCreationRoot != null) characterCreationRoot.SetActive(false);
    }

    public void OnNewCharacterClicked()
    {
        mainMenuPanel.SetActive(false);
        characterCreationRoot.SetActive(true);

        // Name zurücksetzen
        if (nameInputField != null) nameInputField.text = "";

        // Anzeigen aktualisieren
        UpdateAllDisplays();
    }

    public void OnBackClicked()
    {
        characterCreationRoot.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnExitClicked()
    {
        Application.Quit();
    }

    // --- SPIEL STARTEN (Der wichtigste Moment) ---

    public void OnStartGameClicked()
    {
        if (availableClasses.Length == 0) return;

        // 1. Ausgewählte Daten sammeln
        CharacterClassData selectedClass = availableClasses[currentClassIndex];

        // Name: Wenn leer, nimm den Klassennamen
        string finalName = (nameInputField != null && nameInputField.text.Length > 0) ? nameInputField.text : selectedClass.className;

        string selectedRace = races[currentRaceIndex];
        // Hier könntest du auch Background und Keepsake speichern, wenn du willst

        // 2. Geschlecht bestimmen
        // Wir schauen nur auf den Male-Toggle. Wenn der AN ist -> Male. Sonst -> Female.
        string gender = "Female";
        if (maleToggle != null && maleToggle.isOn)
        {
            gender = "Male";
        }

        // 3. Speichern für die nächste Szene (PlayerManager)
        GameManager.Instance.selectedClass = selectedClass;
        PlayerPrefs.SetString("PlayerName", finalName);
        PlayerPrefs.SetString("PlayerRace", selectedRace);
        PlayerPrefs.SetString("PlayerGender", gender);

        // 4. Szene wechseln
        GameManager.Instance.StartGame(selectedClass, gameSceneName);
    }

    // --- NAVIGATION: KLASSE (Rechts) ---
    public void OnNextClassClicked()
    {
        currentClassIndex = (currentClassIndex + 1) % availableClasses.Length;
        UpdateAllDisplays();
    }
    public void OnPrevClassClicked()
    {
        currentClassIndex--;
        if (currentClassIndex < 0) currentClassIndex = availableClasses.Length - 1;
        UpdateAllDisplays();
    }

    // --- NAVIGATION: RASSE (Links) ---
    public void OnNextRaceClicked()
    {
        currentRaceIndex = (currentRaceIndex + 1) % races.Length;
        UpdateAllDisplays();
    }
    public void OnPrevRaceClicked()
    {
        currentRaceIndex--;
        if (currentRaceIndex < 0) currentRaceIndex = races.Length - 1;
        UpdateAllDisplays();
    }

    // --- NAVIGATION: HINTERGRUND (Links) ---
    public void OnNextBackgroundClicked()
    {
        currentBackgroundIndex = (currentBackgroundIndex + 1) % backgrounds.Length;
        UpdateAllDisplays();
    }
    public void OnPrevBackgroundClicked()
    {
        currentBackgroundIndex--;
        if (currentBackgroundIndex < 0) currentBackgroundIndex = backgrounds.Length - 1;
        UpdateAllDisplays();
    }

    // --- NAVIGATION: BONUS (Links) ---
    public void OnNextKeepsakeClicked()
    {
        currentKeepsakeIndex = (currentKeepsakeIndex + 1) % keepsakes.Length;
        UpdateAllDisplays();
    }
    public void OnPrevKeepsakeClicked()
    {
        currentKeepsakeIndex--;
        if (currentKeepsakeIndex < 0) currentKeepsakeIndex = keepsakes.Length - 1;
        UpdateAllDisplays();
    }


    // --- VISUELLE UPDATES ---
    // Diese Funktion schreibt die richtigen Texte in die Felder und tauscht das Bild
    void UpdateAllDisplays()
    {
        // 1. Klasse Update (Rechts)
        if (availableClasses.Length > 0)
        {
            CharacterClassData data = availableClasses[currentClassIndex];

            // Name zwischen den Pfeilen
            if (classNameText != null) classNameText.text = data.className;

            // Beschreibung darunter
            if (classDescriptionText != null) classDescriptionText.text = data.description;

            // Großes Bild
            if (classIconDisplay != null) classIconDisplay.sprite = data.classIcon;
        }

        // 2. Optionen Update (Links)
        if (raceText != null) raceText.text = races[currentRaceIndex];
        if (backgroundText != null) backgroundText.text = backgrounds[currentBackgroundIndex];
        if (keepsakeText != null) keepsakeText.text = keepsakes[currentKeepsakeIndex];
    }
}