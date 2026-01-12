using System.Collections.Generic;
using UnityEngine;

public class StatsHandler : MonoBehaviour
{
    // Die echte Datenbank (Unsichtbar im Inspector)
    private Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

    // --- LIVE DEBUG (Nur lesen) ---
    // Nützlich um im Inspector zu sehen, ob Werte da sind, aber ohne Konsole-Spam
    [Header("LIVE DEBUG (Nur lesen)")]
    public List<StatConfig> debugView;

    void Awake()
    {
        // Initialisieren mit 0
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            stats.Add(type, new Stat(0));
        }
        UpdateDebugView();
    }

    public float GetStatValue(StatType type)
    {
        if (stats.ContainsKey(type)) return stats[type].Value;
        return 0;
    }

    public void SetBaseStat(StatType type, float value)
    {
        if (stats.ContainsKey(type))
        {
            // Keine Debug.Log mehr hier!
            stats[type].BaseValue = value;
            UpdateDebugView();
        }
    }

    public void AddStatModifier(StatType type, StatModifier mod)
    {
        if (stats.ContainsKey(type))
        {
            stats[type].AddModifier(mod);
            UpdateDebugView();
        }
    }

    public void RemoveStatModifier(StatType type, StatModifier mod)
    {
        if (stats.ContainsKey(type))
        {
            stats[type].RemoveModifier(mod);
            UpdateDebugView();
        }
    }

    // Kopiert die unsichtbaren Daten in die sichtbare Liste für den Inspector
    private void UpdateDebugView()
    {
        debugView = new List<StatConfig>();
        foreach (var pair in stats)
        {
            // Wir zeigen nur Stats an, die nicht 0 sind
            if (pair.Value.Value != 0)
            {
                debugView.Add(new StatConfig { type = pair.Key, value = pair.Value.Value });
            }
        }
    }
}