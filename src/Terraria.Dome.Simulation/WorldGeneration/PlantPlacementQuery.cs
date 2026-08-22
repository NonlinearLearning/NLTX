using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PlantPlacementQuery
{
  private const ushort SandShaleSaplingTileType = 703;

  public static bool CanPlace(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    int tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    int supportY = y + 1;
    if (tileType == SandShaleSaplingTileType)
    {
      return TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        x,
        supportY);
    }

    int supportTileType = GetSupportTileType(snapshot, x, supportY, tileType);
    return !PlantTypeConversionQuery.IsBadTypeMatch(supportTileType, tileType);
  }

  private static int GetSupportTileType(
    WorldGridSnapshot snapshot,
    int x,
    int supportY,
    int tileType)
  {
    if (!snapshot.Metadata.IsInside(x, supportY))
    {
      return tileType;
    }

    WorldTile supportTile = snapshot.GetTile(x, supportY);
    if (!supportTile.IsActive || supportTile.IsInactive || supportTile.IsHalfBrick ||
        supportTile.Slope != 0)
    {
      return -1;
    }

    return supportTile.Type;
  }
}
