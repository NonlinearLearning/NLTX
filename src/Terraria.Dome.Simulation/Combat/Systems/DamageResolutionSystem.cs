using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Combat.Components;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class DamageResolutionSystem
{
  private readonly DamageCalculationSystem _damageCalculationSystem = new();

  public bool TryResolve(
    ref HealthComponent health,
    DefenseComponent defense,
    ref ImmunityComponent immunity,
    int rawAmount,
    DamageTargetKind targetKind,
    WorldRuleState worldRules,
    out int appliedAmount)
  {
    if (rawAmount <= 0 || health.Current <= 0 || immunity.IsImmune)
    {
      appliedAmount = 0;
      return false;
    }

    appliedAmount = _damageCalculationSystem.CalculateAppliedAmount(
      rawAmount,
      defense.Value,
      targetKind,
      worldRules);
    health.Current = Math.Clamp(health.Current - appliedAmount, 0, health.Maximum);
    return appliedAmount > 0;
  }
}
