using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SkillEffectConfig
{
    [Header("Effekt Konfiguration")]
    [Tooltip("Welcher Effekt soll ausgelöst werden?")]
    public StatusEffectType type;

    [Tooltip("Wie viele Runden hält der Effekt an?")]
    public int duration = 2;

    [Tooltip("Stärke pro Runde (Schaden bei Gift/Feuer, Heilung bei Regen).")]
    public float amount = 10f;

    [Tooltip("Wahrscheinlichkeit (0.0 bis 1.0). 1 = 100%")]
    [Range(0f, 1f)]
    public float chance = 1.0f;

    [Tooltip("Wende ich das auf mich selbst an? (z.B. für Heilung)")]
    public bool applyToSelf = false;
}

[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skill Template")]
public class SkillTemplate : ScriptableObject
{
    [Header("Basis Info")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Runden-Management")]
    [Tooltip("Verzögerung am Start des Kampfes")]
    public int initialTurnDelay = 0;
    [Tooltip("Cooldown nach Benutzung")]
    public int rechargeTurns = 3;

    [Header("Schaden & Art")]
    public DamageType damageType = DamageType.Physical;
    public float damageMultiplier = 1.5f;
    public bool isHeal = false;

    [Header("Flächenschaden (AoE)")]
    [Range(0f, 1f)]
    public float splashDamagePercent = 0.0f;

    // HIER ist die neue Liste
    [Header("Zusatz-Effekte")]
    [Tooltip("Liste aller Statuseffekte, die dieser Skill auslösen kann")]
    public List<SkillEffectConfig> effects;

    [Header("Optik")]
    public GameObject vfxPrefab;
}