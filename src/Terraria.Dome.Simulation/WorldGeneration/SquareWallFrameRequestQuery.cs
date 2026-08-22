using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class SquareWallFrameRequestQuery
{
  public static IReadOnlyList<WallFrameCoordinate> CreateCoordinates(
    WorldGridSnapshot snapshot,
    int x,
    int y)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<WallFrameCoordinate> coordinates = new(9);
    for (int offsetY = -1; offsetY <= 1; offsetY++)
    {
      for (int offsetX = -1; offsetX <= 1; offsetX++)
      {
        int coordinateX = x + offsetX;
        int coordinateY = y + offsetY;
        if (snapshot.Metadata.IsInside(coordinateX, coordinateY))
        {
          coordinates.Add(new WallFrameCoordinate(coordinateX, coordinateY));
        }
      }
    }

    return coordinates;
  }
}
