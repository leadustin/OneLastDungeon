using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Skill Effects/Generic Status Logic")]
public class StatusEffect : SkillEffect
{
    [Header("Verhalten")]
    public StatusType fixedType = StatusType.DamageOverTime;
    public bool applyToSelf = false; // True = Buff Spieler, False = Debuff Gegner

    public override void Activate(GameObject caster, SkillStep stepData)
    {
        int playerLevel = 1;
        if (PlayerManager.Instance != null) playerLevel = PlayerManager.Instance.level;

        // Wert berechnen
        int amount = stepData.GetScaledValue(playerLevel);

        // Status-Paket schnüren
        ActiveStatusEffect newStatus = new ActiveStatusEffect(
            stepData.statusName,    // Name aus Container ("Brennen")
            fixedType,
            amount,                 // Skalierter Wert
            stepData.durationRounds // Dauer aus Container
        );

        if (applyToSelf)
        {
            // Buff auf Spieler
            PlayerManager.Instance.AddStatusEffect(newStatus);
        }
        else
        {
            // Debuff auf Gegner
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.AddEnemyStatusEffect(newStatus);
            }
        }
    }
}