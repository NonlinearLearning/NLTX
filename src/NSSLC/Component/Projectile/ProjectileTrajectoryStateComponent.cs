using System;

namespace Terraria.Projectile;

public struct ProjectileTrajectoryStateComponent
{
  public ProjectileTrajectoryStateComponent(
    float rotation,
    int spriteDirection,
    float stepSpeed,
    int numUpdates,
    float gfxOffY)
  {
    ValidateFinite(rotation, nameof(rotation));
    ValidateFinite(stepSpeed, nameof(stepSpeed));
    ValidateFinite(gfxOffY, nameof(gfxOffY));
    if (stepSpeed < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(stepSpeed));
    }

    Rotation = rotation;
    SpriteDirection = spriteDirection;
    StepSpeed = stepSpeed;
    NumUpdates = numUpdates;
    GfxOffY = gfxOffY;
  }

  public float Rotation;
  public int SpriteDirection;
  public float StepSpeed;
  // -1 is the exhausted-loop sentinel; projectile behavior may reset it to 0.
  public int NumUpdates;
  public float GfxOffY;

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
