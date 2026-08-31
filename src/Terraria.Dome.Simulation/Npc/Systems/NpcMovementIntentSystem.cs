using System;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Movement.Components;
using NpcComponents = Terraria.Dome.Simulation.Npc.Components;
using ArchWorld = Arch.Core.World;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcMovementIntentSystem
{
  private readonly NpcBehaviorSystem _behaviorSystem = new();

  public void Apply(ArchWorld world, Entity npc, bool isDayTime)
  {
    ArgumentNullException.ThrowIfNull(world);
    if (!world.IsAlive(npc))
    {
      return;
    }

    NpcComponents.NpcBehaviorStateComponent behavior =
      world.Get<NpcComponents.NpcBehaviorStateComponent>(npc);
    NpcComponents.NpcTargetComponent target =
      world.Get<NpcComponents.NpcTargetComponent>(npc);
    TransformComponent npcTransform = world.Get<TransformComponent>(npc);
    NpcComponents.NpcTargetComponent effectiveTarget = target;
    SimulationVector targetPosition = default;
    if (target.HasTarget && world.IsAlive(target.Target))
    {
      TransformComponent targetTransform = world.Get<TransformComponent>(target.Target);
      targetPosition = new SimulationVector(targetTransform.X, targetTransform.Y);
    }
    else
    {
      effectiveTarget = new NpcComponents.NpcTargetComponent(
        default,
        0,
        NpcComponents.NpcTargetLockReason.NoValidTarget);
    }

    NpcBehaviorResult result = _behaviorSystem.Evaluate(
      behavior,
      effectiveTarget,
      new SimulationVector(npcTransform.X, npcTransform.Y),
      targetPosition,
      isDayTime);
    ref MovementIntentComponent intent = ref world.Get<MovementIntentComponent>(npc);
    intent.HasNpcIntent = true;
    intent.NpcHorizontalVelocity = result.Velocity.X;
    intent.NpcVerticalVelocity = result.Velocity.Y;
    intent.HorizontalDirection = result.Facing;
  }
}
