using System;

namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengeExpirationPolicy
{
  public const int CopperThreshold = 1;
  public const int SilverThreshold = 100;
  public const int GoldThreshold = 10000;
  public const int PlatinumThreshold = 1000000;
  public const int OneMinuteTicks = 3600;
  public const int AdditionalLifetimeTicks = 18000;

  public static int CalculateExpirationTick(int currentGameTick, int coinValue)
  {
    if (currentGameTick < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(currentGameTick),
        currentGameTick,
        "Game tick must be non-negative.");
    }

    int lifetimeTicks = CalculateLifetimeTicks(coinValue);
    return checked(currentGameTick + lifetimeTicks);
  }

  public static int CalculateLifetimeTicks(int coinValue)
  {
    double lifetimeTicks;
    if (coinValue < SilverThreshold)
    {
      lifetimeTicks = Lerp(
        0.0,
        OneMinuteTicks,
        GetLerpValue(CopperThreshold, SilverThreshold, coinValue));
    }
    else if (coinValue < GoldThreshold)
    {
      lifetimeTicks = Lerp(
        36000.0,
        108000.0,
        GetLerpValue(SilverThreshold, GoldThreshold, coinValue));
    }
    else if (coinValue >= PlatinumThreshold)
    {
      lifetimeTicks = 432000.0;
    }
    else
    {
      lifetimeTicks = Lerp(
        108000.0,
        216000.0,
        GetLerpValue(SilverThreshold, GoldThreshold, coinValue));
    }

    return checked((int)lifetimeTicks + AdditionalLifetimeTicks);
  }

  private static double GetLerpValue(int from, int to, int value)
  {
    if (value <= from)
    {
      return 0.0;
    }

    if (value >= to)
    {
      return 1.0;
    }

    return (double)(value - from) / (to - from);
  }

  private static double Lerp(double from, double to, double amount)
  {
    return from + (to - from) * amount;
  }
}
