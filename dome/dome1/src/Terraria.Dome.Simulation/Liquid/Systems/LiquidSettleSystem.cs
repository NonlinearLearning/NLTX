using System;
using Terraria.Dome.Simulation.Liquid.Components;

namespace Terraria.Dome.Simulation.Liquid.Systems;

public sealed class LiquidSettleSystem
{
  private const int MaximumRetries = 3;

  public bool Requeue(LiquidUpdateQueueComponent queue, LiquidUpdateNode node)
  {
    ArgumentNullException.ThrowIfNull(queue);
    return queue.TryRequeue(node, MaximumRetries);
  }
}
