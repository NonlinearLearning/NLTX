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
    out int appliedAmount,
    float postDefenseDamageMultiplier = 1.0f)
  {
    if (!float.IsFinite(postDefenseDamageMultiplier) || postDefenseDamageMultiplier < 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(postDefenseDamageMultiplier));
    }

    if (rawAmount <= 0 || health.Current <= 0 || immunity.IsImmune)
    {
      appliedAmount = 0;
      return false;
    }

    double resolvedAmount = _damageCalculationSystem.CalculateRawAmount(
      rawAmount,
      defense.Value,
      targetKind,
      worldRules);
    appliedAmount = ApplyPostDefenseMultiplier(resolvedAmount, postDefenseDamageMultiplier);
    health.Current = Math.Clamp(health.Current - appliedAmount, 0, health.Maximum);
    return appliedAmount > 0;
  }

  private static int ApplyPostDefenseMultiplier(double amount, float multiplier)
  {
    double scaledAmount = amount * (double)multiplier;
    return scaledAmount >= int.MaxValue ? int.MaxValue : (int)scaledAmount;
  }
}
