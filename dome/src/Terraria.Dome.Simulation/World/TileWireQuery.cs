using System;

namespace Terraria.Dome.Simulation.WorldModel;

public static class TileWireQuery
{
  public static bool HasAnyWireNearby(
    WorldGridSnapshot snapshot,
    int sourceX,
    int sourceY,
    int boxSpread)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentOutOfRangeException.ThrowIfNegative(boxSpread);

    int minimumX = ClampToWorld(sourceX - (long)boxSpread, snapshot.Metadata.Width);
    int maximumX = ClampToWorld(sourceX + (long)boxSpread, snapshot.Metadata.Width);
    int minimumY = ClampToWorld(sourceY - (long)boxSpread, snapshot.Metadata.Height);
    int maximumY = ClampToWorld(sourceY + (long)boxSpread, snapshot.Metadata.Height);
    for (int x = minimumX; x <= maximumX; x++)
    {
      for (int y = minimumY; y <= maximumY; y++)
      {
        WorldTile tile = snapshot.GetTile(x, y);
        if (tile.HasWire || tile.HasWire2 || tile.HasWire3 || tile.HasWire4)
        {
          return true;
        }
      }
    }

    return false;
  }

  private static int ClampToWorld(long coordinate, int length)
  {
    return (int)Math.Clamp(coordinate, 0, length - 1L);
  }
}
