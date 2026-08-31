using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileHostileDamageScalingSystem
{
  private const float JourneyDifficulty = 0.5f;
  private const float ClassicDifficulty = 1.0f;
  private const float ExpertDifficulty = 2.0f;
  private const float MasterDifficulty = 3.0f;
  private const float LightningBaseMultiplier = 0.08f;

  public int ScalePlayerDamage(
    int rawDamage,
    ProjectileDefinitionComponent definition,
    WorldRuleState worldRules)
  {
    ArgumentNullException.ThrowIfNull(worldRules);
    if (rawDamage <= 0 || !definition.Hostile ||
        definition.PlayerDamagePolicy == PlayerDamagePolicy.None)
    {
      return rawDamage;
    }

    float multiplier = definition.HostileDamageScaling switch
    {
      ProjectileHostileDamageScaling.Default => GetDifficulty(worldRules),
      ProjectileHostileDamageScaling.Lightning =>
        GetDifficulty(worldRules) * LightningBaseMultiplier,
      _ => throw new ArgumentOutOfRangeException(nameof(definition))
    };
    return Math.Max(1, (int)MathF.Floor(rawDamage * multiplier));
  }

  private static float GetDifficulty(WorldRuleState worldRules)
  {
    if (worldRules.IsJourneyMode)
    {
      return JourneyDifficulty;
    }

    if (worldRules.IsMasterMode || worldRules.GameMode == WorldGameMode.Master)
    {
      return MasterDifficulty;
    }

    if (worldRules.IsExpertMode || worldRules.GameMode == WorldGameMode.Expert)
    {
      return ExpertDifficulty;
    }

    return ClassicDifficulty;
  }
}
