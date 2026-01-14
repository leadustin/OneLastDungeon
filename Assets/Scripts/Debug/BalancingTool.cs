using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;

public enum DataSource { ManualInput, UseTemplates }

// --- SETUP STRUKTUREN ---
[System.Serializable]
public struct HeroSetup
{
    public string name;
    public HeroClassTemplate template;
    public SkillTemplate skill;
    public List<EquipmentTemplate> equipment;
}

public class BalancingTool : MonoBehaviour
{
    [Header("--- BALANCING SETTINGS (Global) ---")]
    public float globalResistScaling = 10f;
    public float globalChanceScaling = 20f;

    [Header("--- MODUS ---")]
    public DataSource source = DataSource.ManualInput;
    public bool useAutoScaling = true;

    [Header("--- MANUAL CONFIG (Hybrid Damage) ---")]
    [Tooltip("Der physische Basis-Schaden")]
    public float manualBasePhysDmg = 15f;

    [Header("Addional Element Dmg")]
    public DamageType manualElemType = DamageType.Fire;
    [Range(0f, 2f)] public float manualElemRatio = 0.3f;

    [Space]
    [Range(1, 6)] public int manualHeroCount = 1;
    [Range(1, 9)] public int manualEnemyCount = 1;

    [Header("--- BASE STATS (Lvl 1) ---")]
    public float baseHP = 100;
    public float baseArmor = 10;
    public float baseResist = 5;
    public float baseCrit = 5;
    public float baseDodge = 5;

    [Header("--- GROWTH (Per Level) ---")]
    // HIER LAG DER FEHLER: Wir definieren sie jetzt wieder explizit getrennt!
    public float heroHpGrowth = 12f;
    public float heroDmgGrowth = 3f;
    public float heroArmorGrowth = 2f;
    public float heroResistGrowth = 0.5f;

    public float enemyHpGrowth = 10f;
    public float enemyDmgGrowth = 2f;
    public float enemyArmorGrowth = 1f;
    // Enemy Resist Growth ist oft 0 oder manuell, wir nutzen hier testweise heroResistGrowth oder 0

    [Header("--- PARTY CONFIG (Template Mode) ---")]
    public List<HeroSetup> heroParty = new List<HeroSetup>();
    public List<EnemyTemplate> enemyWave = new List<EnemyTemplate>();

    [Header("--- SIMULATION ---")]
    public int heroLevel = 1;
    public int enemyLevel = 1;
    public int simulationRuns = 500;

    [Header("--- REPORT ---")]
    [TextArea(20, 50)] public string report;

    // INTERNE SIM UNIT
    class SimUnit
    {
        public string name;
        public float hp, maxHp;
        public Dictionary<StatType, float> stats = new Dictionary<StatType, float>();

        public float GetStat(StatType t) => stats.ContainsKey(t) ? stats[t] : 0f;
        public void SetStat(StatType t, float v) { if (stats.ContainsKey(t)) stats[t] = v; else stats.Add(t, v); }
        public void AddStat(StatType t, float v) { if (stats.ContainsKey(t)) stats[t] += v; else stats.Add(t, v); }

        public float skillMult = 1f;
        public int skillMaxCD, currentCD;
        public bool IsAlive => hp > 0;
    }

    private void OnValidate()
    {
        CombatMath.RESIST_SCALING = globalResistScaling;
        CombatMath.CHANCE_SCALING = globalChanceScaling;
    }

    // =================================================================================
    // BUTTON: FULL SWEEP
    // =================================================================================
    [ContextMenu("Run FULL Hybrid Sweep")]
    public void RunFullSweep()
    {
        OnValidate();
        StringBuilder table = new StringBuilder();
        StringBuilder details = new StringBuilder();

        table.AppendLine($"=== HYBRID DAMAGE SWEEP (AMPEL SYSTEM) ===");
        table.AppendLine("Lvl | Rate | Survivors | Rnds | RATING");
        table.AppendLine("-----------------------------------------------------");

        int[] levels = { 1, 5, 10, 25, 50, 75, 100 };

        foreach (int lvl in levels)
        {
            TeamSimResult r = RunTeamSimulation(lvl, lvl);

            // Ampel Bewertung holen
            string rating = GetSimRating(r);

            string survivors = r.winRate > 0.01f ? $"{r.avgSurvivors:F1} Heroes" : "WIPE";
            table.AppendLine($"{lvl,-3} | {r.winRate * 100,-3:F0}% | {survivors,-11} | {r.avgRounds,-4:F1} | {rating}");

            if (GetPartySize() > 0 && GetWaveSize() > 0)
                details.AppendLine(GetDetailedLevelReport(lvl, r, rating));
        }
        report = table.ToString() + "\n" + details.ToString();
    }

    // =================================================================================
    // AMPEL LOGIK
    // =================================================================================
    string GetSimRating(TeamSimResult r)
    {
        int partySize = GetPartySize();

        // 1. ROT: TOTALER FEHSCHLAG (Zu schwer)
        if (r.winRate < 0.2f) return "🔴 IMPOSSIBLE";
        if (r.winRate < 0.6f) return "🔴 HARD / UNFAIR";

        // 2. ROT: ZU EINFACH (Zu langweilig)
        if (r.winRate > 0.99f && r.avgRounds < 1.5f) return "🔴 TRIVIAL (1-Shot)";

        // 3. GELB: OPTIMIERUNG NÖTIG
        if (r.avgSurvivors < partySize * 0.5f) return "🟡 PYRRHIC (High Casualties)";
        if (r.avgRounds > 15f) return "🟡 SLOW (Bullet Sponge)";
        if (r.avgRounds < 2f) return "🟡 TOO FAST";

        // 4. GRÜN: ALLES GUT
        return "🟢 BALANCED";
    }

    // --- SIMULATION LOGIC ---

    struct TeamSimResult { public float winRate, avgRounds, avgSurvivors; }

    TeamSimResult RunTeamSimulation(int hLvl, int eLvl)
    {
        if (GetPartySize() == 0 || GetWaveSize() == 0) return new TeamSimResult();
        int wins = 0; float totalRounds = 0; float totalSurvivors = 0;

        for (int i = 0; i < simulationRuns; i++)
        {
            List<SimUnit> heroes = CreateHeroParty(hLvl);
            List<SimUnit> enemies = CreateEnemyWave(eLvl);
            int rounds = SimulateFight(heroes, enemies, hLvl, eLvl);

            int hCount = heroes.Count(h => h.IsAlive);
            if (hCount > 0) { wins++; totalSurvivors += hCount; }
            totalRounds += rounds;
        }
        return new TeamSimResult { winRate = (float)wins / simulationRuns, avgRounds = totalRounds / simulationRuns, avgSurvivors = wins > 0 ? totalSurvivors / wins : 0 };
    }

    int SimulateFight(List<SimUnit> heroes, List<SimUnit> enemies, int hLvl, int eLvl)
    {
        int rounds = 0;
        while (rounds < 100)
        {
            if (!heroes.Any(x => x.IsAlive) || !enemies.Any(x => x.IsAlive)) break;
            rounds++;

            // HEROES TURN
            foreach (var hero in heroes)
            {
                if (!hero.IsAlive) continue;
                var target = enemies.FirstOrDefault(e => e.IsAlive);
                if (target == null) break;
                PerformHybridAttack(hero, target, hLvl);
            }

            // ENEMIES TURN
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;
                var target = heroes.FirstOrDefault(h => h.IsAlive);
                if (target == null) break;
                PerformHybridAttack(enemy, target, eLvl);
            }
        }
        return rounds;
    }

    void PerformHybridAttack(SimUnit attacker, SimUnit defender, int attLvl)
    {
        // 1. Dodge
        float dodgeChance = CombatMath.CalculateChance(defender.GetStat(StatType.DodgeChance), attLvl);
        if (UnityEngine.Random.value < dodgeChance) return;

        // 2. Skill Mult
        float mult = 1f;
        if (attacker.currentCD <= 0 && attacker.skillMult > 1f)
        {
            mult = attacker.skillMult; attacker.currentCD = attacker.skillMaxCD;
        }
        else { attacker.currentCD--; }

        float totalDamageDealt = 0;

        // 3. Physical
        float physRaw = attacker.GetStat(StatType.PhysicalDamage) * mult;
        if (physRaw > 0)
        {
            float armor = defender.GetStat(StatType.Armor);
            totalDamageDealt += CombatMath.CalculateFinalDamage(physRaw, DamageType.Physical, armor, attLvl);
        }

        // 4. Elemental
        foreach (DamageType dt in Enum.GetValues(typeof(DamageType)))
        {
            if (dt == DamageType.Physical) continue;
            StatType dmgStat = GetDamageStatFromType(dt);
            float elemRaw = attacker.GetStat(dmgStat) * mult;

            if (elemRaw > 0)
            {
                StatType resistStat = CombatMath.GetResistanceStat(dt);
                float resistVal = defender.GetStat(resistStat);
                totalDamageDealt += CombatMath.CalculateFinalDamage(elemRaw, dt, resistVal, attLvl);
            }
        }

        // 5. Crit
        float critChance = CombatMath.CalculateChance(attacker.GetStat(StatType.CritChance), attLvl);
        if (UnityEngine.Random.value < critChance) totalDamageDealt *= 1.5f;

        defender.hp -= totalDamageDealt;
    }

    StatType GetDamageStatFromType(DamageType dt)
    {
        switch (dt)
        {
            case DamageType.Fire: return StatType.FireDamage;
            case DamageType.Ice: return StatType.IceDamage;
            case DamageType.Lightning: return StatType.LightningDamage;
            case DamageType.Poison: return StatType.PoisonDamage;
            case DamageType.Arcane: return StatType.ArcaneDamage;
            case DamageType.Holy: return StatType.HolyDamage;
            case DamageType.Shadow: return StatType.ShadowDamage;
            default: return StatType.PhysicalDamage;
        }
    }

    // --- UNIT CREATION ---

    List<SimUnit> CreateHeroParty(int lvl)
    {
        var list = new List<SimUnit>();
        if (source == DataSource.UseTemplates) { foreach (var s in heroParty) list.Add(CreateUnitFromTemplate(s, lvl)); }
        else { for (int i = 0; i < manualHeroCount; i++) list.Add(CreateManualUnit("Hero", true, lvl)); }
        return list;
    }

    List<SimUnit> CreateEnemyWave(int lvl)
    {
        var list = new List<SimUnit>();
        if (source == DataSource.UseTemplates) { foreach (var t in enemyWave) list.Add(CreateUnitFromEnemyTemplate(t, lvl)); }
        else { for (int i = 0; i < manualEnemyCount; i++) list.Add(CreateManualUnit("Enemy", false, lvl)); }
        return list;
    }

    SimUnit CreateManualUnit(string name, bool isHero, int lvl)
    {
        SimUnit u = new SimUnit(); u.name = name;

        // FIX: Nutzt jetzt die korrekten Variablen
        float gHP = isHero ? heroHpGrowth : enemyHpGrowth;
        float gDmg = isHero ? heroDmgGrowth : enemyDmgGrowth;
        float gArmor = isHero ? heroArmorGrowth : enemyArmorGrowth;

        float hp = baseHP + (gHP * (lvl - 1));
        float physDmg = manualBasePhysDmg + (gDmg * (lvl - 1));
        float armor = baseArmor + (gArmor * (lvl - 1));
        float resist = baseResist + (heroResistGrowth * (lvl - 1)); // Vereinfacht

        u.maxHp = hp; u.hp = hp;
        u.SetStat(StatType.PhysicalDamage, physDmg);
        u.SetStat(StatType.Armor, armor);
        u.SetStat(StatType.CritChance, baseCrit);
        u.SetStat(StatType.DodgeChance, baseDodge);

        if (manualElemRatio > 0)
        {
            float elemDmg = physDmg * manualElemRatio;
            u.SetStat(GetDamageStatFromType(manualElemType), elemDmg);
        }

        // Test Resistenzen
        u.SetStat(StatType.FireResist, resist);
        return u;
    }

    SimUnit CreateUnitFromTemplate(HeroSetup setup, int lvl)
    {
        SimUnit u = new SimUnit();
        u.name = setup.template != null ? setup.template.className : "Hero";
        if (setup.template != null)
        {
            u.maxHp = GetStat(setup.template.startingStats, StatType.MaxHealth);
            u.hp = u.maxHp;
            LoadStatsIntoDict(u, setup.template.startingStats);

            // FIX: Nutzt jetzt heroHpGrowth etc. als Fallback
            if (lvl > 1)
            {
                float hpG = GetStat(setup.template.statsPerLevel, StatType.MaxHealth); if (hpG == 0) hpG = heroHpGrowth;
                float dmgG = GetStat(setup.template.statsPerLevel, StatType.PhysicalDamage); if (dmgG == 0) dmgG = heroDmgGrowth;
                float armG = GetStat(setup.template.statsPerLevel, StatType.Armor); if (armG == 0) armG = heroArmorGrowth;

                u.maxHp += hpG * (lvl - 1); u.hp = u.maxHp;
                u.AddStat(StatType.PhysicalDamage, dmgG * (lvl - 1));
                u.AddStat(StatType.Armor, armG * (lvl - 1));
            }
        }
        else return CreateManualUnit("Hero", true, lvl);

        if (setup.equipment != null)
        {
            foreach (var item in setup.equipment)
            {
                if (item == null || item.possibleStats == null) continue;
                foreach (var s in item.possibleStats)
                {
                    float val = ((s.min + s.max) / 2f) * s.chance;
                    if (s.stat == StatType.MaxHealth) { u.maxHp += val; u.hp += val; }
                    else { u.AddStat(s.stat, val); }
                }
            }
        }
        return u;
    }

    SimUnit CreateUnitFromEnemyTemplate(EnemyTemplate tmpl, int lvl)
    {
        SimUnit u = new SimUnit();
        u.name = tmpl != null ? tmpl.unitName : "Enemy";
        if (tmpl != null)
        {
            u.maxHp = GetStat(tmpl.stats, StatType.MaxHealth); u.hp = u.maxHp;
            LoadStatsIntoDict(u, tmpl.stats);

            // FIX: Nutzt enemyHpGrowth etc.
            if (useAutoScaling && lvl > 1)
            {
                u.maxHp += enemyHpGrowth * (lvl - 1); u.hp = u.maxHp;
                u.AddStat(StatType.PhysicalDamage, enemyDmgGrowth * (lvl - 1));
                u.AddStat(StatType.Armor, enemyArmorGrowth * (lvl - 1));
            }
        }
        else return CreateManualUnit("Enemy", false, lvl);
        return u;
    }

    // --- REPORT GENERATOR ---
    string GetDetailedLevelReport(int lvl, TeamSimResult r, string rating)
    {
        List<SimUnit> heroes = CreateHeroParty(lvl);
        List<SimUnit> enemies = CreateEnemyWave(lvl);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== LEVEL {lvl} ANALYSIS: {rating} ===");

        SimUnit targetDummyEnemy = (enemies.Count > 0) ? enemies[0] : null;
        SimUnit targetDummyHero = (heroes.Count > 0) ? heroes[0] : null;

        // HEROES
        sb.AppendLine("\n>>> HERO SQUAD <<<");
        foreach (var h in heroes)
        {
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"[{h.name}]  HP: {h.maxHp:F0} | Armor: {h.GetStat(StatType.Armor):F0}");
            if (targetDummyEnemy != null)
            {
                sb.Append("   > DMG VS ENEMY: ");
                AppendAttackAnalysis(sb, h, targetDummyEnemy, lvl);
            }
        }

        // ENEMIES
        sb.AppendLine("\n>>> ENEMY WAVE <<<");
        foreach (var e in enemies)
        {
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"[{e.name}]  HP: {e.maxHp:F0} | Armor: {e.GetStat(StatType.Armor):F0}");
            if (targetDummyHero != null)
            {
                sb.Append("   > DMG VS HERO: ");
                AppendAttackAnalysis(sb, e, targetDummyHero, lvl);
            }
        }
        return sb.ToString();
    }

    void AppendAttackAnalysis(StringBuilder sb, SimUnit att, SimUnit def, int lvl)
    {
        float totalCalc = 0;
        List<string> hits = new List<string>();

        // Phys
        float phys = att.GetStat(StatType.PhysicalDamage);
        if (phys > 0)
        {
            float armor = def.GetStat(StatType.Armor);
            float reduction = CombatMath.GetReductionPercent(armor, lvl);
            float final = phys * (1 - reduction);
            hits.Add($"{final:F0} Phys");
            totalCalc += final;
        }

        // Elem
        foreach (DamageType dt in Enum.GetValues(typeof(DamageType)))
        {
            if (dt == DamageType.Physical) continue;
            StatType dmgStat = GetDamageStatFromType(dt);
            float elem = att.GetStat(dmgStat);
            if (elem > 0)
            {
                StatType resStat = CombatMath.GetResistanceStat(dt);
                float resVal = def.GetStat(resStat);
                float reduction = CombatMath.GetReductionPercent(resVal, lvl);
                float final = elem * (1 - reduction);
                hits.Add($"{final:F0} {dt}");
                totalCalc += final;
            }
        }
        sb.AppendLine($"{totalCalc:F0} ({string.Join("+", hits)})");
    }

    void LoadStatsIntoDict(SimUnit u, List<StatConfig> list) { if (list == null) return; foreach (var s in list) u.SetStat(s.type, s.value); }
    float GetStat(List<StatConfig> l, StatType t) { if (l == null) return 0; var f = l.Find(x => x.type == t); return f != null ? f.value : 0; }
    int GetPartySize() => (source == DataSource.UseTemplates) ? heroParty.Count : manualHeroCount;
    int GetWaveSize() => (source == DataSource.UseTemplates) ? enemyWave.Count : manualEnemyCount;
}