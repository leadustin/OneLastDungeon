using UnityEngine;

[CreateAssetMenu(fileName = "New Currency Drop", menuName = "Card System/Currency")]
public class CurrencyData : CardData
{
    [Header("Minimaler Drop")]
    public int minGold;
    public int minSilver;
    public int minCopper;

    [Header("Maximaler Drop")]
    public int maxGold;
    public int maxSilver;
    public int maxCopper;

    // Hilfsmethoden für den GridManager
    public int GetMinInCopper() => (minGold * 10000) + (minSilver * 100) + minCopper;
    public int GetMaxInCopper() => (maxGold * 10000) + (maxSilver * 100) + maxCopper;

    protected void OnValidate()
    {
        cardType = CardType.Gold; // Stellt sicher, dass der Typ immer Gold ist
    }
}