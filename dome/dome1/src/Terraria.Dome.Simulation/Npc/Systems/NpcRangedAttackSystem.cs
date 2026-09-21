using System;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcRangedAttackSystem
{
  public bool TryProduce(
    NpcHandle sourceNpc,
    ref NpcRangedAttackState state,
    SimulationVector sourcePosition,
    SimulationVector targetPosition,
    bool hasTarget,
    bool hasLineOfSight,
    long sequence,
    out NpcRangedAttackCommand command)
  {
    command = default;
    if (!sourceNpc.IsValid || sequence < 0 || !IsFinite(sourcePosition) ||
        !IsFinite(targetPosition) || state.ProjectileType <= 0 || state.Damage <= 0 ||
        !float.IsFinite(state.ProjectileSpeed) || state.ProjectileSpeed <= 0.0f ||
        state.CooldownTicks < 0 || state.RemainingCooldownTicks < 0)
    {
      return false;
    }

    if (state.RemainingCooldownTicks > 0)
    {
      state.RemainingCooldownTicks--;
      return false;
    }

    if (!hasTarget || !hasLineOfSight)
    {
      return false;
    }

    float deltaX = targetPosition.X - sourcePosition.X;
    float deltaY = targetPosition.Y - sourcePosition.Y;
    float length = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);
    if (!float.IsFinite(length) || length <= 0.0f)
    {
      return false;
    }

    state.RemainingCooldownTicks = state.CooldownTicks;
    command = new NpcRangedAttackCommand(
      sourceNpc,
      sequence,
      state.ProjectileType,
      state.Damage,
      sourcePosition,
      new SimulationVector(
        deltaX / length * state.ProjectileSpeed,
        deltaY / length * state.ProjectileSpeed));
    return true;
  }

  private static bool IsFinite(SimulationVector value)
  {
    return float.IsFinite(value.X) && float.IsFinite(value.Y);
  }
}
