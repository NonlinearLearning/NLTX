using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Protocol.V1456.Isolation;

public sealed class NetworkTileSectionSlice
{
  private NetworkTileSectionSlice(
    WorldSectionCoordinates coordinates,
    int width,
    int height,
    long version,
    IReadOnlyList<WorldTile> tiles)
  {
    Coordinates = coordinates;
    Width = width;
    Height = height;
    Version = version;
    Tiles = tiles;
  }

  public WorldSectionCoordinates Coordinates { get; }

  public int Width { get; }

  public int Height { get; }

  public long Version { get; }

  public IReadOnlyList<WorldTile> Tiles { get; }

  public static NetworkTileSectionSlice From(WorldSectionSnapshot snapshot)
  {
    WorldTile[] tiles = new WorldTile[snapshot.Width * snapshot.Height];
    int index = 0;
    for (int y = 0; y < snapshot.Height; y++)
    {
      for (int x = 0; x < snapshot.Width; x++)
      {
        tiles[index] = snapshot.GetTile(x, y);
        index++;
      }
    }

    return new NetworkTileSectionSlice(
      snapshot.Coordinates,
      snapshot.Width,
      snapshot.Height,
      snapshot.Version,
      tiles);
  }
}
