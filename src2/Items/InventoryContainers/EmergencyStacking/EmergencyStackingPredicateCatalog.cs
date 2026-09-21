namespace Terraria.Items.InventoryContainers;

public static class EmergencyStackingPredicateCatalog
{
  public const string RareCurrency = "RareCurrency";

  public const string Equipment = "Equipment";

  public const string SilverCoins = "SilverCoins";

  public const string CopperCoins = "CopperCoins";

  public const string FallenStars = "FallenStars";

  public const string Default = "Default";

  public static bool IsKnown(string predicateKey)
  {
    return predicateKey is RareCurrency
      or Equipment
      or SilverCoins
      or CopperCoins
      or FallenStars
      or Default;
  }

  public static bool Matches(string predicateKey, ItemStackSnapshot item)
  {
    ArgumentNullException.ThrowIfNull(item);
    return predicateKey switch
    {
      RareCurrency => item.ContentType is 73 or 74 or 3822,
      Equipment => item.UniqueStack,
      SilverCoins => item.ContentType == 72,
      CopperCoins => item.ContentType == 71,
      FallenStars => item.ContentType == 75,
      Default => true,
      _ => throw new ArgumentException("Unknown emergency-stack predicate.", nameof(predicateKey))
    };
  }
}
