using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PressurePlatePlacementQuery
{
  private const int WorldEdgeFluff = 3;

  public static bool CanGenerateAt(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    PressurePlatePlacementDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!IsInsideWithFluff(snapshot, x, y))
    {
      return false;
    }

    int supportY = y + 1;
    if (!TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, supportY))
    {
      return false;
    }

    WorldTile supportTile = snapshot.GetTile(x, supportY);
    return !definition.IsBoulder && supportTile.WallType != definition.ForbiddenWallType;
  }

  private static bool IsInsideWithFluff(WorldGridSnapshot snapshot, int x, int y)
  {
    return x >= WorldEdgeFluff && x < snapshot.Metadata.Width - WorldEdgeFluff &&
      y >= WorldEdgeFluff && y < snapshot.Metadata.Height - WorldEdgeFluff;
  }
}
