using System.Collections.Generic;
using UnityEngine;

// WICHTIG: Kein MonoBehaviour! Das hier sind reine Daten im RAM.
[System.Serializable]
public class GameItem
{
    // Welcher Bauplan wurde genutzt?
    public ItemTemplate template;

    // Stapelgröße (für Tränke/Material)
    public int stackCount = 1;

    // Die ECHTEN Werte dieses spezifischen Items (nur für Equipment relevant)
    // Key: StatType (z.B. Stärke), Value: Wert (z.B. +7)
    public Dictionary<StatType, int> rolledStats = new Dictionary<StatType, int>();

    // Konstruktor
    public GameItem(ItemTemplate tmpl, int amount = 1)
    {
        template = tmpl;
        stackCount = amount;
    }

    // Helfer: Gib mir den Wert für Stat X (oder 0 wenn nicht vorhanden)
    public int GetStat(StatType type)
    {
        if (rolledStats.ContainsKey(type)) return rolledStats[type];
        return 0;
    }

    public bool IsEquipment()
    {
        return template is EquipmentTemplate;
    }
}