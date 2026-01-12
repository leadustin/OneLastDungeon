using UnityEngine;
using TMPro;

public class CharacterScreenUI : MonoBehaviour
{
    [Header("Text Anzeigen")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI manaText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI xpText;

    [Header("Links: Rüstung")]
    public EquipmentSlot headSlot;
    public EquipmentSlot chestSlot;
    public EquipmentSlot handsSlot;
    public EquipmentSlot legsSlot;
    public EquipmentSlot feetSlot;

    [Header("Rechts: Schmuck & Waffen")]
    public EquipmentSlot neckSlot;
    public EquipmentSlot ring1Slot;
    public EquipmentSlot ring2Slot;
    public EquipmentSlot weaponSlot;
    public EquipmentSlot offhandSlot;

    void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (PlayerManager.Instance == null) return;

        if (levelText != null) levelText.text = "Lvl " + PlayerManager.Instance.level;

        if (healthText != null) healthText.text = $"{PlayerManager.Instance.currentHealth} / {PlayerManager.Instance.maxHealth}";
        if (manaText != null) manaText.text = $"{PlayerManager.Instance.currentMana} / {PlayerManager.Instance.maxMana}";

        // NEU: Zeige Min - Max Schaden an
        if (damageText != null)
        {
            damageText.text = $"DMG: {PlayerManager.Instance.minAttackDamage} - {PlayerManager.Instance.maxAttackDamage}";
        }

        if (defenseText != null) defenseText.text = "DEF: " + PlayerManager.Instance.defense;

        // Slots updaten
        if (headSlot != null) headSlot.UpdateSlotUI(PlayerManager.Instance.headItem);
        if (chestSlot != null) chestSlot.UpdateSlotUI(PlayerManager.Instance.chestItem);
        if (handsSlot != null) handsSlot.UpdateSlotUI(PlayerManager.Instance.handsItem);
        if (legsSlot != null) legsSlot.UpdateSlotUI(PlayerManager.Instance.legsItem);
        if (feetSlot != null) feetSlot.UpdateSlotUI(PlayerManager.Instance.feetItem);

        if (neckSlot != null) neckSlot.UpdateSlotUI(PlayerManager.Instance.neckItem);
        if (ring1Slot != null) ring1Slot.UpdateSlotUI(PlayerManager.Instance.ring1Item);
        if (ring2Slot != null) ring2Slot.UpdateSlotUI(PlayerManager.Instance.ring2Item);
        if (weaponSlot != null) weaponSlot.UpdateSlotUI(PlayerManager.Instance.weaponItem);
        if (offhandSlot != null) offhandSlot.UpdateSlotUI(PlayerManager.Instance.offhandItem);
    }

    public void CloseWindow()
    {
        // Falls du einen UIManager hast:
        // if (UIManager.Instance != null) UIManager.Instance.ToggleCharacterScreen();
        // else 
        gameObject.SetActive(false);
    }
}