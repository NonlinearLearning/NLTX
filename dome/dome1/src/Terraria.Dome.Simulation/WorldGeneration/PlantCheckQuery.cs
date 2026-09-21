using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class PlantCheckQuery
{
  private const ushort SandShaleSaplingTileType = 703;

  public static PlantCheckResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);

    x = Math.Clamp(x, 1, snapshot.Metadata.Width - 2);
    y = Math.Clamp(y, 1, snapshot.Metadata.Height - 2);

    WorldTile plantTile = snapshot.GetTile(x, y);
    if (plantTile.Type == SandShaleSaplingTileType)
    {
      bool shouldDestroy = !TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        x,
        y + 1);
      return new PlantCheckResult(
        shouldDestroy,
        false,
        new PlantTypeConversionResult(plantTile.Type, plantTile.FrameX, false));
    }

    int supportTileType = GetSupportTileType(snapshot, x, y, plantTile.Type);
    if (!PlantTypeConversionQuery.IsBadTypeMatch(supportTileType, plantTile.Type))
    {
      return new PlantCheckResult(
        false,
        false,
        new PlantTypeConversionResult(plantTile.Type, plantTile.FrameX, false));
    }

    PlantTypeConversionResult conversion = PlantTypeConversionQuery.Evaluate(
      plantTile.Type,
      plantTile.FrameX,
      supportTileType);
    return new PlantCheckResult(
      conversion.TileType == plantTile.Type,
      conversion.TileType != plantTile.Type,
      conversion);
  }

  private static int GetSupportTileType(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    ushort plantTileType)
  {
    int supportY = y + 1;
    if (!snapshot.Metadata.IsInside(x, supportY))
    {
      return plantTileType;
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
