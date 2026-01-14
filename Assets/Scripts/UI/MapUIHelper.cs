using UnityEngine;
using UnityEngine.UI;

public class MapUIHelper : MonoBehaviour
{
    // Diese Methode rufen wir vom Button auf
    public void Click_BackToHub()
    {
        // Wir suchen den GameManager dynamisch im Code. 
        // Das funktioniert IMMER, egal woher der GameManager kommt.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToHub();
        }
        else
        {
            // Notfall-Lösung, falls man die Map direkt testet ohne GameManager
            Debug.LogWarning("Kein GameManager gefunden! Lade Hub manuell.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
        }
    }
}