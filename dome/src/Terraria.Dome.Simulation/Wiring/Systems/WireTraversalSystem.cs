using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Wiring.Commands;
using Terraria.Dome.Simulation.Wiring.Components;

namespace Terraria.Dome.Simulation.Wiring.Systems;

public sealed class WireTraversalSystem
{
  public WireTraversalResult Traverse(
    WireNetworkComponent network,
    WireTraversalStateComponent state,
    int startX,
    int startY,
    WireColor color,
    int maximumNodes)
  {
    ArgumentNullException.ThrowIfNull(network);
    ArgumentNullException.ThrowIfNull(state);
    if (!Enum.IsDefined(color) || maximumNodes <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumNodes));
    }

    state.Clear();
    List<WireTraversalNode> visited = new();
    Queue<(int X, int Y)> pending = new();
    if (network.HasWire(startX, startY, color))
    {
      pending.Enqueue((startX, startY));
    }

    while (pending.Count > 0 && visited.Count < maximumNodes)
    {
      (int x, int y) = pending.Dequeue();
      if (!network.HasWire(x, y, color) || !state.TryVisit(x, y))
      {
        continue;
      }

      visited.Add(new WireTraversalNode(visited.Count, x, y, color));
      pending.Enqueue((x, y - 1));
      pending.Enqueue((x - 1, y));
      pending.Enqueue((x + 1, y));
      pending.Enqueue((x, y + 1));
    }

    bool budgetExceeded = pending.Count > 0;
    if (budgetExceeded)
    {
      state.MarkBudgetExceeded();
    }

    return new WireTraversalResult(visited, budgetExceeded);
  }
}

public sealed record WireTraversalResult(
  IReadOnlyList<WireTraversalNode> Nodes,
  bool BudgetExceeded);
