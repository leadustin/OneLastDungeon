using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemInfoUI : MonoBehaviour
{
    public static ItemInfoUI Instance;

    [Header("UI Referenzen")]
    public GameObject windowObject;
    public Image itemIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI actionButtonText;

    [Header("Tooltip Einstellungen")]
    public Vector2 offset = new Vector2(50, -50);
    public float fadeSpeed = 5f;

    private CardData currentItem;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        Instance = this;
        if (windowObject != null)
        {
            rectTransform = windowObject.GetComponent<RectTransform>();
            canvasGroup = windowObject.GetComponent<CanvasGroup>();
            windowObject.SetActive(false);
        }
    }

    public void OpenInfo(CardData item)
    {
        if (item == null) return;
        currentItem = item;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        windowObject.SetActive(true);
        canvasGroup.alpha = 0;

        UpdatePosition();

        if (itemIcon != null) { itemIcon.sprite = item.artwork; itemIcon.preserveAspect = true; }
        if (nameText != null) nameText.text = item.cardName;
        if (descriptionText != null) descriptionText.text = item.description;

        if (statsText != null) statsText.text = GetStatsString(item);

        if (actionButtonText != null) actionButtonText.text = (item is ConsumableData) ? "Benutzen" : "Ausrüsten";

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private void UpdatePosition()
    {
        if (rectTransform == null) return;

        Vector3 newPos = Input.mousePosition + (Vector3)offset;
        float windowWidth = rectTransform.rect.width * rectTransform.lossyScale.x;
        float windowHeight = rectTransform.rect.height * rectTransform.lossyScale.y;

        if (newPos.x + windowWidth > Screen.width)
            newPos.x = Input.mousePosition.x - windowWidth - offset.x;

        if (newPos.y - windowHeight < 0)
            newPos.y = Input.mousePosition.y + windowHeight + Mathf.Abs(offset.y);

        windowObject.transform.position = newPos;
    }

    public void CloseInfo()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        windowObject.SetActive(false);
        currentItem = null;
    }

    public void OnActionClick()
    {
        if (currentItem != null)
        {
            PlayerManager.Instance.UseItem(currentItem);
            CloseInfo();
        }
    }

    private string GetStatsString(CardData item)
    {
        string result = "";

        // Vergleich mit Waffen (NEU: Range statt festem Wert)
        if (item is WeaponData w)
        {
            // Wir zeigen einfach die Range der Waffe an
            result = $"Schaden: {w.minDamage} - {w.maxDamage}";
        }
        else if (item is ArmorData a)
        {
            int currentDef = PlayerManager.Instance.defense;
            int diff = a.defenseAmount - currentDef;
            string diffColor = diff >= 0 ? "green" : "red";
            string sign = diff >= 0 ? "+" : "";

            result = $"Rüstung: {a.defenseAmount} (<color={diffColor}>{sign}{diff}</color>)";
        }
        else if (item is AccessoryData acc)
        {
            result = $"Bonus: DMG +{acc.bonusDamage} / DEF +{acc.bonusDefense}";
        }
        else if (item is ConsumableData c)
        {
            result = $"Heilung: {c.minAmount}-{c.maxAmount}";
        }

        return result;
    }
}