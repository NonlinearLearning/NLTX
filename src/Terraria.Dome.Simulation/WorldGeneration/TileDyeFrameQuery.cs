using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileDyeFrameQuery
{
  private const ushort CactusTileType = 80;

  public static TileDyeFrameResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    short frameX)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive)
    {
      return new TileDyeFrameResult(false, false);
    }

    int frameBand = frameX / 34;
    if (frameBand == 7)
    {
      return CreateResult(
        TileStateQuery.IsSolid(
          snapshot,
          tileDefinitions,
          x,
          y - 1));
    }

    if (frameBand == 6)
    {
      WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
        ? snapshot.GetTile(x, y + 1)
        : default;
      return CreateResult(support.IsActive && support.Type == CactusTileType);
    }

    return CreateResult(
      TileStateQuery.IsSolid(
        snapshot,
        tileDefinitions,
        x,
        y + 1));
  }

  private static TileDyeFrameResult CreateResult(bool supported)
  {
    return supported
      ? new TileDyeFrameResult(true, false)
      : new TileDyeFrameResult(false, true);
  }
}
