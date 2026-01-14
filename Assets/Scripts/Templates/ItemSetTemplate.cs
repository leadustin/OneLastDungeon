using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct SetBonus
{
    public int piecesRequired; // Wie viele Teile braucht man? (z.B. 2)
    public StatType statBonus; // Welcher Stat?
    public float value;        // Wie viel davon?

    [Tooltip("Optional: Ein passiver Skill (z.B. Aura ab 4 Teilen)")]
    public SkillTemplate passiveSkillEffect;
}

[CreateAssetMenu(fileName = "New Item Set", menuName = "RPG/Inventory/Item Set")]
public class ItemSetTemplate : ScriptableObject
{
    public string setName;
    public List<SetBonus> bonuses;
}