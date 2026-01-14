using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Wichtig für Klicks

public class TurnOrderSlot : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referenzen")]
    public Image frameImage;
    public Image iconImage;

    [Header("Zusatz-Infos")]
    public Image markerImage; // Das kleine Form-Icon (● ■ ▲)
    public Image statusImage; // Für Stun/Sleep Icon (optional)

    private BattleUnit linkedUnit;

    // Daten setzen
    public void Setup(BattleUnit unit, Sprite markerSprite)
    {
        linkedUnit = unit;

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = unit.GetIcon();
            iconImage.preserveAspect = true;
        }

        // Rahmenfarbe
        if (frameImage != null)
        {
            // Grün für Spieler, Rot für Gegner
            frameImage.color = unit.isPlayerTeam ? Color.green : Color.red;
        }

        // Marker (Formen)
        if (markerImage != null)
        {
            if (unit.markerIndex >= 0 && markerSprite != null)
            {
                markerImage.gameObject.SetActive(true);
                markerImage.sprite = markerSprite;
            }
            else
            {
                markerImage.gameObject.SetActive(false);
            }
        }

        // Status Check (Beispiel für Stun)
        if (statusImage != null)
        {
            // Prüfen ob gestunnt (Logic muss in BattleUnit existieren)
            // Hier vereinfacht: Wir prüfen activeEffects
            bool isStunned = false;
            // foreach(var eff in unit.activeEffects) if(eff.type == StatusEffectType.Stun) isStunned = true;

            statusImage.gameObject.SetActive(isStunned);
        }
    }

    // --- INTERAKTION (Bidirektional) ---

    // Wenn Spieler auf das UI-Icon tippt -> Gegner im Feld leuchtet
    public void OnPointerDown(PointerEventData eventData)
    {
        if (linkedUnit != null && BattleSystem.Instance != null)
        {
            BattleSystem.Instance.HighlightUnitInWorld(linkedUnit);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Optional: Highlight wieder ausmachen beim Loslassen? 
        // Oder anlassen, bis jemand anderes gewählt wird.
        // BattleSystem.Instance.HighlightUnitInWorld(null); 
    }

    // Highlight dieses UI-Slots (wenn Gegner im Feld getippt wurde)
    public void SetVisualHighlight(bool active)
    {
        // Slot wird z.B. etwas größer oder Rahmen wird weiß
        transform.localScale = active ? Vector3.one * 1.2f : Vector3.one;
        if (frameImage != null) frameImage.color = active ? Color.white : (linkedUnit.isPlayerTeam ? Color.green : Color.red);
    }
}