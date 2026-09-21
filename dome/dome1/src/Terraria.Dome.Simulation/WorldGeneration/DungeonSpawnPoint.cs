using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonSpawnPoint
{
  public DungeonSpawnPoint(int x, int y)
  {
    if (x < 0 || y < 0)
    {
      throw new ArgumentOutOfRangeException(x < 0 ? nameof(x) : nameof(y));
    }

    X = x;
    Y = y;
  }

  public static DungeonSpawnPoint Unset { get; } = new(-1, -1, true);

  public static DungeonSpawnPoint FromLegacy(int x, int y)
  {
    return x >= 0 && y >= 0 ? new DungeonSpawnPoint(x, y) : Unset;
  }

  public int X { get; }
  public int Y { get; }
  public bool IsSet => X >= 0 && Y >= 0;

  private DungeonSpawnPoint(int x, int y, bool allowUnset)
  {
    if (!allowUnset || x != -1 || y != -1)
    {
      throw new ArgumentOutOfRangeException(nameof(allowUnset));
    }

    X = x;
    Y = y;
  }
}
