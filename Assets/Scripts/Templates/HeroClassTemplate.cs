using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero Class", menuName = "RPG/Hero Class")]
public class HeroClassTemplate : ScriptableObject
{
    public string className;
    public Sprite classIcon;

    // --- NEU: Platz für die Animation ---
    [Header("Visuals")]
    [Tooltip("Markiere alle gesliceten Sprites im Projekt und ziehe sie hier rein.")]
    public Sprite[] deathAnimationFrames;

    [Header("Kampfwerte")]
    public AttackRange attackRange;
    public List<StatConfig> startingStats;
    public List<StatConfig> statsPerLevel;

    [Header("Spezialangriffe")]
    public List<SkillTemplate> skills;
}