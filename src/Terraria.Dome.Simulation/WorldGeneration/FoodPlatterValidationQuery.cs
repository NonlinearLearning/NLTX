using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class FoodPlatterValidationQuery
{
  public static FoodPlatterValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    bool hasBottomSupport = snapshot.Metadata.IsInside(x, y + 1) &&
      TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y + 1);
    return new FoodPlatterValidationResult(hasBottomSupport, !hasBottomSupport, hasBottomSupport);
  }
}
