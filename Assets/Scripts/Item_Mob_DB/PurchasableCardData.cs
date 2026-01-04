using UnityEngine;

public abstract class PurchasableCardData : CardData
{
    [Header("Shop Preis")]
    [Min(0)] public int priceGold;
    [Min(0)] public int priceSilver;
    [Min(0)] public int priceCopper;

    public int GetPriceInCopper()
    {
        return (priceGold * 10000) + (priceSilver * 100) + priceCopper;
    }

    public string GetPriceString()
    {
        if (priceGold > 0) return $"{priceGold}g {priceSilver}s {priceCopper}c";
        if (priceSilver > 0) return $"{priceSilver}s {priceCopper}c";
        return $"{priceCopper}c";
    }
}