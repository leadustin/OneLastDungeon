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

    // Rückgabetyp auf long geändert und Cast hinzugefügt
    public long GetMinInCopper() => ((long)minGold * 10000) + ((long)minSilver * 100) + minCopper;
    public long GetMaxInCopper() => ((long)maxGold * 10000) + ((long)maxSilver * 100) + maxCopper;

    protected void OnValidate()
    {
        cardType = CardType.Gold;
    }
}