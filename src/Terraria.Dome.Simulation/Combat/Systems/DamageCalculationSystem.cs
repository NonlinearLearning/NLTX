using System;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Combat.Systems;

public sealed class DamageCalculationSystem
{
  private const double MinimumDamage = 1.0;

  public double CalculateRawAmount(
    int rawAmount,
    int defense,
    DamageTargetKind targetKind,
    WorldRuleState worldRules)
  {
    ArgumentNullException.ThrowIfNull(worldRules);
    if (rawAmount <= 0)
    {
      return 0.0;
    }

    if (defense < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(defense));
    }

    double defenseMultiplier = targetKind switch
    {
      DamageTargetKind.Npc => 0.5,
      DamageTargetKind.PlayerPvp => 0.5,
      DamageTargetKind.Player when worldRules.IsMasterMode => 1.0,
      DamageTargetKind.Player when worldRules.IsExpertMode => 0.75,
      DamageTargetKind.Player => 0.5,
      _ => throw new ArgumentOutOfRangeException(nameof(targetKind))
    };

    return Math.Max(MinimumDamage, rawAmount - defense * defenseMultiplier);
  }

  public int CalculateAppliedAmount(
    int rawAmount,
    int defense,
    DamageTargetKind targetKind,
    WorldRuleState worldRules)
  {
    double calculated = CalculateRawAmount(rawAmount, defense, targetKind, worldRules);
    return calculated <= 0.0 ? 0 : (int)calculated;
  }
}
