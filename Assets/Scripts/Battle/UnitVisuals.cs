using System.Collections;
using UnityEngine;

public class UnitVisuals : MonoBehaviour
{
    [Header("Referenzen")]
    public SpriteRenderer unitRenderer;
    public Animator animator;

    [Header("Einstellungen")]
    public float moveSpeed = 10f;
    public float scalePunch = 1.2f;

    // NEU: Wie schnell soll die Todes-Animation laufen?
    public float deathFrameDuration = 0.08f;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalColor;

    // NEU: Speicher für die Animation
    private Sprite[] deathFrames;

    void Awake()
    {
        if (unitRenderer == null) unitRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (unitRenderer != null) originalColor = unitRenderer.color;
        originalPosition = transform.position;
    }

    // --- GEÄNDERT: Nimmt jetzt auch die Animation entgegen ---
    public void SetData(Sprite icon, Sprite[] deathAnimSprites = null)
    {
        if (unitRenderer != null && icon != null)
        {
            unitRenderer.sprite = icon;
            unitRenderer.color = originalColor; // Farbe resetten
        }
        // Wir speichern die Liste für später
        this.deathFrames = deathAnimSprites;
    }

    public void FlipOrientation(bool facingRight)
    {
        if (unitRenderer != null)
        {
            unitRenderer.flipX = !facingRight;
        }
    }

    // --- NEU: Die Todes-Animation ---
    public IEnumerator PlayDeathAnimation()
    {
        // Haben wir Bilder?
        if (deathFrames != null && deathFrames.Length > 0)
        {
            // Optional: Einmal kurz weiß aufblitzen
            if (unitRenderer != null) unitRenderer.color = new Color(1, 0.5f, 0.5f);

            // Jedes Bild im Array abspielen
            foreach (Sprite frame in deathFrames)
            {
                if (unitRenderer != null) unitRenderer.sprite = frame;
                yield return new WaitForSeconds(deathFrameDuration);
            }
            // Kurz warten, damit man das letzte Bild (z.B. Rauch) noch sieht
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // Fallback: Wenn keine Bilder da sind, einfach rot werden und ausblenden
            if (unitRenderer != null) unitRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            float fadeTime = 0.5f;
            float timer = 0;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
                if (unitRenderer != null)
                {
                    Color c = unitRenderer.color;
                    c.a = alpha;
                    unitRenderer.color = c;
                }
                yield return null;
            }
        }

        // Am Ende ganz unsichtbar machen
        if (unitRenderer != null)
        {
            Color final = unitRenderer.color;
            final.a = 0;
            unitRenderer.color = final;
        }
    }

    // --- KAMPF ANIMATIONEN (Unverändert) ---
    public IEnumerator FocusAttacker(bool active)
    {
        if (active)
        {
            transform.localScale = originalScale * scalePunch;
            if (unitRenderer != null) unitRenderer.color = Color.white;
        }
        else
        {
            transform.localScale = originalScale;
            if (unitRenderer != null) unitRenderer.color = originalColor;
        }
        yield return null;
    }

    public IEnumerator MoveToTarget(Vector3 targetPos)
    {
        originalPosition = transform.position;
        Vector3 direction = (targetPos - transform.position).normalized;
        Vector3 endPos = targetPos - (direction * 0.8f);

        while (Vector3.Distance(transform.position, endPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator ReturnToStart()
    {
        while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = originalPosition;
    }
}