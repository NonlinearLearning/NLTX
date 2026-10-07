using System.Collections.Generic;
using System.Linq;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class LegacyTileMutationTracker
{
  private readonly HashSet<TileCoordinate> _changedCoordinates = new();

  public void MarkChanged(int x, int y)
  {
    _changedCoordinates.Add(new TileCoordinate(x, y));
  }

  public TileCoordinate[] Drain()
  {
    TileCoordinate[] snapshot = _changedCoordinates
      .OrderBy(static coordinate => coordinate.Y)
      .ThenBy(static coordinate => coordinate.X)
      .ToArray();
    _changedCoordinates.Clear();
    return snapshot;
  }
}
