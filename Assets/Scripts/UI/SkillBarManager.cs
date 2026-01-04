using UnityEngine;
using System.Collections.Generic;

public class SkillBarManager : MonoBehaviour
{
    // Hier ziehen wir später deine 5 Objekte (SkillFrameBg1-5) rein
    public SkillSlotUI[] slots;

    void Start()
    {
        // Wir warten kurz, bis der PlayerManager startklar ist
        Invoke("InitializeSkillBar", 0.2f);
    }

    public void InitializeSkillBar()
    {
        // Hole Skills vom Spieler
        List<SkillData> skills = PlayerManager.Instance.learnedSkills;

        // Gehe alle 5 Slots durch
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < skills.Count)
            {
                // Slot bekommt Skill
                slots[i].Setup(skills[i]);
            }
            else
            {
                // Slot wird leer gemacht
                slots[i].Setup(null);
            }
        }
    }
}