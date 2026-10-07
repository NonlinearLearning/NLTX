using System;
using System.Numerics;

namespace Terraria.Projectile;

public static class ProjectileStormDefinitionFactory
{
  public const int StormCount = 6;

  public static ProjectileStormDefinition[] CreateAll(float localAi0)
  {
    ProjectileStormDefinition[] definitions = new ProjectileStormDefinition[StormCount];
    for (int stormIndex = 0; stormIndex < definitions.Length; stormIndex++)
    {
      definitions[stormIndex] = Create(stormIndex, localAi0);
    }

    return definitions;
  }

  public static ProjectileStormDefinition Create(int stormIndex, float localAi0)
  {
    float pi = (float)Math.PI;
    float from = (float)stormIndex * 10.0f;
    float to = 90.0f + (float)stormIndex * 10.0f;

    return new ProjectileStormDefinition(
      StartAngle: (float)stormIndex * (pi / 3.0f)
        - pi / 2.0f
        + (float)stormIndex * (pi / 5.0f),
      AnglePerBullet: pi * 2.0f / 3.0f,
      BulletsInStorm: 3,
      BulletsProgressInStormStartNormalized: (localAi0 - from) / (to - from),
      BulletsProgressInStormBonusByIndexNormalized: 0.0f,
      StormTotalRange: 500.0f,
      BulletSize: new Vector2(16.0f, 16.0f));
  }
}
