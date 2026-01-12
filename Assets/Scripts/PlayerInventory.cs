using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    [Header("Inhalt")]
    // Hier sind die ECHTEN Items gespeichert
    public List<GameItem> items = new List<GameItem>();

    public int goldCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Überlebt Levelwechsel!
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Die Methode, um Items hinzuzufügen
    public void AddItem(GameItem newItem)
    {
        if (newItem == null) return;

        // 1. Ist es Ausrüstung? -> Immer als neues Item hinzufügen (weil Stats unique sind)
        if (newItem.IsEquipment())
        {
            items.Add(newItem);
            Debug.Log($"Inventar: {newItem.template.itemName} hinzugefügt (Unique).");
            return;
        }

        // 2. Ist es stapelbar (Trank/Material)? -> Suchen ob wir es schon haben
        GameItem existingItem = items.Find(x => x.template == newItem.template);

        if (existingItem != null)
        {
            // Stapel erhöhen
            existingItem.stackCount += newItem.stackCount;
            Debug.Log($"Inventar: {newItem.template.itemName} aufgestockt auf {existingItem.stackCount}.");
        }
        else
        {
            // Neu hinzufügen
            items.Add(newItem);
            Debug.Log($"Inventar: {newItem.template.itemName} hinzugefügt (Neu).");
        }
    }

    public void AddGold(int amount)
    {
        goldCount += amount;
    }
}