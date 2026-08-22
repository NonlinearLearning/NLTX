using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class RangeFrameCoordinateQuery
{
  public static IReadOnlyList<WallFrameCoordinate> CreateCoordinates(
    WorldGridSnapshot snapshot,
    int startX,
    int startY,
    int endX,
    int endY)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    if (startX > endX || startY > endY)
    {
      throw new ArgumentException("Range frame start must not exceed its end.");
    }

    List<WallFrameCoordinate> coordinates = new();
    for (int x = startX - 1; x <= endX + 1; x++)
    {
      for (int y = startY - 1; y <= endY + 1; y++)
      {
        if (snapshot.Metadata.IsInside(x, y))
        {
          coordinates.Add(new WallFrameCoordinate(x, y));
        }
      }
    }

    return coordinates;
  }
}
