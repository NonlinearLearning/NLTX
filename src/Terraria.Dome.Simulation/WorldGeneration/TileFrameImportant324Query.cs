using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameImportant324Query
{
  private const ushort TileType = 324;

  public static TileFrameImportant324Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    return Evaluate(snapshot, tileDefinitions, x, y, BoulderTileRegistry.RegisterDefaults());
  }

  public static TileFrameImportant324Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlySet<ushort> boulderTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(boulderTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive || source.Type != TileType)
    {
      return new TileFrameImportant324Result(false, false);
    }

    int supportY = y + 1;
    if (!TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          x,
          supportY))
    {
      return new TileFrameImportant324Result(false, true);
    }

    if (snapshot.Metadata.IsInside(x, supportY) &&
        boulderTileTypes.Contains(snapshot.GetTile(x, supportY).Type) &&
        snapshot.GetTile(x, supportY).IsActive)
    {
      return new TileFrameImportant324Result(false, true);
    }

    return new TileFrameImportant324Result(true, false);
  }
}
