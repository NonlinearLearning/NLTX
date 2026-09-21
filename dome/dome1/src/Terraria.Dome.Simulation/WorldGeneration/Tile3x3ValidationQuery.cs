using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile3x3ValidationQuery
{
  private const int TileFrameWidth = 18;
  private static readonly IReadOnlySet<ushort> BottomSupportTileTypes = new HashSet<ushort>
  {
    106, 212, 219, 220, 228, 231, 243, 247, 283,
    300, 301, 302, 303, 304, 305, 306, 307, 308,
    354, 355, 406, 412, 452, 455, 491, 642, 733
  }.ToFrozenSet();

  public static IReadOnlySet<ushort> RegisterBottomSupportDefaults()
  {
    return BottomSupportTileTypes;
  }

  public static Tile3x3ValidationResult Evaluate(
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
    int styleBand = frameColumn / 3;
    int originX = x - frameColumn % 3;
    int frameBand = source.FrameY / 54;
    int originY = y - (source.FrameY % 54) / TileFrameWidth;
    bool valid = true;
    for (int offsetX = 0; offsetX < 3; offsetX++)
    {
      for (int offsetY = 0; offsetY < 3; offsetY++)
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
          tile.FrameX == checked((short)(styleBand * 54 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameBand * 54 + offsetY * TileFrameWidth));
      }
    }

    bool usesBottomSupport = UsesBottomSupport(tileType);
    if (usesBottomSupport)
    {
      for (int offsetX = 0; offsetX < 3; offsetX++)
      {
        valid &= TileStateQuery.IsSolidAllowingBottomSlope(
          snapshot,
          tileDefinitions,
          originX + offsetX,
          originY + 3);
      }
    }
    else
    {
      WorldTile support = snapshot.Metadata.IsInside(originX + 1, originY - 1)
        ? snapshot.GetTile(originX + 1, originY - 1)
        : default;
      valid &= support.IsActive && TileStateQuery.IsSolidWithoutPlatforms(
        support,
        tileDefinitions);
    }

    return new Tile3x3ValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      usesBottomSupport);
  }

  private static bool UsesBottomSupport(ushort tileType)
  {
    return BottomSupportTileTypes.Contains(tileType);
  }
}
