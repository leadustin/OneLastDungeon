using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skill Effects/Generic Damage Logic")]
public class DamageEffect : SkillEffect
{
    // HIER KEINE VARIABLEN MEHR (Alles kommt vom Container)

    public override void Activate(GameObject caster, SkillStep stepData)
    {
        // 1. Level für Skalierung holen
        int playerLevel = 1;
        if (PlayerManager.Instance != null) playerLevel = PlayerManager.Instance.level;

        // 2. Schaden berechnen
        int finalDamage = stepData.GetScaledValue(playerLevel);

        Debug.Log($"Logik: Verursache {finalDamage} {stepData.damageType}-Schaden.");

        // 3. Text anzeigen (z.B. "50 (Fire)")
        if (FloatingTextManager.Instance != null)
        {
            Vector3 pos = Vector3.zero;
            // Versuch die Position der Karte zu finden
            if (GridManager.Instance != null) pos = GridManager.Instance.GetPlayerCardPosition();
            else pos = caster.transform.position;

            // Optional: Farbe je nach DamageType wählen (hier vereinfacht)
            string msg = $"{finalDamage} <size=70%>{stepData.damageType}</size>";
            FloatingTextManager.Instance.Show(CombatTextType.SkillName, msg, pos + Vector3.up);
        }

        // 4. Schaden an den aktuellen Gegner senden
        if (CombatManager.Instance != null && CombatManager.Instance.isFighting)
        {
            CombatManager.Instance.ApplySkillDamage(finalDamage);
        }
    }
}