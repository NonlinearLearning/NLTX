using System;

namespace Terraria.Projectile;

public struct ProjectileTrajectoryStateComponent
{
  public ProjectileTrajectoryStateComponent(
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    float localAi0 = 0.0f,
    float localAi1 = 0.0f,
    float localAi2 = 0.0f,
    float rotation = 0.0f,
    int spriteDirection = 1,
    float stepSpeed = 1.0f,
    int substepCounter = 0,
    float gfxOffY = 0.0f)
  {
    ValidateFinite(ai0, nameof(ai0));
    ValidateFinite(ai1, nameof(ai1));
    ValidateFinite(ai2, nameof(ai2));
    ValidateFinite(localAi0, nameof(localAi0));
    ValidateFinite(localAi1, nameof(localAi1));
    ValidateFinite(localAi2, nameof(localAi2));
    ValidateFinite(rotation, nameof(rotation));
    ValidateFinite(stepSpeed, nameof(stepSpeed));
    ValidateFinite(gfxOffY, nameof(gfxOffY));
    if (stepSpeed < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(stepSpeed));
    }

    if (substepCounter < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(substepCounter));
    }

    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    LocalAi0 = localAi0;
    LocalAi1 = localAi1;
    LocalAi2 = localAi2;
    Rotation = rotation;
    SpriteDirection = spriteDirection;
    StepSpeed = stepSpeed;
    SubstepCounter = substepCounter;
    GfxOffY = gfxOffY;
  }

  public float Ai0;
  public float Ai1;
  public float Ai2;
  public float LocalAi0;
  public float LocalAi1;
  public float LocalAi2;
  public float Rotation;
  public int SpriteDirection;
  public float StepSpeed;
  public int SubstepCounter;
  public float GfxOffY;

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
