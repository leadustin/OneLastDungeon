using System.Collections.Generic;
using UnityEngine;

// WICHTIG: Kein MonoBehaviour! Das sind reine Daten.
[System.Serializable]
public class HeroRuntimeData
{
    // Identität
    public HeroClassTemplate heroTemplate;
    public string heroName;

    // Fortschritt
    public int currentLevel;
    public int currentXP;
    public int xpToNextLevel;

    // KÖRPER (Ausrüstung) - Key: Slot (z.B. Head), Value: Das Item
    public Dictionary<EquipmentSlot, GameItem> equipment = new Dictionary<EquipmentSlot, GameItem>();

    // GÜRTEL (Für den Kampf) - 2 Plätze für Tränke
    public GameItem[] beltSlots = new GameItem[2];

    // --- NEU: SKILL LISTE (Das Gedächtnis für den Kampf) ---
    public List<RuntimeSkill> activeSkills = new List<RuntimeSkill>();
    // -------------------------------------------------------

    // Konstruktor: Erstellt einen frischen Helden aus dem Template
    public HeroRuntimeData(HeroClassTemplate tmpl, int level)
    {
        heroTemplate = tmpl;
        heroName = tmpl.className;
        currentLevel = level;
        xpToNextLevel = CalculateNextLevelXP();

        // Hier könnten wir später Start-Skills laden:
        // if (tmpl.startingSkills != null) { ... }
    }

    // --- NEU: SKILL METHODEN ---

    // Einen neuen Skill lernen (z.B. durch Buch oder Level Up)
    public void LearnSkill(SkillTemplate newSkill)
    {
        if (newSkill == null) return;

        // Prüfen, ob wir ihn schon haben (keine Duplikate)
        foreach (var s in activeSkills)
        {
            if (s.template == newSkill) return;
        }

        // Als "RuntimeSkill" verpacken und hinzufügen
        activeSkills.Add(new RuntimeSkill(newSkill));
    }

    // Muss am Ende jeder Runde vom BattleManager aufgerufen werden
    public void TickAllCooldowns()
    {
        foreach (var skill in activeSkills)
        {
            skill.TickCooldown();
        }
    }
    // ---------------------------

    // --- AUSRÜSTUNG LOGIK ---

    public void EquipItem(GameItem newItem, EquipmentSlot slot)
    {
        // Sicherheitscheck
        EquipmentTemplate equipTmpl = newItem.template as EquipmentTemplate;
        if (equipTmpl == null || equipTmpl.slot != slot) return;

        // Wenn schon was da ist -> Zurück in den Rucksack
        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.AddItem(equipment[slot]);
        }

        // Neues Item anziehen
        equipment[slot] = newItem;
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            if (PlayerInventory.Instance != null)
                PlayerInventory.Instance.AddItem(equipment[slot]);

            equipment.Remove(slot);
        }
    }

    // --- STAT BERECHNUNG (Der wichtigste Teil) ---

    // Berechnet den TOTALEN Wert eines Stats (Basis + Level + Items)
    public float GetTotalStat(StatType stat)
    {
        float total = 0;

        // 1. Basis-Stats vom Template holen
        foreach (var startStat in heroTemplate.startingStats)
        {
            if (startStat.type == stat) total += startStat.value;
        }

        // 2. Level-Ups addieren
        if (currentLevel > 1 && heroTemplate.statsPerLevel != null)
        {
            foreach (var growthStat in heroTemplate.statsPerLevel)
            {
                if (growthStat.type == stat) total += growthStat.value * (currentLevel - 1);
            }
        }

        // 3. Ausrüstung addieren
        foreach (var item in equipment.Values)
        {
            if (item != null)
            {
                total += item.GetStat(stat);
            }
        }

        return total;
    }

    private int CalculateNextLevelXP()
    {
        return currentLevel * 1000; // Platzhalter-Formel
    }
}