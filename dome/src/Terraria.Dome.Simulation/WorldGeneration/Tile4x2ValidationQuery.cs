using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class Tile4x2ValidationQuery
{
  private const int TileFrameWidth = 18;
  private const ushort PicnicTableTileType = 487;
  private const ushort BedTileType = 79;
  private const ushort BathtubTileType = 90;

  public static Tile4x2ValidationResult Evaluate(
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
    int originX = tileType == PicnicTableTileType
      ? x - frameColumn % 4
      : x - frameColumn;
    if (tileType is BedTileType or BathtubTileType && source.FrameX >= 72)
    {
      originX += 4;
    }

    int frameRow = source.FrameY / TileFrameWidth;
    int styleBand = 0;
    while (frameRow > 1)
    {
      frameRow -= 2;
      styleBand++;
    }

    int originY = y - frameRow;
    if (tileType == PicnicTableTileType)
    {
      styleBand = source.FrameX / 72;
    }

    bool valid = true;
    for (int offsetX = 0; offsetX < 4; offsetX++)
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

        int expectedFrameX = offsetX * TileFrameWidth;
        int expectedFrameY = offsetY * TileFrameWidth;
        if (tileType is BedTileType or BathtubTileType && source.FrameX >= 72)
        {
          expectedFrameX = (offsetX + 4) * TileFrameWidth;
        }

        if (tileType == PicnicTableTileType)
        {
          expectedFrameX += styleBand * 72;
        }
        else
        {
          expectedFrameY += styleBand * 36;
        }

        WorldTile tile = snapshot.GetTile(tileX, tileY);
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)expectedFrameX) &&
          tile.FrameY == checked((short)expectedFrameY);
      }

      bool supported = TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + 2);
      if (snapshot.Metadata.IsInside(originX + offsetX, originY + 2))
      {
        WorldTile support = snapshot.GetTile(originX + offsetX, originY + 2);
        if (tileDefinitions.TryGet(support.Type, out TileDefinition definition))
        {
          supported |= support.IsActive && definition.IsPlatform;
        }
      }

      valid &= supported;
    }

    return new Tile4x2ValidationResult(valid, !valid, originX, originY, styleBand);
  }
}
