using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class WorldGenerationValidationSystem
{
  public WorldGenerationValidationResult Validate(
    WorldGridSnapshot snapshot,
    ref WorldGenerationStateComponent state,
    IReadOnlyCollection<LiquidWorkItemComponent> pendingWorkItems,
    IReadOnlyCollection<StructurePlacementComponent> placements)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    ArgumentNullException.ThrowIfNull(pendingWorkItems);
    ArgumentNullException.ThrowIfNull(placements);
    if (state.Stage < WorldGenerationStage.Committed)
    {
      return WorldGenerationValidationResult.Failed(
        "World generation cannot validate before commit.");
    }

    foreach (LiquidWorkItemComponent workItem in pendingWorkItems)
    {
      if (!snapshot.Metadata.IsInside(workItem.X, workItem.Y) || workItem.Amount == 0)
      {
        return WorldGenerationValidationResult.Failed(
          "A pending liquid work item was outside bounds or empty.");
      }
    }

    if (pendingWorkItems.Count != 0)
    {
      return WorldGenerationValidationResult.Failed(
        "World generation liquid propagation had pending work at validation.");
    }

    foreach (StructurePlacementComponent placement in placements)
    {
      for (int y = 0; y < placement.Footprint.Height; y++)
      {
        for (int x = 0; x < placement.Footprint.Width; x++)
        {
          if (!snapshot.Metadata.IsInside(placement.OriginX + x, placement.OriginY + y))
          {
            return WorldGenerationValidationResult.Failed(
              "A structure placement was outside world bounds.");
          }
        }
      }
    }

    if (!state.TryAdvance(WorldGenerationStage.Validated))
    {
      return WorldGenerationValidationResult.Failed(
        "World generation validation stage could not be completed.");
    }

    return WorldGenerationValidationResult.Success();
  }
}

public sealed record WorldGenerationValidationResult(bool Succeeded, string? FailureReason)
{
  public static WorldGenerationValidationResult Success()
  {
    return new WorldGenerationValidationResult(true, null);
  }

  public static WorldGenerationValidationResult Failed(string reason)
  {
    return new WorldGenerationValidationResult(false, reason);
  }
}
