using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems; // Wichtig für UI Blockierung

public enum BattleState { SETUP, ATB_RUNNING, PLAYER_INPUT, TARGETING, EXECUTING, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;
    public BattleState state;

    [Header("Einstellungen")]
    public bool isAutoBattle = false;
    public float battleSpeed = 1.0f;

    [Header("UI Referenzen")]
    public GameObject startBattleButton;
    public GameObject autoBattleToggleVisual;

    [Header("Hero HUD")]
    public Transform heroHUDContainer;
    public GameObject heroPanelPrefab;

    [Header("Kampf Daten")]
    private BattleUnit playerUnit;
    private List<BattleUnit> enemyUnits;
    private List<BattleUnit> allUnits = new List<BattleUnit>();

    // Cache
    private BattleUnit activeUnit;
    public BattleUnit ActiveUnit => activeUnit; // Zugriff für UI

    private RuntimeSkill selectedSkill;
    private RuntimeSkill fallbackAttackSkill;

    void Awake() { Instance = this; }

    public void StartBattle(BattleUnit player, List<BattleUnit> enemies)
    {
        playerUnit = player;
        enemyUnits = enemies;
        allUnits.Clear();
        allUnits.Add(player);
        allUnits.AddRange(enemies);

        // 1. Initialisierung
        player.Initialize(true);
        foreach (var e in enemies) e.Initialize(false);

        // 2. Marker Logik (Identische Gegner gruppieren und nummerieren)
        var groupedEnemies = enemies.GroupBy(e => e.unitName);
        foreach (var group in groupedEnemies)
        {
            if (group.Count() > 1)
            {
                int i = 0;
                foreach (var e in group) e.markerIndex = i++ % 5;
            }
            else
            {
                foreach (var e in group) e.markerIndex = -1;
            }
        }

        // 3. Visuals & Skills
        if (BattlefieldManager.Instance != null) BattlefieldManager.Instance.ToggleSlotVisuals(true);
        CreateFallbackAttackSkill();
        CreateHeroHUD();

        // 4. UI Reset
        if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Hide();
        if (startBattleButton != null) startBattleButton.SetActive(true);

        state = BattleState.SETUP;
    }

    public void OnStartBattleClicked()
    {
        if (state != BattleState.SETUP) return;
        if (startBattleButton != null) startBattleButton.SetActive(false);
        state = BattleState.ATB_RUNNING;
    }

    public void ToggleAutoBattle()
    {
        isAutoBattle = !isAutoBattle;
        if (autoBattleToggleVisual != null) autoBattleToggleVisual.SetActive(isAutoBattle);

        // Wenn wir im Menü warten und Auto anmachen -> Sofort loslegen
        if (isAutoBattle && state == BattleState.PLAYER_INPUT)
        {
            if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Hide();
            StartCoroutine(ExecuteAutoTurn(activeUnit));
        }
    }

    // --- CORE LOOP ---

    void Update()
    {
        // 1. ATB LÄUFT (Wenn niemand am Zug ist)
        if (state == BattleState.ATB_RUNNING)
        {
            bool someoneReady = false;
            foreach (var unit in allUnits)
            {
                if (unit.currentHP > 0)
                {
                    unit.TickATB(Time.deltaTime * battleSpeed * 5f);
                    if (unit.currentATB >= unit.maxATB) someoneReady = true;
                }
            }
            if (someoneReady) HandleReadyUnits();
        }

        // 2. TARGETING INPUT (Nur wenn Spieler Ziel wählt)
        if (state == BattleState.TARGETING && !isAutoBattle)
        {
            HandleTargetingInput();
        }
    }

    void HandleReadyUnits()
    {
        // Wer ist bereit? (Sortiert nach höchster ATB, dann Speed)
        var readyUnit = allUnits
            .Where(u => u.currentHP > 0 && u.currentATB >= u.maxATB)
            .OrderByDescending(u => u.currentATB)
            .ThenByDescending(u => u.speed)
            .FirstOrDefault();

        if (readyUnit != null)
        {
            StartTurn(readyUnit);
        }
    }

    // --- ZUG LOGIK ---

    void StartTurn(BattleUnit unit)
    {
        activeUnit = unit;
        activeUnit.OnTurnStart(); // Cooldowns ticken, DoTs wirken

        // Falls Unit an DoT gestorben ist
        if (activeUnit.currentHP <= 0)
        {
            EndTurn();
            return;
        }

        if (unit.isPlayerTeam)
        {
            if (isAutoBattle)
            {
                StartCoroutine(ExecuteAutoTurn(unit));
            }
            else
            {
                // UI ANZEIGEN (State: PLAYER_INPUT)
                state = BattleState.PLAYER_INPUT;
                if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Show(unit);
            }
        }
        else
        {
            // GEGNER AI
            state = BattleState.EXECUTING;
            StartCoroutine(ExecuteAutoTurn(unit));
        }
    }

    // --- MENU CALLBACKS (Vom BattleMenuUI aufgerufen) ---

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYER_INPUT) return;
        if (fallbackAttackSkill == null) CreateFallbackAttackSkill();

        InitiateTargeting(fallbackAttackSkill);
    }

    public void OnDefendButton()
    {
        if (state != BattleState.PLAYER_INPUT) return;

        // Defend Logik: Zug überspringen, evtl. Buff geben (hier simpel gehalten)
        Debug.Log($"{activeUnit.unitName} verteidigt sich.");
        if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Hide();

        EndTurn();
    }

    public void OnSkillSelectedFromMenu(RuntimeSkill skill)
    {
        if (state != BattleState.PLAYER_INPUT) return;
        InitiateTargeting(skill);
    }

    public void CancelTargeting()
    {
        if (state != BattleState.TARGETING) return;

        // Highlights entfernen
        HighlightValidTargets(null, false);

        // Zurück ins Menü
        state = BattleState.PLAYER_INPUT;
        if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Show(activeUnit);
    }

    // --- TARGETING LOGIC ---

    void InitiateTargeting(RuntimeSkill skill)
    {
        selectedSkill = skill;

        // A) Sofort-Effekte (Kein Targeting nötig)
        if (skill.template.targetMode == SkillTargetMode.Self ||
            skill.template.targetMode == SkillTargetMode.AllEnemies ||
            skill.template.targetMode == SkillTargetMode.AllAllies)
        {
            if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Hide();

            // PerformMove mit 'null' triggert die Auto-Target Logik in PerformMove
            StartCoroutine(PerformMove(activeUnit, null, selectedSkill));
            return;
        }

        // B) Manuelle Auswahl nötig (Single Target)
        state = BattleState.TARGETING;
        if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.SetTargetingMode(true); // UI ausblenden, Back-Button an

        // Visuelles Feedback: Wen darf ich wählen?
        HighlightValidTargets(skill, true);
    }

    void HandleTargetingInput()
    {
        // Verhindern, dass wir durch UI-Buttons hindurchklicken
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;

        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 inputPos = (Input.touchCount > 0) ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(inputPos);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null)
            {
                BattleUnit touched = hit.collider.GetComponent<BattleUnit>();
                if (touched != null && IsValidTarget(touched, selectedSkill))
                {
                    // ZIEL BESTÄTIGT
                    HighlightValidTargets(null, false); // Alles aus
                    if (BattleMenuUI.Instance != null) BattleMenuUI.Instance.Hide();

                    StartCoroutine(PerformMove(activeUnit, touched, selectedSkill));
                }
            }
        }
    }

    // --- EXECUTION (Animation & Damage) ---

    IEnumerator PerformMove(BattleUnit source, BattleUnit mainTarget, RuntimeSkill skill)
    {
        state = BattleState.EXECUTING; // Sperrt Input und ATB

        // 1. Bewegung zum Ziel (außer Self/Global)
        if (source.Visuals != null &&
            skill.template.targetMode != SkillTargetMode.Self &&
            skill.template.targetMode != SkillTargetMode.AllEnemies &&
            skill.template.targetMode != SkillTargetMode.AllAllies)
        {
            Vector3 targetPos = (mainTarget != null) ? mainTarget.transform.position : source.transform.position;
            // Falls kein Ziel (Fallback), bleib stehen oder geh leicht vor
            if (mainTarget == null && !source.isPlayerTeam) targetPos = playerUnit.transform.position;

            yield return StartCoroutine(source.Visuals.MoveToTarget(targetPos));
        }

        yield return new WaitForSeconds(0.1f);

        // 2. Ziele bestimmen (falls noch nicht geschehen oder AoE)
        List<BattleUnit> targets = new List<BattleUnit>();

        if (skill.template.targetMode == SkillTargetMode.AllEnemies)
        {
            targets.AddRange(enemyUnits.Where(e => e.currentHP > 0));
        }
        else if (skill.template.targetMode == SkillTargetMode.AllAllies)
        {
            // Annahme: Aktuell nur 1 Held, später hier alle Helden adden
            targets.Add(playerUnit);
        }
        else if (skill.template.targetMode == SkillTargetMode.Self)
        {
            targets.Add(source);
        }
        else if (mainTarget != null)
        {
            targets.Add(mainTarget);
        }

        // 3. Effekt anwenden (Schaden/Heilung)
        float baseDmg = source.GetRawStat(StatType.PhysicalDamage) * skill.template.damageMultiplier;

        foreach (var t in targets)
        {
            // Special Case: Heilung (Wenn Holy Damage auf sich selbst/Ally)
            // (Kann man später schöner über SkillType lösen)
            if (skill.template.damageType == DamageType.Holy && (t.isPlayerTeam == source.isPlayerTeam))
            {
                t.Heal(baseDmg);
                continue;
            }

            // Crit Check
            HitType hitType = HitType.Normal;
            if (Random.value < CombatMath.CalculateChance(source.GetRawStat(StatType.CritChance), source.currentLevel))
                hitType = HitType.Critical;

            // Apply Damage
            ApplyDamage(source, t, baseDmg, skill.template.damageType, hitType, skill.template.damageMultiplier);

            // VFX
            if (skill.template.vfxPrefab != null)
                Instantiate(skill.template.vfxPrefab, t.transform.position, Quaternion.identity);
        }

        skill.OnUse(); // Cooldown setzen
        yield return new WaitForSeconds(0.5f);

        // 4. Zurücklaufen
        if (source.Visuals != null) yield return StartCoroutine(source.Visuals.ReturnToStart());

        EndTurn();
    }

    void ApplyDamage(BattleUnit source, BattleUnit target, float baseAmount, DamageType baseType, HitType hitType, float multiplier)
    {
        // Haupt-Schaden
        target.TakeDamage(baseAmount, hitType, source.currentLevel, baseType);

        // Hybrid-Schaden Loop (Elementar-Bonus)
        foreach (DamageType dt in System.Enum.GetValues(typeof(DamageType)))
        {
            if (dt == DamageType.Physical || dt == baseType) continue; // Nicht doppelt berechnen

            float bonusStat = source.GetRawStat(GetOffensiveStatFor(dt));
            if (bonusStat > 0)
            {
                target.TakeDamage(bonusStat * multiplier, hitType, source.currentLevel, dt);
            }
        }
    }

    IEnumerator ExecuteAutoTurn(BattleUnit unit)
    {
        // Kurze Denkpause
        yield return new WaitForSeconds(0.3f);

        // 1. Skill wählen (Erster bereiter Skill, der nicht Attack ist, sonst Attack)
        RuntimeSkill chosen = unit.activeSkills.FirstOrDefault(s => s.IsReady() && s.template != fallbackAttackSkill.template);
        if (chosen == null)
        {
            if (fallbackAttackSkill == null) CreateFallbackAttackSkill();
            chosen = fallbackAttackSkill;
        }

        // 2. Ziel wählen (Simpel: Erster lebender Feind)
        BattleUnit target = null;
        if (unit.isPlayerTeam) target = GetFirstAliveEnemy();
        else target = playerUnit;

        // 3. Ausführen
        yield return StartCoroutine(PerformMove(unit, target, chosen));
    }

    void EndTurn()
    {
        if (activeUnit != null)
        {
            activeUnit.ResetATB();
            activeUnit = null;
        }

        if (CheckBattleOver()) return;

        state = BattleState.ATB_RUNNING;
    }

    // --- HELPER & TOOLS ---

    bool CheckBattleOver()
    {
        if (!enemyUnits.Any(e => e.currentHP > 0))
        {
            state = BattleState.WON;
            BattlefieldManager.Instance?.OnBattleWon();
            return true;
        }
        if (playerUnit.currentHP <= 0)
        {
            state = BattleState.LOST;
            BattlefieldManager.Instance?.OnBattleLost();
            return true;
        }
        return false;
    }

    bool IsValidTarget(BattleUnit target, RuntimeSkill skill)
    {
        if (target == null || target.currentHP <= 0) return false;

        if (skill.template.targetMode == SkillTargetMode.SingleEnemy && !target.isPlayerTeam) return true;
        if (skill.template.targetMode == SkillTargetMode.SingleAlly && target.isPlayerTeam) return true;

        // Sonderfall: Heilung auf sich selbst wird meist über Self Mode geregelt, 
        // aber falls SingleAlly auch Self erlaubt:
        if (skill.template.targetMode == SkillTargetMode.SingleAlly && target == activeUnit) return true;

        return false;
    }

    void HighlightValidTargets(RuntimeSkill skill, bool active)
    {
        foreach (var unit in allUnits)
        {
            if (unit.Visuals == null) continue;

            if (!active)
            {
                unit.Visuals.ToggleHighlight(false);
                continue;
            }

            if (IsValidTarget(unit, skill))
            {
                unit.Visuals.ToggleHighlight(true);
            }
            else
            {
                unit.Visuals.ToggleHighlight(false);
            }
        }
    }

    // Wird vom UI aufgerufen (bidirektionales Highlight)
    public void HighlightUnitInWorld(BattleUnit unit)
    {
        foreach (var u in allUnits)
        {
            if (u.Visuals != null) u.Visuals.ToggleHighlight(u == unit);
        }
    }

    // Turn Prediction für die Leiste
    public List<BattleUnit> PredictTurnOrder(int count = 5)
    {
        var predictions = new List<BattleUnit>();
        Dictionary<BattleUnit, float> tempATB = new Dictionary<BattleUnit, float>();
        foreach (var u in allUnits) if (u.currentHP > 0) tempATB[u] = u.currentATB;

        for (int i = 0; i < count; i++)
        {
            BattleUnit winner = null;
            float minTime = float.MaxValue;

            foreach (var u in allUnits)
            {
                if (u.currentHP <= 0) continue;
                float needed = u.maxATB - tempATB[u];
                if (needed <= 0) needed = 0;

                float time = needed / Mathf.Max(1f, u.speed);
                if (time < minTime)
                {
                    minTime = time;
                    winner = u;
                }
            }

            if (winner != null)
            {
                predictions.Add(winner);
                foreach (var key in tempATB.Keys.ToList())
                {
                    tempATB[key] += minTime * key.speed;
                }
                tempATB[winner] -= winner.maxATB;
            }
            else break;
        }
        return predictions;
    }

    void CreateHeroHUD()
    {
        foreach (Transform child in heroHUDContainer) Destroy(child.gameObject);

        GameObject go = Instantiate(heroPanelPrefab, heroHUDContainer);
        HeroStatusPanel panel = go.GetComponent<HeroStatusPanel>();
        if (panel != null) panel.Setup(playerUnit);
    }

    void CreateFallbackAttackSkill()
    {
        if (fallbackAttackSkill != null) return;
        SkillTemplate tmpl = ScriptableObject.CreateInstance<SkillTemplate>();
        tmpl.skillName = "Attack";
        tmpl.damageMultiplier = 1.0f;
        tmpl.rechargeTurns = 0;
        tmpl.initialTurnDelay = 0;
        tmpl.targetMode = SkillTargetMode.SingleEnemy;
        tmpl.damageType = DamageType.Physical;
        fallbackAttackSkill = new RuntimeSkill(tmpl);
    }

    BattleUnit GetFirstAliveEnemy() => enemyUnits.FirstOrDefault(e => e.currentHP > 0);

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