using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class HousingTestBoundsQuery
{
  public static HousingTestBounds Calculate(
    int roomStartX,
    int roomEndX,
    int roomStartY,
    int roomEndY,
    int worldWidth,
    int worldHeight)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldWidth);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(worldHeight);
    int startX = Math.Max(roomStartX - 46, 5);
    int endX = Math.Min(roomEndX + 46, worldWidth - 6);
    int startY = Math.Max(roomStartY - 44, 5);
    int endY = Math.Min(roomEndY + 44, worldHeight - 6);
    return new HousingTestBounds(startX, endX, startY, endY);
  }
}
