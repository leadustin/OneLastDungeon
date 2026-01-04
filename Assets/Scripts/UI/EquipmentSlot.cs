using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Welcher Slot ist das?")]
    public EquipmentType allowedType;

    [Header("UI Referenzen")]
    public Image iconDisplay;
    public Sprite defaultIcon;

    private CardData currentItem;

    public void UpdateSlotUI(CardData item)
    {
        currentItem = item;

        if (currentItem != null)
        {
            iconDisplay.sprite = currentItem.artwork;
            iconDisplay.preserveAspect = true;
            iconDisplay.color = Color.white;
            iconDisplay.enabled = true;
        }
        else
        {
            if (defaultIcon != null)
            {
                iconDisplay.sprite = defaultIcon;
                iconDisplay.color = new Color(1, 1, 1, 0.2f);
                iconDisplay.enabled = true;
            }
            else
            {
                iconDisplay.enabled = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Klick auf Slot -> Detailfenster öffnen (zum Ausziehen)
        if (currentItem != null)
        {
            ItemComparisonUI.Instance.OpenEquippedItemDetail(currentItem);
        }
    }
}