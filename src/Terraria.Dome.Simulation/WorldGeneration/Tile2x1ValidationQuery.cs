using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile2x1ValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort PileTileType = 185;
  private static readonly IReadOnlySet<ushort> TableSupportTileTypes = new HashSet<ushort>
  {
    29, 103, 462
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterTableSupportDefaults()
  {
    return TableSupportTileTypes;
  }

  public static Tile2x1ValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    return Evaluate(
      snapshot,
      tileDefinitions,
      x,
      y,
      tileType,
      RoomNeedsTileRegistry.RegisterTableTileDefaults());
  }

  public static Tile2x1ValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType,
    IReadOnlySet<ushort> tableTileTypes)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    ArgumentNullException.ThrowIfNull(tableTileTypes);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int styleBand = frameColumn >> 1;
    int originX = x - (frameColumn % 2);
    int originY = y;

    if (tileType == PileTileType)
    {
      return new Tile2x1ValidationResult(false, false, true, originX, originY, styleBand);
    }

    bool valid = true;
    for (int offset = 0; offset < 2; offset++)
    {
      int tileX = originX + offset;
      WorldTile tile = snapshot.Metadata.IsInside(tileX, originY)
        ? snapshot.GetTile(tileX, originY)
        : default;
      valid &= tile.IsActive && tile.Type == tileType &&
        tile.FrameX == checked((short)(styleBand * 36 + offset * TileFrameWidth));

      WorldTile support = snapshot.Metadata.IsInside(tileX, originY + 1)
        ? snapshot.GetTile(tileX, originY + 1)
        : default;
      valid &= IsValidSupport(
        support,
        snapshot,
        tileDefinitions,
        tileX,
        originY + 1,
        tableTileTypes,
        tileType);
    }

    return new Tile2x1ValidationResult(valid, !valid, false, originX, originY, styleBand);
  }

  private static bool IsValidSupport(
    WorldTile support,
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    IReadOnlySet<ushort> tableTileTypes,
    ushort tileType)
  {
    if (TableSupportTileTypes.Contains(tileType))
    {
      return support.IsActive && tableTileTypes.Contains(support.Type) &&
        !support.IsHalfBrick && !IsTopSlope(support.Slope);
    }

    return TileStateQuery.IsSolidAllowingBottomSlope(snapshot, tileDefinitions, x, y);
  }

  private static bool IsTopSlope(byte slope)
  {
    return slope is 1 or 2;
  }
}
