using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("Konfiguration")]
    public EquipmentSlot slotType;

    [Header("UI Referenzen")]
    public Image iconImage;
    public Sprite placeholderIcon;
    public Button button;

    // HIER WAR DER FEHLER: Typ angepasst
    private InventoryMenu parentUI;
    private HeroRuntimeData currentHero;
    private InventoryMenu currentMenu;

    // Methode angepasst
    public void Setup(HeroRuntimeData hero, InventoryMenu menu)
    {
        currentHero = hero;
        currentMenu = menu;

        // Prüfen, ob der Held an diesem Slot (z.B. MainHand) etwas trägt
        GameItem itemInSlot = null;
        if (hero != null && hero.equipment.ContainsKey(slotType))
        {
            itemInSlot = hero.equipment[slotType];
        }

        // --- HIER IST DER FIX ---
        if (itemInSlot != null)
        {
            // --- HIER EINFÜGEN ---
            Debug.Log($"BINGO! Slot {slotType} hat Item {itemInSlot.template.itemName}. Schalte Bild an!");
            // ---------------------
            // 1. Bild setzen
            iconImage.sprite = itemInSlot.template.icon;

            // 2. WICHTIG: Bild einschalten! (Falls es vorher aus war)
            iconImage.enabled = true;

            // 3. WICHTIG: Farbe auf Weiß setzen (Falls es transparent war)
            iconImage.color = Color.white;
        }
        else
        {
            // Slot ist leer
            iconImage.sprite = null;

            // Zeige Placeholder wenn vorhanden, sonst mach das Bild ganz aus
            if (placeholderIcon != null)
            {
                iconImage.sprite = placeholderIcon;
                iconImage.enabled = true;
                iconImage.color = new Color(1, 1, 1, 0.5f); // Halbtransparent
            }
            else
            {
                iconImage.enabled = false; // Bild ausschalten
            }
        }
    }

    public void OnClickSlot()
    {
        // Wir leiten den Klick an den Manager weiter
        if (currentMenu != null)
        {
            currentMenu.TryUnequipItem(slotType);
        }
    }
}