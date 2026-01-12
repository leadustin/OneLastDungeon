using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Einstellungen")]
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);

    // --- NEU: Ein Schalter, um die Kamera anzuhalten ---
    public bool isLocked = false;

    void LateUpdate()
    {
        // Wenn kein Ziel da ist ODER die Kamera gesperrt ist -> Abbruch
        if (target == null || isLocked) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}