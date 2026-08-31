using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public sealed class LiquidSourceSystem
{
  public void AppendWorkItems(
    IReadOnlyCollection<LiquidSourceComponent> sources,
    WorldBoundsComponent bounds,
    ref WorldGenerationStateComponent state,
    List<LiquidWorkItemComponent> workItems)
  {
    ArgumentNullException.ThrowIfNull(sources);
    ArgumentNullException.ThrowIfNull(workItems);
    if (state.Stage < WorldGenerationStage.Liquid &&
        !state.TryAdvance(WorldGenerationStage.Liquid))
    {
      throw new InvalidOperationException("Liquid stage could not be started.");
    }

    foreach (LiquidSourceComponent source in sources)
    {
      if (!bounds.Contains(source.X, source.Y))
      {
        throw new ArgumentOutOfRangeException(
          nameof(sources),
          "Liquid source was outside the world.");
      }

      workItems.Add(new LiquidWorkItemComponent(
        source.X,
        source.Y,
        source.LiquidType,
        source.Amount,
        state.ReserveSequence(),
        source.Source));
    }
  }
}
