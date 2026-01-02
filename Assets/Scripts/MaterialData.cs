using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Dungeon/Items/Material")]
public class MaterialData : CardData
{
    // HIER GELÖSCHT: public string description... 
    // Grund: Das Feld existiert jetzt in "CardData" und wird vererbt.

    protected void OnValidate()
    {
        cardType = CardType.Material;
    }
}