using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetItemRow : MonoBehaviour
{
    [Header("UI Elemente")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI bonusText;

    // Wir nehmen jetzt den fertigen "bonusString" entgegen
    public void Setup(CardData item, bool isEquipped, string bonusString)
    {
        if (item == null) return;

        // 1. Icon
        if (iconImage != null)
        {
            iconImage.sprite = item.artwork;
            iconImage.preserveAspect = true;
            iconImage.color = isEquipped ? Color.white : new Color(1, 1, 1, 0.5f);
        }

        // 2. Name
        if (nameText != null)
        {
            nameText.text = item.cardName;
            // Grün wenn ausgerüstet, sonst Grau
            nameText.color = isEquipped ? new Color(0, 1, 0) : new Color(0.7f, 0.7f, 0.7f);
        }

        // 3. Set Bonus Text
        if (bonusText != null)
        {
            bonusText.text = bonusString;
            // Etwas dezenter als der Name
            bonusText.color = isEquipped ? new Color(0.8f, 1f, 0.8f) : new Color(0.6f, 0.6f, 0.6f);
        }
    }
}