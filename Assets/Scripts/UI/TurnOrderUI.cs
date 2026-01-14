using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TurnOrderUI : MonoBehaviour
{
    public static TurnOrderUI Instance; // Singleton für einfachen Zugriff

    [Header("Konfiguration")]
    public GameObject slotPrefab;
    public Transform container;
    public int slotCount = 6;

    [Header("Marker Assets (Formen)")]
    // Zieh hier deine Sprites rein: Kreis, Viereck, Dreieck, Raute, Kreuz
    public Sprite[] markerSprites;

    private List<TurnOrderSlot> spawnedSlots = new List<TurnOrderSlot>();
    private float updateTimer;

    void Awake() { Instance = this; }

    void Start()
    {
        // Pool erstellen
        for (int i = 0; i < slotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, container);
            TurnOrderSlot slot = go.GetComponent<TurnOrderSlot>(); // Nutzt jetzt das neue Script
            if (slot == null) slot = go.AddComponent<TurnOrderSlot>();

            // Referenzen autom. suchen falls im Inspector nicht gesetzt
            if (slot.frameImage == null) slot.frameImage = go.GetComponent<Image>();
            if (slot.iconImage == null) slot.iconImage = go.transform.Find("Icon")?.GetComponent<Image>();
            // Marker Image suchen (muss im Prefab existieren, z.B. oben rechts im Eck)
            if (slot.markerImage == null) slot.markerImage = go.transform.Find("Marker")?.GetComponent<Image>();

            spawnedSlots.Add(slot);
            go.SetActive(false);
        }
    }

    void Update()
    {
        // Aktualisierung der Liste (nicht zu oft für Performance)
        updateTimer += Time.deltaTime;
        if (updateTimer < 0.1f) return;
        updateTimer = 0;

        RefreshList();
    }

    void RefreshList()
    {
        if (BattleSystem.Instance == null) return;

        List<BattleUnit> predictedOrder = BattleSystem.Instance.PredictTurnOrder(slotCount);
        BattleUnit currentSelected = null;
        // Falls wir wissen wollen, wen der Spieler gerade targetet (optional)
        // currentSelected = BattleSystem.Instance.CurrentTarget; 

        for (int i = 0; i < slotCount; i++)
        {
            TurnOrderSlot slot = spawnedSlots[i];

            if (i < predictedOrder.Count)
            {
                BattleUnit unit = predictedOrder[i];
                slot.gameObject.SetActive(true);

                // Welchen Marker hat die Unit?
                Sprite marker = null;
                if (unit.markerIndex >= 0 && unit.markerIndex < markerSprites.Length)
                {
                    marker = markerSprites[unit.markerIndex];
                }

                slot.Setup(unit, marker);

                // Bidirektionales Highlight (Ist diese Unit gerade im Fokus?)
                // Hier prüfen wir einfach: Ist es die Unit, auf die wir gerade zielen?
                // (Logik muss im BattleSystem 'activeTarget' speichern wenn man tippt)
                // bool isTargeted = (unit == currentSelected);
                // slot.SetVisualHighlight(isTargeted);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    // API: Wird vom BattleSystem gerufen, wenn man im 3D Raum tippt
    public void HighlightSlotForUnit(BattleUnit unit)
    {
        foreach (var slot in spawnedSlots)
        {
            // Wir vergleichen hier Referenzen. Vorsicht: Wenn Unit 2x in Liste ist, leuchten beide!
            // Das ist bei Option C (Instanz-basiert) aber korrekt und gewollt.
            // slot.SetVisualHighlight(slot.LinkedUnit == unit); 
            // (Dafür müsstest du LinkedUnit im Slot public machen)
        }
    }
}