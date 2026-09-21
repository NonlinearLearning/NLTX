namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengeCachePolicy
{
  public const int MinimumCoins = 1000;

  public static bool CanCache(int coinValue)
  {
    return coinValue >= MinimumCoins;
  }
}
