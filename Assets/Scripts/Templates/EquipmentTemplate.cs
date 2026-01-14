using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct StatRange
{
    public StatType stat;
    public int min;
    public int max;
    [Range(0f, 1f)] public float chance; // 1.0 = Immer drauf, 0.1 = 10% Chance
}

[CreateAssetMenu(fileName = "New Equipment", menuName = "RPG/Inventory/Equipment")]
public class EquipmentTemplate : ItemTemplate
{
    [Header("Ausrüstung Slot")]
    public EquipmentSlot slot;

    [Header("Set Zugehörigkeit")]
    public ItemSetTemplate itemSet; // Kann leer sein

    [Header("Mögliche Werte (RNG)")]
    [Tooltip("Definiere hier, welche Stats möglich sind (z.B. Min 5 - Max 10 Stärke)")]
    public List<StatRange> possibleStats;

    [Header("Feste Boni")]
    [Tooltip("Ein Passiver Skill, der immer aktiv ist, wenn getragen")]
    public SkillTemplate grantedSkill;
}