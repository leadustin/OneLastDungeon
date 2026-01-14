using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum BattleState { SETUP, START, AUTO_BATTLE, TARGETING, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;
    public BattleState state;

    [Header("UI Referenzen")]
    public GameObject startBattleButton;
    public Transform skillButtonContainer;
    public GameObject skillButtonPrefab;

    [Header("Kampf Daten")]
    private BattleUnit playerUnit;
    private List<BattleUnit> enemyUnits;
    private RuntimeSkill selectedSkill;

    void Awake() { Instance = this; }

    public void StartBattle(BattleUnit player, List<BattleUnit> enemies)
    {
        playerUnit = player;
        enemyUnits = enemies;

        // Reset für neuen Kampf
        playerUnit.Initialize(true);
        foreach (var e in enemies) e.Initialize(false);

        if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.ToggleSlotVisuals(true);

        state = BattleState.SETUP;
        if (startBattleButton != null) startBattleButton.SetActive(true);

        SpawnSkillButtons();
    }

    public void OnStartBattleClicked()
    {
        if (state != BattleState.SETUP) return;
        if (startBattleButton != null) startBattleButton.SetActive(false);

        if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.ToggleSlotVisuals(false);

        StartCoroutine(BattleRoutine());
    }

    IEnumerator BattleRoutine()
    {
        state = BattleState.START;

        while (state != BattleState.WON && state != BattleState.LOST)
        {
            // --- 1. SPIELER RUNDE ---
            yield return StartCoroutine(PlayerTurn());
            if (CheckBattleOver()) break;

            // --- 2. GEGNER RUNDE ---
            yield return StartCoroutine(EnemyTurn());
            if (CheckBattleOver()) break;
        }

        EndBattle();
    }

    IEnumerator PlayerTurn()
    {
        bool playerStunned = playerUnit.ProcessTurnStart();
        playerUnit.ReduceCooldowns();
        RefreshSkillButtons();

        if (playerUnit.currentHP <= 0) { state = BattleState.LOST; yield break; }
        if (playerStunned)
        {
            Debug.Log("Spieler ist betäubt und setzt aus!");
            yield return new WaitForSeconds(1f);
            yield break;
        }

        state = BattleState.TARGETING;
        selectedSkill = null;

        while (state == BattleState.TARGETING)
        {
            yield return null;
        }
    }

    IEnumerator EnemyTurn()
    {
        foreach (var enemy in enemyUnits)
        {
            if (enemy.currentHP <= 0) continue;

            bool enemyStunned = enemy.ProcessTurnStart();
            enemy.ReduceCooldowns();

            if (enemy.currentHP <= 0) continue;
            if (enemyStunned)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            RuntimeSkill enemySkill = enemy.activeSkills.FirstOrDefault(s => s.IsReady());
            if (enemySkill == null && enemy.activeSkills.Count > 0) enemySkill = enemy.activeSkills[0];

            if (enemySkill != null)
            {
                yield return StartCoroutine(ExecuteSkill(enemy, playerUnit, enemySkill));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (playerUnit.currentHP <= 0) { state = BattleState.LOST; break; }
        }
    }

    public void OnSkillButtonClicked(RuntimeSkill skill)
    {
        if (state != BattleState.TARGETING) return;
        if (!skill.IsReady()) return;

        selectedSkill = skill;

        if (skill.template.targetMode == SkillTargetMode.AllEnemies)
        {
            StartCoroutine(PerformPlayerMove(null));
        }
        else if (skill.template.targetMode == SkillTargetMode.Self)
        {
            StartCoroutine(PerformPlayerMove(playerUnit));
        }
        else
        {
            var target = GetTargetEnemy();
            if (target != null) StartCoroutine(PerformPlayerMove(target));
        }
    }

    IEnumerator PerformPlayerMove(BattleUnit target)
    {
        state = BattleState.AUTO_BATTLE;
        yield return StartCoroutine(ExecuteSkill(playerUnit, target, selectedSkill));
        selectedSkill.OnUse();
    }

    // --- KERN-LOGIK: HYBRID SCHADEN ---
    IEnumerator ExecuteSkill(BattleUnit source, BattleUnit mainTarget, RuntimeSkill skill)
    {
        // 1. Visuelle Bewegung
        if (source.Visuals != null && skill.template.targetMode != SkillTargetMode.Self)
        {
            Vector3 targetPos = (mainTarget != null) ? mainTarget.transform.position : source.transform.position;
            yield return StartCoroutine(source.Visuals.MoveToTarget(targetPos));
        }

        yield return new WaitForSeconds(0.2f);

        // 2. Ziel-Erfassung
        List<BattleUnit> targets = new List<BattleUnit>();

        if (skill.template.targetMode == SkillTargetMode.AllEnemies)
        {
            targets.AddRange(enemyUnits.Where(e => e.currentHP > 0));
        }
        else if (skill.template.targetMode == SkillTargetMode.Self)
        {
            targets.Add(source);
        }
        else if (mainTarget != null)
        {
            targets.Add(mainTarget);
            if (skill.template.splashDamagePercent > 0)
            {
                var neighbors = GetNeighbors(mainTarget);
                targets.AddRange(neighbors);
            }
        }

        // 3. Schadensberechnung

        // A) Basis-Schaden (Skaliert mit PhysicalDamage * Skill Multiplier)
        // Wir nutzen PhysicalDamage als Basis-Attribut für Waffen/Angriffe
        float baseDmg = source.GetRawStat(StatType.PhysicalDamage) * skill.template.damageMultiplier;

        foreach (var target in targets)
        {
            // Sonderfall: Heilung (Wenn Skill Typ Holy ist & Ziel Self/Ally)
            if (skill.template.damageType == DamageType.Holy && skill.template.targetMode == SkillTargetMode.Self)
            {
                target.Heal(baseDmg);
                continue;
            }

            // B) Splash Berechnung
            float splashMult = 1f;
            if (target != mainTarget && skill.template.splashDamagePercent > 0)
            {
                splashMult = skill.template.splashDamagePercent;
            }

            // C) Crit Check (Gilt für alle Schadensarten dieses Angriffs)
            HitType hitType = HitType.Normal;
            float critRating = source.GetRawStat(StatType.CritChance);
            if (Random.value < CombatMath.CalculateChance(critRating, source.currentLevel))
            {
                hitType = HitType.Critical;
            }

            // --- HYBRID DAMAGE ANWENDUNG ---

            // 1. Haupt-Schaden anwenden (Basis * Splash)
            // Der Typ wird vom Skill bestimmt (z.B. Fireball -> Fire, Sword -> Physical)
            target.TakeDamage(baseDmg * splashMult, hitType, source.currentLevel, skill.template.damageType);

            // 2. Zusätzlichen Elementar-Schaden anwenden (Bonus-Stats)
            // Wenn der Angreifer z.B. "+10 Fire Damage" auf der Waffe hat
            foreach (DamageType dt in System.Enum.GetValues(typeof(DamageType)))
            {
                if (dt == DamageType.Physical) continue; // Physical ist schon im BaseDmg enthalten

                // Finde den passenden offensiven Stat (z.B. Fire -> FireDamage)
                StatType offensiveStat = GetOffensiveStatFor(dt);
                float elemDmg = source.GetRawStat(offensiveStat);

                if (elemDmg > 0)
                {
                    // Bonus-Schaden wird auch vom Skill-Multi beeinflusst (optional, aber fair)
                    float finalElem = elemDmg * skill.template.damageMultiplier * splashMult;

                    // Wendet den Schaden an (Erzeugt separate Schadenszahl)
                    target.TakeDamage(finalElem, hitType, source.currentLevel, dt);
                }
            }

            // D) Effekte & VFX
            if (skill.template.effects != null)
            {
                foreach (var effectConfig in skill.template.effects)
                {
                    if (Random.value <= effectConfig.chance)
                    {
                        BattleUnit effectTarget = effectConfig.applyToSelf ? source : target;
                        effectTarget.ApplyEffect(effectConfig);
                    }
                }
            }

            if (skill.template.vfxPrefab != null)
            {
                Instantiate(skill.template.vfxPrefab, target.transform.position, Quaternion.identity);
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (source.Visuals != null)
        {
            yield return StartCoroutine(source.Visuals.ReturnToStart());
        }
    }

    void SpawnSkillButtons()
    {
        foreach (Transform child in skillButtonContainer) Destroy(child.gameObject);

        foreach (var skill in playerUnit.activeSkills)
        {
            GameObject go = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton btn = go.GetComponent<SkillButton>();
            btn.Setup(skill, OnSkillButtonClicked);
        }
    }

    void RefreshSkillButtons()
    {
        foreach (Transform child in skillButtonContainer)
        {
            SkillButton btn = child.GetComponent<SkillButton>();
            if (btn != null) btn.UpdateState();
        }
    }

    BattleUnit GetTargetEnemy() => enemyUnits.FirstOrDefault(e => e.currentHP > 0);

    List<BattleUnit> GetNeighbors(BattleUnit center)
    {
        List<BattleUnit> result = new List<BattleUnit>();
        int index = enemyUnits.IndexOf(center);
        if (index == -1) return result;
        if (index > 0 && enemyUnits[index - 1].currentHP > 0) result.Add(enemyUnits[index - 1]);
        if (index < enemyUnits.Count - 1 && enemyUnits[index + 1].currentHP > 0) result.Add(enemyUnits[index + 1]);
        return result;
    }

    bool CheckBattleOver()
    {
        if (!enemyUnits.Any(e => e.currentHP > 0))
        {
            state = BattleState.WON;
            if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.OnBattleWon();
            return true;
        }
        if (playerUnit.currentHP <= 0)
        {
            state = BattleState.LOST;
            if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.OnBattleLost();
            return true;
        }
        return false;
    }

    void EndBattle()
    {
        if (startBattleButton != null) startBattleButton.SetActive(false);
    }

    // --- HELPER FÜR HYBRID MAPPING ---
    StatType GetOffensiveStatFor(DamageType dt)
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
}