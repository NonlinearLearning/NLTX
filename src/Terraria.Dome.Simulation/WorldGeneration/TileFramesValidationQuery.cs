using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileFramesValidationQuery
{
  public static TileFramesValidationResult Evaluate(
    WorldGridSnapshot snapshot,
    int tileType,
    int startX,
    int startY,
    int width,
    int height,
    int styleX,
    int frameXIncrement,
    int styleY,
    int frameYIncrement)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
    bool valid = true;
    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        int x = startX + offsetX;
        int y = startY + offsetY;
        WorldTile tile = snapshot.Metadata.IsInside(x, y)
          ? snapshot.GetTile(x, y)
          : default;
        valid &= tile.IsActive && tile.Type == tileType &&
          tile.FrameX == checked((short)(styleX * width * frameXIncrement +
            offsetX * frameXIncrement)) &&
          tile.FrameY == checked((short)(styleY * height * frameYIncrement +
            offsetY * frameYIncrement));
      }
    }

    return new TileFramesValidationResult(valid, !valid, startX, startY, width, height);
  }
}
