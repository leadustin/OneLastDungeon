using UnityEngine;

public enum ConsumableEffect { HealHP, HealMana, BuffStat, GrantXP }

[CreateAssetMenu(fileName = "New Consumable", menuName = "RPG/Inventory/Consumable")]
public class ConsumableTemplate : ItemTemplate
{
    [Header("Verbrauchs-Effekt")]
    public ConsumableEffect effect;
    public float amount; // z.B. 100 HP Heilung
    public float duration; // 0 = Sofort, >0 = Zeit-Buff

    [Tooltip("Skill, der ausgelöst wird (optional)")]
    public SkillTemplate triggerSkill;
}