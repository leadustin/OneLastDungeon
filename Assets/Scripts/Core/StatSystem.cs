using System.Collections.Generic;
using System;
using UnityEngine;

// 1. Der Modifikator
[Serializable]
public class StatModifier
{
    public float Value;
    public StatModType Type;
    public int Order;
    public object Source;

    public StatModifier(float value, StatModType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }

    public StatModifier(float value, StatModType type) : this(value, type, (int)type, null) { }
}

// 2. Die Stat-Klasse (Das Herzstück)
[Serializable]
public class Stat
{
    // Wir machen BaseValue zu einer Property, keine einfache Variable!
    // Aber für diesen Fix entfernen wir die komplizierte "Dirty"-Logik komplett.
    public float BaseValue;

    protected readonly List<StatModifier> statModifiers;

    public Stat()
    {
        statModifiers = new List<StatModifier>();
    }

    public Stat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    // Hier passiert die Magie: Wir berechnen den Wert IMMER frisch.
    // Das verhindert, dass alte "0"-Werte hängen bleiben.
    public float Value
    {
        get
        {
            return CalculateFinalValue();
        }
    }

    public void AddModifier(StatModifier mod)
    {
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        return statModifiers.Remove(mod);
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            if (statModifiers[i].Source == source)
            {
                didRemove = true;
                statModifiers.RemoveAt(i);
            }
        }
        return didRemove;
    }

    protected virtual int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Order < b.Order) return -1;
        else if (a.Order > b.Order) return 1;
        return 0;
    }

    protected virtual float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // Wenn keine Modifier da sind, gib sofort den Basis-Wert zurück!
        if (statModifiers == null || statModifiers.Count == 0)
        {
            return finalValue;
        }

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
                if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
                {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0;
                }
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= 1 + mod.Value;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}