using UnityEngine;
using UnityEngine.UI;
using System; // Für Action

public class UI_FilterButton : MonoBehaviour
{
    [Header("UI Referenzen")]
    public Image iconImage;       // Das Icon des Buttons
    public Image background;      // Der Hintergrund (für Farbwechsel)
    public Button btn;            // Die Button-Komponente

    [Header("Settings")]
    public Color selectedColor = Color.green; // Farbe wenn aktiv
    public Color defaultColor = Color.white;  // Farbe wenn inaktiv

    // Speichert die Funktion, die beim Klick ausgeführt wird
    private Action onClickCallback;

    public void Setup(Sprite icon, bool isSelected, Action onClick)
    {
        if (iconImage != null) iconImage.sprite = icon;

        onClickCallback = onClick;

        // Button Listener aufräumen und neu setzen
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);

        SetSelected(isSelected);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
        {
            background.color = isSelected ? selectedColor : defaultColor;
        }
    }

    void OnClick()
    {
        // Ruft die Funktion im Manager auf
        onClickCallback?.Invoke();
    }
}