using UnityEngine;

public static class ItemGenerator
{
    public static GameItem Generate(ItemTemplate template)
    {
        if (template == null) return null;

        // 1. Instanz erstellen
        GameItem newItem = new GameItem(template);

        // 2. Wenn es Ausrüstung ist -> Stats würfeln!
        if (template is EquipmentTemplate equipTemplate)
        {
            if (equipTemplate.possibleStats != null)
            {
                foreach (var range in equipTemplate.possibleStats)
                {
                    // Glücksrad: Bekommt das Item diesen Stat?
                    if (Random.value <= range.chance)
                    {
                        // Wert zwischen Min und Max würfeln (inklusive Max)
                        int finalValue = Random.Range(range.min, range.max + 1);

                        // Sicherheitscheck: Keine 0-Werte eintragen
                        if (finalValue != 0)
                        {
                            if (newItem.rolledStats.ContainsKey(range.stat))
                                newItem.rolledStats[range.stat] += finalValue;
                            else
                                newItem.rolledStats.Add(range.stat, finalValue);
                        }
                    }
                }
            }
        }

        return newItem;
    }
}