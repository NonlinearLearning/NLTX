using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFrameImportant529Query
{
  private const ushort TileType = 529;

  public static TileFrameImportant529Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y)
  {
    return Evaluate(
      snapshot,
      tileDefinitions,
      x,
      y,
      ConversionSandTileRegistry.RegisterDefaults());
  }

  public static TileFrameImportant529Result Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlySet<ushort> conversionSandTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(conversionSandTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    if (!source.IsActive || source.Type != TileType)
    {
      return new TileFrameImportant529Result(false, false);
    }

    if (!TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          x,
          y + 1))
    {
      return new TileFrameImportant529Result(false, true);
    }

    WorldTile support = snapshot.Metadata.IsInside(x, y + 1)
      ? snapshot.GetTile(x, y + 1)
      : default;
    return support.IsActive && conversionSandTileTypes.Contains(support.Type)
      ? new TileFrameImportant529Result(true, false)
      : new TileFrameImportant529Result(false, true);
  }
}
