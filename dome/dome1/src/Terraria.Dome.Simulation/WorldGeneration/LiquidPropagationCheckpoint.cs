using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration;

public sealed class LiquidPropagationCheckpoint
{
  public LiquidPropagationCheckpoint(
    WorldGridSnapshot snapshot,
    WorldGenerationStateComponent state,
    IReadOnlyCollection<LiquidWorkItemComponent> pendingWorkItems,
    IReadOnlyCollection<LiquidPropagationVisit> completedVisits)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingWorkItems);
    ArgumentNullException.ThrowIfNull(completedVisits);
    Snapshot = snapshot;
    State = state;
    LiquidWorkItemComponent[] copiedWorkItems = new List<LiquidWorkItemComponent>(pendingWorkItems)
      .ToArray();
    PendingWorkItems = Array.AsReadOnly(copiedWorkItems);
    LiquidPropagationVisit[] copiedVisits = new List<LiquidPropagationVisit>(completedVisits)
      .ToArray();
    CompletedVisits = Array.AsReadOnly(copiedVisits);
  }

  public IReadOnlyList<LiquidPropagationVisit> CompletedVisits { get; }

  public IReadOnlyList<LiquidWorkItemComponent> PendingWorkItems { get; }

  public WorldGridSnapshot Snapshot { get; }

  public WorldGenerationStateComponent State { get; }

  public WorldGrid RestoreWorld()
  {
    return WorldGrid.FromSnapshot(Snapshot);
  }
}
