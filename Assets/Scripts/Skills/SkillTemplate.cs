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

    // --- NEU: ZIELWAHL ---
    [Header("Ziel-Logik")]
    [Tooltip("Wen muss ich anklicken, um diesen Skill zu nutzen?")]
    public SkillTargetMode targetMode = SkillTargetMode.SingleEnemy;
    // ---------------------

    [Header("Runden-Management")]
    [Tooltip("Verzögerung am Start des Kampfes (Initial Cooldown)")]
    public int initialTurnDelay = 0;
    [Tooltip("Cooldown nach Benutzung (in Runden)")]
    public int rechargeTurns = 3;

    [Header("Schaden & Art")]
    public DamageType damageType = DamageType.Physical;
    public float damageMultiplier = 1.5f; // 1.5 = 150% Waffenschaden
    [Range(0f, 1f)] public float splashDamagePercent = 0.5f;
    public bool isHeal = false;

    [Header("Visuals")]
    public GameObject vfxPrefab;
    public string animationName = "Attack"; // Standard Animation Trigger

    [Header("Zusatz-Effekte")]
    public List<SkillEffectConfig> effects;
}