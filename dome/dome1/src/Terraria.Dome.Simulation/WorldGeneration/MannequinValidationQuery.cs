using System;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MannequinValidationQuery
{
  private const int TileFrameWidth = 18;
  private const int Width = 2;
  private const int Height = 3;

  public static MannequinValidationResult Evaluate(
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
    int normalizedFrameX = NormalizeFrameX(source.FrameX);
    int originX = x - normalizedFrameX / TileFrameWidth;
    int originY = y - source.FrameY / TileFrameWidth;
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
          NormalizeFrameX(tile.FrameX) == offsetX * TileFrameWidth &&
          tile.FrameY == offsetY * TileFrameWidth;
      }
    }

    for (int offsetX = 0; offsetX < Width; offsetX++)
    {
      valid &= TileStateQuery.IsSolidAllowingBottomSlope(
        snapshot,
        tileDefinitions,
        originX + offsetX,
        originY + Height);
    }

    return new MannequinValidationResult(valid, !valid, originX, originY, tileType);
  }

  private static int NormalizeFrameX(short frameX)
  {
    int normalized = frameX;
    while (normalized >= 100)
    {
      normalized -= 100;
    }

    if (normalized >= 36)
    {
      normalized -= 36;
    }

    return normalized;
  }
}
