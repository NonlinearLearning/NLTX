using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyUnderworldAshBrickPolicy
{
  public static bool IsOuterColumn(int x, int width)
  {
    return x >= 25 && x < width - 25 && (x < width * 0.17 || x > width * 0.83);
  }

  public static bool ShouldConvert(
    WorldGridSnapshot snapshot,
    int x,
    int y,
    bool isRemixWorld)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (isRemixWorld || y < 1 || y >= snapshot.Metadata.Height - 1 ||
        !IsOuterColumn(x, snapshot.Metadata.Width))
    {
      return false;
    }

    WorldTile tile = snapshot.GetTile(x, y);
    if (!tile.IsActive || tile.Type != 57)
    {
      return false;
    }

    for (int offsetX = -1; offsetX <= 1; offsetX++)
    {
      for (int offsetY = -1; offsetY <= 1; offsetY++)
      {
        if ((offsetX != 0 || offsetY != 0) &&
            !snapshot.GetTile(x + offsetX, y + offsetY).IsActive)
        {
          return true;
        }
      }
    }

    return false;
  }
}
