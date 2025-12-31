using UnityEngine;

[CreateAssetMenu(fileName = "NewMaterial", menuName = "Dungeon/Items/Material")]
public class MaterialData : CardData
{
    [TextArea] public string description = "Handwerksmaterial.";
    protected void OnValidate() { cardType = CardType.Material; }
}