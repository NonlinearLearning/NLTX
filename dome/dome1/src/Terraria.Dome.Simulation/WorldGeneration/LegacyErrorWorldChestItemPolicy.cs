using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldChestItemPolicy
{
  private const int BalancedChestItemType = -1;

  public static int SelectItemType(bool balancedChests, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (balancedChests)
    {
      return BalancedChestItemType;
    }

    return random.Next(32) switch
    {
      0 => 4008,
      1 => 238,
      2 => 2275,
      3 => 3352,
      4 => 3262,
      5 => 3334,
      6 => 4818,
      7 => 1325,
      8 => 4144,
      9 => 3350,
      10 => 4347,
      11 => 1309,
      12 => 1863,
      13 => 485,
      14 => 748,
      15 => 1825,
      16 => 1321,
      17 => 5451,
      18 => 3385,
      19 => 3386,
      20 => 3387,
      21 => 3388,
      22 => 4951,
      24 => 3043,
      25 => 2341,
      26 => 2342,
      27 => 2800,
      28 => 3623,
      29 => 4980,
      30 => 4273,
      31 => 4711,
      _ => 4420
    };
  }
}
