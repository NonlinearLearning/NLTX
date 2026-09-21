using System;

namespace Terraria.Dome.Simulation.Items.Systems;

public static class ItemPriceSystem
{
  public const int CopperValue = 1;
  public const int SilverValue = 100;
  public const int GoldValue = 10000;
  public const int PlatinumValue = 1000000;
  public const int SellPriceMultiplier = 5;
  public static readonly int ShadowOrbPrice = CalculateSellPrice(gold: 1, silver: 50);
  public static readonly int DungeonPrice = CalculateSellPrice(gold: 1, silver: 75);
  public static readonly int QueenBeePrice = CalculateSellPrice(gold: 2);
  public static readonly int HellPrice = CalculateSellPrice(gold: 2, silver: 50);
  public static readonly int EclipsePrice = CalculateSellPrice(gold: 7, silver: 50);
  public static readonly int EclipsePostPlanteraPrice = CalculateSellPrice(gold: 10);
  public static readonly int EclipseMothronPrice = CalculateSellPrice(gold: 12, silver: 50);

  public static int CalculateBuyPrice(
    int platinum = 0,
    int gold = 0,
    int silver = 0,
    int copper = 0)
  {
    ValidateCurrency(platinum, nameof(platinum));
    ValidateCurrency(gold, nameof(gold));
    ValidateCurrency(silver, nameof(silver));
    ValidateCurrency(copper, nameof(copper));

    try
    {
      return checked(
        platinum * PlatinumValue +
        gold * GoldValue +
        silver * SilverValue +
        copper * CopperValue);
    }
    catch (OverflowException)
    {
      throw new ArgumentOutOfRangeException(
        nameof(platinum),
        "The item price exceeds the supported currency range.");
    }
  }

  public static int CalculateSellPrice(
    int platinum = 0,
    int gold = 0,
    int silver = 0,
    int copper = 0)
  {
    int buyPrice = CalculateBuyPrice(platinum, gold, silver, copper);
    try
    {
      return checked(buyPrice * SellPriceMultiplier);
    }
    catch (OverflowException)
    {
      throw new ArgumentOutOfRangeException(
        nameof(platinum),
        "The item sell price exceeds the supported currency range.");
    }
  }

  public static int CalculateSellPriceFromItemValue(int valueCopper)
  {
    ValidateCurrency(valueCopper, nameof(valueCopper));
    return valueCopper / SellPriceMultiplier;
  }

  private static void ValidateCurrency(int value, string parameterName)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
  }
}

public static class ItemCurrencySystem
{
  public static bool IsCoinType(ushort itemType)
  {
    return itemType is >= 71 and <= 74;
  }
}
