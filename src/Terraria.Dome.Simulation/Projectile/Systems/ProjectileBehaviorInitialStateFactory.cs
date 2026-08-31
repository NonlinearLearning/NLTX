using System;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Projectile.Behaviors;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public static class ProjectileBehaviorInitialStateFactory
{
  private const int RandomFrameCount = 6;

  public static ProjectileBehaviorState Create(
    int behaviorId,
    int identity,
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 1.0f)
  {
    if (identity <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    float secondary = behaviorId == LegacyAiStyle2RandomFrameProjectileBehavior.Id
      ? 1 + (identity - 1) % RandomFrameCount
      : ai1;
    return new ProjectileBehaviorState(ai0, secondary, 0, 0, ai2);
  }
}
