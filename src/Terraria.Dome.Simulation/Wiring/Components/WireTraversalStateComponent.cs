using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class WireTraversalStateComponent
{
  public int VisitedCount => _visited.Count;
  private readonly HashSet<(int X, int Y)> _visited = new();

  public void Clear()
  {
    _visited.Clear();
  }

  public bool TryVisit(int x, int y)
  {
    return _visited.Add((x, y));
  }
}
