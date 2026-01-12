using UnityEngine;

public class UnitDragger : MonoBehaviour
{
    private BattleUnit selectedUnit;
    private Vector3 originalPosition;
    private bool isDragging = false;

    [Header("Einstellungen")]
    public float snapDistance = 1.0f; // Wie nah muss man am Slot sein?

    void Update()
    {
        // Drag & Drop ist NUR erlaubt in der SETUP Phase
        if (BattleSystem.Instance == null || BattleSystem.Instance.state != BattleState.SETUP) return;

        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        // 1. Klick Starten (Unit greifen)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                BattleUnit unit = hit.collider.GetComponent<BattleUnit>();
                // Wir dürfen nur eigene Einheiten bewegen!
                if (unit != null && unit.isPlayerTeam)
                {
                    selectedUnit = unit;
                    originalPosition = unit.transform.position;
                    isDragging = true;
                }
            }
        }

        // 2. Ziehen (Unit folgt der Maus)
        if (isDragging && selectedUnit != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0; // Wichtig bei 2D
            selectedUnit.transform.position = mousePos;
        }

        // 3. Loslassen (Auf Slot einrasten)
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            SnapToNearestSlot();
            selectedUnit = null;
            isDragging = false;
        }
    }

    void SnapToNearestSlot()
    {
        Transform nearestSlot = null;
        float minDistance = float.MaxValue;

        // Slot suchen (wie vorher) ...
        if (BattlefieldManager.Instance != null)
        {
            foreach (Transform slot in BattlefieldManager.Instance.playerSlots)
            {
                float dist = Vector2.Distance(selectedUnit.transform.position, slot.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestSlot = slot;
                }
            }
        }

        if (nearestSlot != null && minDistance <= snapDistance)
        {
            // Parent setzen
            selectedUnit.transform.SetParent(nearestSlot);

            // WICHTIG: Position nullen, aber Z-Achse auf -1 zwingen (näher zur Kamera)
            selectedUnit.transform.localPosition = new Vector3(0, 0, -1);

            // WICHTIG: Skalierung zurücksetzen (falls der Slot skaliert ist)
            selectedUnit.transform.localScale = Vector3.one;
        }
        else
        {
            // Zurück zum Start
            selectedUnit.transform.position = originalPosition;
            // Auch hier sicherheitshalber Z korrigieren
            Vector3 fixedPos = originalPosition;
            fixedPos.z = -1;
            selectedUnit.transform.position = fixedPos;
        }
    }
}