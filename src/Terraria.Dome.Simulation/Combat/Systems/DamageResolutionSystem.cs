using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Combat.Components;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class DamageResolutionSystem
{
  public bool TryResolve(
    ref HealthComponent health,
    DefenseComponent defense,
    ref ImmunityComponent immunity,
    int rawAmount,
    out int appliedAmount)
  {
    if (rawAmount <= 0 || health.Current <= 0 || immunity.IsImmune)
    {
      appliedAmount = 0;
      return false;
    }

    appliedAmount = Math.Max(0, rawAmount - defense.Value);
    health.Current = Math.Clamp(health.Current - appliedAmount, 0, health.Maximum);
    return appliedAmount > 0;
  }
}
