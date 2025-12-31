using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour
{
    public int gridX, gridY;
    public bool isPlayer = false;
    public CardData myData;

    [Header("UI Referenzen")]
    public TextMeshPro nameText;

    private SpriteRenderer spriteRenderer;
    private Sprite defaultShape;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultShape = spriteRenderer.sprite;
        if (nameText == null) nameText = GetComponentInChildren<TextMeshPro>();
    }

    public void SetupCard(int x, int y, CardData data, bool playerStatus)
    {
        gridX = x; gridY = y;
        isPlayer = playerStatus;
        myData = data;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (nameText != null) nameText.text = "";
        spriteRenderer.sprite = defaultShape;

        if (isPlayer)
        {
            spriteRenderer.color = Color.green;
            if (nameText != null) nameText.text = "Hero";
        }
        else if (myData != null)
        {
            if (nameText != null) nameText.text = myData.cardName;

            if (myData.artwork != null)
            {
                spriteRenderer.sprite = myData.artwork;
                spriteRenderer.color = Color.white;
            }
            else
            {
                // FIX: Alle CardData.CardType Präfixe entfernt
                switch (myData.cardType)
                {
                    case CardType.Enemy: spriteRenderer.color = new Color(1f, 0.3f, 0.3f); break;
                    case CardType.Potion: spriteRenderer.color = new Color(0.3f, 0.3f, 1f); break;
                    case CardType.Weapon: spriteRenderer.color = new Color(1f, 0.6f, 0f); break;
                    case CardType.Armor: spriteRenderer.color = Color.gray; break;
                    case CardType.Gold: spriteRenderer.color = Color.yellow; break;
                    default: spriteRenderer.color = Color.white; break;
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        GridManager.Instance.TryMovePlayer(this);
    }
}