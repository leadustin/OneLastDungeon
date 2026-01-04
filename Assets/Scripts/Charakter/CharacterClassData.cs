using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Class", menuName = "Game Data/Character Class")]
public class CharacterClassData : ScriptableObject
{
    [Header("Allgemeine Infos")]
    public string className;        // z.B. "Krieger"
    [TextArea] public string description; // z.B. "Ein starker Nahkämpfer..."
    public Sprite classIcon;        // Für den Auswahl-Screen später

    [Header("Start-Attribute")]
    public float maxHealth = 100;
    public float maxMana = 50;
    public int baseDefense = 0;

    [Header("Start-Schaden (Ohne Waffe)")]
    public int baseMinDamage = 1;
    public int baseMaxDamage = 2;

    [Header("Basis Chancen (%)")]
    [Range(0, 100)] public float baseCritChance = 5.0f;
    [Range(0, 100)] public float baseDodgeChance = 5.0f;
    [Range(0, 100)] public float baseBlockChance = 5.0f;

    [Header("Start-Ausrüstung")]
    // Hier ziehen wir die Items rein, die der Held beim Start tragen soll
    public List<CardData> startingEquipment;
}