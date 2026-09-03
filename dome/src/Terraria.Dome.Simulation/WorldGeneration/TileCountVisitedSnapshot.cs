using System;
using System.Collections.Generic;
using System.Collections.Frozen;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class TileCountVisitedSnapshot
{
  public TileCountVisitedSnapshot(IReadOnlySet<(int X, int Y)> coordinates)
  {
    ArgumentNullException.ThrowIfNull(coordinates);
    Coordinates = coordinates.ToFrozenSet();
  }

  public IReadOnlySet<(int X, int Y)> Coordinates { get; }

  public bool Contains(int x, int y)
  {
    return Coordinates.Contains((x, y));
  }
}
