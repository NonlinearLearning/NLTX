using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile3x3ValidationQuery
{
  private const int TileFrameWidth = 18;

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
    return tileType is 106 or 212 or 219 or 220 or 228 or 231 or 243 or 247 or 283 or
      (>= 300 and <= 308) or 354 or 355 or 406 or 412 or 452 or 455 or 491 or 642 or 733;
  }
}
