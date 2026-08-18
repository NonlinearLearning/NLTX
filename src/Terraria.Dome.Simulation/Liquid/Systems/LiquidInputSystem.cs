using System;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.WorldModel;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidInputSystem
{
  public bool TryAccept(
    WorldGrid world,
    LiquidUpdateQueueComponent queue,
    LiquidWorldStateComponent state,
    PipelineLiquidSourceComponent source)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(queue);
    ArgumentNullException.ThrowIfNull(state);
    if (queue.MaximumLength > state.MaximumQueueLength || source.Sequence < 0 ||
        source.Amount == 0 || !Enum.IsDefined(source.Type) || !world.Contains(source.X, source.Y))
    {
      return false;
    }

    return queue.TryEnqueue(source);
  }
}
