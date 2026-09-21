using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile2x2ValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort BedTopSupportTileType = 95;
  private const ushort DresserTopSupportTileType = 126;
  private const ushort Type172TileType = 172;
  private const ushort BoulderTileType = 132;
  private const ushort Special652TileType = 652;
  private static readonly IReadOnlySet<ushort> DeferredSpecialCaseTileTypes = new HashSet<ushort>
  {
    BedTopSupportTileType, DresserTopSupportTileType, BoulderTileType, Special652TileType
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterDeferredSpecialCaseDefaults()
  {
    return DeferredSpecialCaseTileTypes;
  }

  public static Tile2x2ValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    TileDefinitionRegistry tileDefinitions,
    int x,
    int y,
    ushort tileType)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(tileDefinitions);
    if (!snapshot.Metadata.IsInside(x, y))
    {
      throw new ArgumentOutOfRangeException(nameof(x));
    }

    WorldTile source = snapshot.GetTile(x, y);
    int frameColumn = source.FrameX / TileFrameWidth;
    int frameOffsetX = 0;
    int originX = x - frameColumn;
    if (frameColumn > 1)
    {
      originX += 2;
      frameOffsetX = 36;
    }

    int frameHeight = tileType == Type172TileType ? 38 : 36;
    int frameRow = source.FrameY;
    int styleBand = 0;
    while (frameRow >= frameHeight)
    {
      frameRow -= frameHeight;
      styleBand++;
    }

    int originY = y - frameRow / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 2; offsetX++)
    {
      for (int offsetY = 0; offsetY < 2; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        if (!snapshot.Metadata.IsInside(tileX, tileY))
        {
          valid = false;
          continue;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(offsetX * TileFrameWidth + frameOffsetX)) &&
          tile.FrameY == checked((short)(offsetY * TileFrameWidth + styleBand * frameHeight));
      }

      bool support = tileType is BedTopSupportTileType or DresserTopSupportTileType
        ? TileStateQuery.IsSolidWithoutPlatforms(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY - 1)
        : TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY + 2);
      valid &= support;
    }

    bool deferredSpecialCase = DeferredSpecialCaseTileTypes.Contains(tileType);
    return new Tile2x2ValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      deferredSpecialCase);
  }
}
