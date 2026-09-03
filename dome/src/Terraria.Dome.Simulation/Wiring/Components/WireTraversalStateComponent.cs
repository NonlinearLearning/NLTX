using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class WireTraversalStateComponent
{
  public int VisitedCount => _visited.Count;
  private readonly HashSet<(int X, int Y)> _visited = new();
  public bool BudgetExceeded { get; private set; }

  public void Clear()
  {
    _visited.Clear();
    BudgetExceeded = false;
  }

  public void MarkBudgetExceeded()
  {
    BudgetExceeded = true;
  }

  public bool TryVisit(int x, int y)
  {
    return _visited.Add((x, y));
  }
}
