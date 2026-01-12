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

    private bool isPaused = false;
    private Coroutine battleCoroutine;

    void Awake() { Instance = this; }

    public void StartBattle(BattleUnit player, List<BattleUnit> enemies)
    {
        playerUnit = player;
        enemyUnits = enemies;

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

        StartCoroutine(StartAutoBattle());
    }

    IEnumerator StartAutoBattle()
    {
        state = BattleState.START;
        yield return new WaitForSeconds(0.5f);
        state = BattleState.AUTO_BATTLE;
        battleCoroutine = StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        while (state == BattleState.AUTO_BATTLE || state == BattleState.TARGETING)
        {
            // --- 1. SPIELER ZUG ---
            playerUnit.ReduceCooldowns();
            UpdateSkillButtons();

            bool playerIsStunned = playerUnit.ProcessTurnStart();
            if (playerUnit.currentHP <= 0) { CheckBattleOver(); yield break; } // DoT Tod Check

            if (playerIsStunned)
            {
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                float waitTimer = 0f;
                while (waitTimer < 1.0f)
                {
                    if (isPaused) yield return null;
                    else { waitTimer += Time.deltaTime; yield return null; }
                }

                if (!isPaused) yield return StartCoroutine(PerformAutoAttack());
                else while (isPaused) yield return null;
            }

            // --- 2. GEGNER ZUG ---
            yield return StartCoroutine(EnemyTurn());

            if (CheckBattleOver()) break;
        }
    }

    IEnumerator PerformAutoAttack()
    {
        BattleUnit target = GetTargetEnemy();
        if (target != null)
        {
            if (playerUnit.Visuals != null) yield return StartCoroutine(playerUnit.Visuals.FocusAttacker(true));

            if (playerUnit.attackRange == AttackRange.Melee && playerUnit.Visuals != null)
                yield return StartCoroutine(playerUnit.Visuals.MoveToTarget(target.transform.position));
            else
                yield return new WaitForSeconds(0.2f);

            float dmg = playerUnit.GetDamageValue();
            DealDamageTo(target, dmg, null); // Auto Attack = Kein Skill

            yield return new WaitForSeconds(0.3f);

            if (playerUnit.attackRange == AttackRange.Melee && playerUnit.Visuals != null)
                yield return StartCoroutine(playerUnit.Visuals.ReturnToStart());

            if (playerUnit.Visuals != null) yield return StartCoroutine(playerUnit.Visuals.FocusAttacker(false));
        }
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var enemy in enemyUnits)
        {
            if (enemy.currentHP <= 0) continue;

            enemy.ReduceCooldowns();
            bool enemyIsStunned = enemy.ProcessTurnStart();
            if (enemyIsStunned) { yield return new WaitForSeconds(0.5f); continue; }

            RuntimeSkill chosenSkill = null;
            var readySkills = enemy.activeSkills.Where(s => s.currentCooldown <= 0).ToList();
            if (readySkills.Count > 0 && Random.value < 0.5f) chosenSkill = readySkills[Random.Range(0, readySkills.Count)];

            if (enemy.Visuals != null) yield return StartCoroutine(enemy.Visuals.FocusAttacker(true));

            if (chosenSkill != null) // SKILL
            {
                if (enemy.attackRange == AttackRange.Melee && enemy.Visuals != null)
                    yield return StartCoroutine(enemy.Visuals.MoveToTarget(playerUnit.transform.position));
                else
                    yield return new WaitForSeconds(0.5f);

                float baseDmg = enemy.GetDamageValue();
                float skillDmg = baseDmg * chosenSkill.template.damageMultiplier;

                DealDamageTo(playerUnit, skillDmg, chosenSkill); // Skill übergeben!
                chosenSkill.currentCooldown = chosenSkill.template.rechargeTurns;

                yield return new WaitForSeconds(0.5f);
                if (enemy.attackRange == AttackRange.Melee && enemy.Visuals != null)
                    yield return StartCoroutine(enemy.Visuals.ReturnToStart());
            }
            else // NORMALER ANGRIFF
            {
                if (enemy.attackRange == AttackRange.Melee && enemy.Visuals != null)
                    yield return StartCoroutine(enemy.Visuals.MoveToTarget(playerUnit.transform.position));
                else
                    yield return new WaitForSeconds(0.3f);

                DealDamageTo(playerUnit, enemy.GetDamageValue(), null);

                yield return new WaitForSeconds(0.3f);
                if (enemy.attackRange == AttackRange.Melee && enemy.Visuals != null)
                    yield return StartCoroutine(enemy.Visuals.ReturnToStart());
            }

            if (enemy.Visuals != null) yield return StartCoroutine(enemy.Visuals.FocusAttacker(false));
        }
    }

    // --- UI & INPUT ---
    void SpawnSkillButtons()
    {
        foreach (Transform child in skillButtonContainer) Destroy(child.gameObject);
        foreach (var skill in playerUnit.activeSkills) CreateButton(skill);
    }

    void CreateButton(RuntimeSkill skill)
    {
        GameObject go = Instantiate(skillButtonPrefab, skillButtonContainer);
        SkillButton btnScript = go.GetComponent<SkillButton>();
        btnScript.Setup(skill, OnSkillClicked);
    }

    void UpdateSkillButtons()
    {
        SkillButton[] buttons = skillButtonContainer.GetComponentsInChildren<SkillButton>();
        foreach (var btn in buttons) btn.UpdateState();
    }

    void OnSkillClicked(RuntimeSkill skill)
    {
        if (playerUnit.currentHP <= 0) return;
        if (state != BattleState.AUTO_BATTLE && state != BattleState.TARGETING) return;
        isPaused = true;
        state = BattleState.TARGETING;
        selectedSkill = skill;
    }

    void Update()
    {
        if (state == BattleState.TARGETING)
        {
            if (Input.GetMouseButtonDown(1)) CancelTargeting();
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

                if (hit.collider != null)
                {
                    BattleUnit target = hit.collider.GetComponent<BattleUnit>();
                    if (target != null && !target.isPlayerTeam && target.currentHP > 0)
                    {
                        StartCoroutine(ExecuteSkillAndResume(target));
                    }
                }
            }
        }
    }

    void CancelTargeting()
    {
        isPaused = false;
        state = BattleState.AUTO_BATTLE;
        selectedSkill = null;
    }

    IEnumerator ExecuteSkillAndResume(BattleUnit mainTarget)
    {
        state = BattleState.AUTO_BATTLE;

        if (playerUnit.currentHP <= 0) { selectedSkill = null; isPaused = false; yield break; }
        RuntimeSkill currentSkill = selectedSkill; // LOKALE KOPIE
        if (currentSkill == null) yield break;

        if (playerUnit.Visuals != null) yield return StartCoroutine(playerUnit.Visuals.FocusAttacker(true));

        if (playerUnit.attackRange == AttackRange.Melee && playerUnit.Visuals != null)
            yield return StartCoroutine(playerUnit.Visuals.MoveToTarget(mainTarget.transform.position));
        else
            yield return new WaitForSeconds(0.2f);

        float dmg = playerUnit.GetDamageValue() * currentSkill.template.damageMultiplier;

        // HAUPTZIEL
        DealDamageTo(mainTarget, dmg, currentSkill);

        // AOE
        if (currentSkill.template.splashDamagePercent > 0)
        {
            foreach (var neighbor in GetNeighbors(mainTarget))
            {
                DealDamageTo(neighbor, dmg * currentSkill.template.splashDamagePercent, currentSkill);
            }
        }

        currentSkill.currentCooldown = currentSkill.template.rechargeTurns;
        UpdateSkillButtons();
        yield return new WaitForSeconds(0.5f);

        if (playerUnit.attackRange == AttackRange.Melee && playerUnit.Visuals != null)
            yield return StartCoroutine(playerUnit.Visuals.ReturnToStart());

        if (playerUnit.Visuals != null) yield return StartCoroutine(playerUnit.Visuals.FocusAttacker(false));

        selectedSkill = null;
        isPaused = false;
    }

    // --- DAMAGE LOGIC ---

    void DealDamageTo(BattleUnit target, float rawDmg, RuntimeSkill sourceSkill = null)
    {
        // 1. Miss
        if (Random.value < 0.10f) { target.TakeDamage(0, HitType.Miss); return; }

        // 2. Crit
        bool isCrit = Random.value < 0.20f;
        float finalDmg = isCrit ? rawDmg * 1.5f : rawDmg;
        HitType type = isCrit ? HitType.Critical : HitType.Normal;

        // 3. Defense
        float def = target.GetDefenseValue();
        finalDmg = Mathf.Max(finalDmg - def, 1);

        Sprite icon = (sourceSkill != null && sourceSkill.template != null) ? sourceSkill.template.icon : null;

        // 4. Anwenden & Check Death
        bool isDead = target.TakeDamage(finalDmg, type, StatusEffectType.None, icon);

        // 5. Effekte
        if (sourceSkill != null && sourceSkill.template.effects != null)
        {
            foreach (var effectConfig in sourceSkill.template.effects)
            {
                if (Random.value <= effectConfig.chance)
                {
                    BattleUnit effectTarget = effectConfig.applyToSelf ? playerUnit : target;
                    effectTarget.ApplyEffect(effectConfig);
                }
            }
        }

        // 6. VFX
        if (sourceSkill != null && sourceSkill.template.vfxPrefab != null)
        {
            Instantiate(sourceSkill.template.vfxPrefab, target.transform.position, Quaternion.identity);
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
        if (!enemyUnits.Any(e => e.currentHP > 0)) { state = BattleState.WON; BattlefieldManager.Instance.OnWaveCleared(); return true; }
        if (playerUnit.currentHP <= 0) { state = BattleState.LOST; BattlefieldManager.Instance.OnPlayerDefeated(); return true; }
        return false;
    }
}