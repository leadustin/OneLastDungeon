using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance;

    [Header("Start Party (Nur für Test/Neues Spiel)")]
    public List<HeroClassTemplate> startingHeroes;

    // DAS HIER ist deine echte Party zur Laufzeit
    public List<HeroRuntimeData> activeParty = new List<HeroRuntimeData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeParty();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeParty()
    {
        // Erstellt aus den Templates echte Daten-Objekte
        foreach (var tmpl in startingHeroes)
        {
            HeroRuntimeData newHero = new HeroRuntimeData(tmpl, 1);
            activeParty.Add(newHero);
            Debug.Log($"Held erstellt: {newHero.heroName}");
        }
    }

    // Helfer: Gib mir den Held an Index 0 (Leader)
    public HeroRuntimeData GetLeader()
    {
        if (activeParty.Count > 0) return activeParty[0];
        return null;
    }
}