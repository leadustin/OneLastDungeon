using UnityEngine;
using System.Collections.Generic; // Wichtig für Listen

public class DebugCheat : MonoBehaviour
{
    [Header("Cheat Einstellungen")]
    [Tooltip("Ziehe hier alle Items rein, die zufällig erscheinen können (Schwerter, Tränke, etc.)")]
    public List<ItemTemplate> possibleLoot;

    [Tooltip("Wie viele Items sollen pro Tastendruck (K) hinzugefügt werden?")]
    public int amountToAdd = 10;

    void Update()
    {
        // Wenn K gedrückt wird
        if (Input.GetKeyDown(KeyCode.K))
        {
            GiveRandomLoot();
        }
    }

    void GiveRandomLoot()
    {
        if (PlayerInventory.Instance == null)
        {
            Debug.LogError("Fehler: PlayerInventory nicht in der Szene gefunden!");
            return;
        }

        if (possibleLoot == null || possibleLoot.Count == 0)
        {
            Debug.LogWarning("Fehler: Die Liste 'Possible Loot' im Inspector ist leer!");
            return;
        }

        // Schleife: So oft wiederholen, wie in 'amountToAdd' steht
        for (int i = 0; i < amountToAdd; i++)
        {
            // 1. Zufälliges Template aus der Liste picken
            int randomIndex = Random.Range(0, possibleLoot.Count);
            ItemTemplate selectedTemplate = possibleLoot[randomIndex];

            // 2. Echtes Item generieren (WICHTIG: Nutzt ItemGenerator für Random Stats!)
            GameItem newItem = ItemGenerator.Generate(selectedTemplate);

            // 3. Ins Inventar legen
            PlayerInventory.Instance.AddItem(newItem);
        }

        Debug.Log($"CHEAT: {amountToAdd} zufällige Items hinzugefügt.");

        // 4. UI sofort aktualisieren (falls das Fenster offen ist)
        if (InventoryMenu.Instance != null && InventoryMenu.Instance.gameObject.activeSelf)
        {
            InventoryMenu.Instance.RefreshUI();
        }
    }
}