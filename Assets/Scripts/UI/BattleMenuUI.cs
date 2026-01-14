using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BattleMenuUI : MonoBehaviour
{
    public static BattleMenuUI Instance;

    [Header("Panel Container")]
    public GameObject rootPanel;      // Das Panel, das sich bewegen soll
    public GameObject primaryMenu;    // Buttons (Attack, Skills...)
    public GameObject subMenu;        // Liste (Feuerball, Heilung...)

    [Header("Positionierung")]
    public Vector3 menuOffset = new Vector3(1.5f, 0, 0); // Standard: Rechts neben dem Helden

    [Header("Primary Buttons")]
    public Button btnAttack;
    public Button btnSkills;
    public Button btnItems;
    public Button btnDefend;
    public Button btnBack;

    [Header("SubMenu Listen")]
    public Transform listContainer;
    public GameObject actionButtonPrefab;

    private BattleUnit activeHero;

    void Awake()
    {
        Instance = this;

        btnAttack.onClick.AddListener(() => BattleSystem.Instance.OnAttackButton());
        btnSkills.onClick.AddListener(() => ShowSkillsList());
        btnItems.onClick.AddListener(() => Debug.Log("Items noch nicht implementiert"));
        btnDefend.onClick.AddListener(() => BattleSystem.Instance.OnDefendButton());

        btnBack.onClick.AddListener(OnBackClicked);
    }

    // WICHTIG: Jeden Frame die Position updaten
    void Update()
    {
        // Nur bewegen, wenn das Menü offen ist und wir einen Helden haben
        if (rootPanel.activeSelf && activeHero != null)
        {
            // 1. World Position des Helden + Offset (in 3D)
            Vector3 targetWorldPos = activeHero.transform.position + menuOffset;

            // 2. Umrechnen in Screen Position (für UI)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldPos);

            // 3. UI dorthin setzen
            rootPanel.transform.position = screenPos;
        }
    }

    public void Show(BattleUnit hero)
    {
        activeHero = hero;
        rootPanel.SetActive(true);
        GoToPrimary();

        // Optional: Einmaliges Update sofort, damit es nicht für 1 Frame an der falschen Stelle blitzt
        Update();
    }

    public void Hide()
    {
        rootPanel.SetActive(false);
    }

    // --- NAVIGATION (Bleibt gleich) ---

    void GoToPrimary()
    {
        primaryMenu.SetActive(true);
        subMenu.SetActive(false);
        btnBack.gameObject.SetActive(false);
    }

    void ShowSkillsList()
    {
        primaryMenu.SetActive(false);
        subMenu.SetActive(true);
        btnBack.gameObject.SetActive(true);

        foreach (Transform child in listContainer) Destroy(child.gameObject);

        foreach (var skill in activeHero.activeSkills)
        {
            GameObject go = Instantiate(actionButtonPrefab, listContainer);
            go.GetComponent<ActionButton>().SetupSkill(skill, OnSkillSelected);
        }
    }

    void OnSkillSelected(RuntimeSkill skill)
    {
        BattleSystem.Instance.OnSkillSelectedFromMenu(skill);
    }

    void OnBackClicked()
    {
        if (subMenu.activeSelf) GoToPrimary();
        else if (BattleSystem.Instance.state == BattleState.TARGETING) BattleSystem.Instance.CancelTargeting();
    }

    public void SetTargetingMode(bool active)
    {
        // Im Floating Mode wollen wir das Menü beim Zielen vielleicht komplett ausblenden,
        // damit es nicht im Weg ist? 
        // Oder wir lassen nur den Back-Button da.
        rootPanel.SetActive(active); // Wenn wir zielen (active=true), Menü an? Nein, eher aus.

        // Anpassung für Floating UX:
        // Wenn Targeting aktiv ist -> Menü ausblenden (wir brauchen freie Sicht).
        // Wenn Targeting abgebrochen wird -> Show() wird eh vom BattleSystem gerufen.

        if (active)
        {
            // Wir verstecken das Hauptmenü, zeigen aber vllt. irgendwo einen "Cancel" Button
            // Fürs Erste: Menü ganz weg beim Zielen.
            rootPanel.SetActive(false);
        }
    }
}