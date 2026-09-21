using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcBehaviorInput(
  NpcBehaviorStateComponent State,
  NpcTargetComponent Target,
  SimulationVector NpcPosition,
  SimulationVector TargetPosition,
  bool IsDayTime);

public delegate NpcBehaviorResult NpcBehaviorHandler(NpcBehaviorInput input);

public sealed class NpcBehaviorRegistry
{
  private readonly IReadOnlyDictionary<NpcBehaviorId, NpcBehaviorHandler> _handlers;

  public NpcBehaviorRegistry(
    IReadOnlyDictionary<NpcBehaviorId, NpcBehaviorHandler> handlers)
  {
    ArgumentNullException.ThrowIfNull(handlers);
    if (handlers.Count == 0)
    {
      throw new ArgumentException("At least one NPC behavior handler is required.", nameof(handlers));
    }

    foreach ((NpcBehaviorId behaviorId, NpcBehaviorHandler handler) in handlers)
    {
      if (!Enum.IsDefined(behaviorId))
      {
        throw new ArgumentException(
          "The behavior registry contains an undefined behavior ID.",
          nameof(handlers));
      }

      ArgumentNullException.ThrowIfNull(handler);
    }

    _handlers = new Dictionary<NpcBehaviorId, NpcBehaviorHandler>(handlers);
  }

  public bool TryGet(NpcBehaviorId behaviorId, out NpcBehaviorHandler handler)
  {
    return _handlers.TryGetValue(behaviorId, out handler!);
  }

  public IReadOnlyCollection<NpcBehaviorId> RegisteredBehaviorIds =>
    _handlers.Keys.OrderBy(behaviorId => (int)behaviorId).ToArray();
}
