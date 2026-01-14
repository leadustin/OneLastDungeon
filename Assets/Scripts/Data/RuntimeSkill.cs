using UnityEngine;

[System.Serializable]
public class RuntimeSkill
{
    public SkillTemplate template;
    public int currentCooldown; // 0 = Bereit, >0 = Warten

    // Konstruktor: Erstellt den Skill aus der Vorlage
    public RuntimeSkill(SkillTemplate tmpl)
    {
        template = tmpl;
        // Zu Beginn des Kampfes evtl. Start-Verzögerung setzen
        currentCooldown = tmpl.initialTurnDelay;
    }

    // Abfrage: Darf ich klicken?
    public bool IsReady()
    {
        return currentCooldown <= 0;
    }

    // Muss am Ende jeder Runde aufgerufen werden
    public void TickCooldown()
    {
        if (currentCooldown > 0)
            currentCooldown--;
    }

    // Wenn der Skill benutzt wurde -> Reset
    public void OnUse()
    {
        currentCooldown = template.rechargeTurns;
    }
}