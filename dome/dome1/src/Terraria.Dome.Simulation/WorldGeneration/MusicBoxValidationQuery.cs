using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MusicBoxValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int Width = 2;
  private const int Height = 2;

  public static MusicBoxValidationResult Evaluate(
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
    int styleBand = frameColumn / Width;
    int frameRow = source.FrameY / TileFrameWidth;
    int frameBand = frameRow / Height;
    int originX = x - frameColumn % Width;
    int originY = y - frameRow % Height;
    bool valid = true;
    for (int offsetX = 0; offsetX < Width; offsetX++)
    {
      for (int offsetY = 0; offsetY < Height; offsetY++)
      {
        int tileX = originX + offsetX;
        int tileY = originY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(tileX, tileY)
          ? snapshot.GetTile(tileX, tileY)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(styleBand * 36 + offsetX * TileFrameWidth)) &&
          tile.FrameY == checked((short)(frameBand * 36 + offsetY * TileFrameWidth));
      }
    }

    bool hasSupport = true;
    for (int offsetX = 0; offsetX < Width; offsetX++)
    {
      hasSupport &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + Height);
    }

    valid &= hasSupport;
    return new MusicBoxValidationResult(
      valid,
      !valid,
      originX,
      originY,
      styleBand,
      hasSupport);
  }
}
