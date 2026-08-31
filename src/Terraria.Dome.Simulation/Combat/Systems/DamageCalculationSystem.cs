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
    WorldRuleState worldRules,
    int armorPenetration = 0)
  {
    ArgumentNullException.ThrowIfNull(worldRules);
    if (rawAmount <= 0)
    {
      return 0.0;
    }

    if (defense < 0 || armorPenetration < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(defense));
    }

    int effectiveDefense = Math.Max(0, defense - armorPenetration);
    double defenseMultiplier = targetKind switch
    {
      DamageTargetKind.Npc => 0.5,
      DamageTargetKind.PlayerPvp => 0.5,
      DamageTargetKind.Player when worldRules.IsMasterMode => 1.0,
      DamageTargetKind.Player when worldRules.IsExpertMode => 0.75,
      DamageTargetKind.Player => 0.5,
      _ => throw new ArgumentOutOfRangeException(nameof(targetKind))
    };

    return Math.Max(MinimumDamage, rawAmount - effectiveDefense * defenseMultiplier);
  }

  public int CalculateAppliedAmount(
    int rawAmount,
    int defense,
    DamageTargetKind targetKind,
    WorldRuleState worldRules,
    int armorPenetration = 0)
  {
    double calculated = CalculateRawAmount(
      rawAmount, defense, targetKind, worldRules, armorPenetration);
    return calculated <= 0.0 ? 0 : (int)calculated;
  }
}
