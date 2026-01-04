using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skill Effects/Generic Heal Logic")]
public class HealEffect : SkillEffect
{
    // KEINE festen Variablen mehr hier (wie healAmount), 
    // da diese nun im Skill-Container (SkillData) pro Schritt definiert werden.

    public override void Activate(GameObject caster, SkillStep stepData)
    {
        // 1. Level des Spielers für die Skalierung holen
        int playerLevel = 1;
        if (PlayerManager.Instance != null) playerLevel = PlayerManager.Instance.level;

        // 2. Den skalierten Heilwert aus den Container-Daten berechnen
        int finalHeal = stepData.GetScaledValue(playerLevel);

        // 3. Heilung beim Spieler durchführen
        PlayerManager.Instance.Heal(finalHeal);

        // 4. Position für den Text finden (über der Spielerkarte)
        Vector3 spawnPos = caster.transform.position;
        if (caster.GetComponent<PlayerManager>() != null && GridManager.Instance != null)
        {
            spawnPos = GridManager.Instance.GetPlayerCardPosition();
        }

        // 5. Text anzeigen (z.B. "Heilung 30")
        if (FloatingTextManager.Instance != null)
        {
            // Wir nutzen den StatusName aus dem Container, falls vorhanden, sonst nur die Zahl
            string message = finalHeal.ToString();
            if (!string.IsNullOrEmpty(stepData.statusName))
            {
                message = $"{stepData.statusName} {finalHeal}";
            }

            // Nutzt den vordefinierten Heal-Style (Grün) im Manager
            FloatingTextManager.Instance.Show(CombatTextType.Heal, message, spawnPos + Vector3.up);
        }

        Debug.Log($"Heilung aktiviert: {finalHeal} HP regeneriert.");
    }
}