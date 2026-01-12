using UnityEngine;

// Das ist die Basisklasse für alle Effekte.
public abstract class SkillEffect : ScriptableObject
{
    // Wir übergeben jetzt 'stepData', damit der Effekt weiß, wie stark er sein soll.
    public abstract void Activate(GameObject caster, SkillStep stepData);
}