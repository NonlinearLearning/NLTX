using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class JungleChestItemSelectionQuery
{
  public static JungleChestItemSelectionResult Evaluate(int jungleItemCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(jungleItemCount);
    int baseItemType = (jungleItemCount % 4) switch
    {
      0 => 211,
      1 => 212,
      2 => 213,
      _ => 964
    };
    return new JungleChestItemSelectionResult(
      baseItemType,
      jungleItemCount,
      checked(jungleItemCount + 1),
      true,
      true);
  }
}
