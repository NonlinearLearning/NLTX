using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyErrorWorldTileRectangleSwap
{
  public static void AppendCommands(
    WorldGridSnapshot snapshot,
    int firstX,
    int firstY,
    int secondX,
    int secondY,
    int width,
    int height,
    ref WorldGenerationStateComponent state,
    List<TileChangeCommand> commands)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(commands);
    ValidateRectangle(snapshot, firstX, firstY, width, height);
    ValidateRectangle(snapshot, secondX, secondY, width, height);
    if (Overlaps(firstX, firstY, secondX, secondY, width, height))
    {
      throw new ArgumentException("ErrorWorld swap rectangles must not overlap.");
    }

    for (int offsetX = 0; offsetX < width; offsetX++)
    {
      for (int offsetY = 0; offsetY < height; offsetY++)
      {
        WorldTile first = snapshot.GetTile(firstX + offsetX, firstY + offsetY);
        WorldTile second = snapshot.GetTile(secondX + offsetX, secondY + offsetY);
        LegacyErrorWorldTileSwap.AppendCommands(
          firstX + offsetX,
          firstY + offsetY,
          secondX + offsetX,
          secondY + offsetY,
          first,
          second,
          ref state,
          commands);
      }
    }
  }

  private static bool Overlaps(int firstX, int firstY, int secondX, int secondY, int width, int height)
  {
    return firstX < secondX + width && secondX < firstX + width &&
      firstY < secondY + height && secondY < firstY + height;
  }

  private static void ValidateRectangle(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    int width,
    int height)
  {
    if (width <= 0 || height <= 0 || x < 0 || y < 0 ||
        x > snapshot.Metadata.Width - width || y > snapshot.Metadata.Height - height)
    {
      throw new ArgumentOutOfRangeException(nameof(width));
    }
  }
}
