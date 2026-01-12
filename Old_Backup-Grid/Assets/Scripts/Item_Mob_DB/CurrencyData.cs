using UnityEngine;

[CreateAssetMenu(fileName = "NewCurrency", menuName = "Game Data/Items/Currency")]
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

    public long GetMinInCopper() => ((long)minGold * 10000) + ((long)minSilver * 100) + minCopper;
    public long GetMaxInCopper() => ((long)maxGold * 10000) + ((long)maxSilver * 100) + maxCopper;

    protected void OnValidate()
    {
        cardType = CardType.Gold;
    }
}