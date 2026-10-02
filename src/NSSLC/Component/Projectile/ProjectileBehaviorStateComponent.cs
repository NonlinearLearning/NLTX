using System;

namespace Terraria.Projectile;

public struct ProjectileBehaviorStateComponent
{
  public const int MaxAiSlots = 3;

  public ProjectileBehaviorStateComponent(
    float ai0 = 0.0f,
    float ai1 = 0.0f,
    float ai2 = 0.0f,
    float localAi0 = 0.0f,
    float localAi1 = 0.0f,
    float localAi2 = 0.0f)
  {
    ValidateFinite(ai0, nameof(ai0));
    ValidateFinite(ai1, nameof(ai1));
    ValidateFinite(ai2, nameof(ai2));
    ValidateFinite(localAi0, nameof(localAi0));
    ValidateFinite(localAi1, nameof(localAi1));
    ValidateFinite(localAi2, nameof(localAi2));

    Ai0 = ai0;
    Ai1 = ai1;
    Ai2 = ai2;
    LocalAi0 = localAi0;
    LocalAi1 = localAi1;
    LocalAi2 = localAi2;
  }

  public float Ai0;
  public float Ai1;
  public float Ai2;
  public float LocalAi0;
  public float LocalAi1;
  public float LocalAi2;

  public readonly float GetAi(int slot)
  {
    return slot switch
    {
      0 => Ai0,
      1 => Ai1,
      2 => Ai2,
      _ => throw new ArgumentOutOfRangeException(nameof(slot)),
    };
  }

  public readonly float GetLocalAi(int slot)
  {
    return slot switch
    {
      0 => LocalAi0,
      1 => LocalAi1,
      2 => LocalAi2,
      _ => throw new ArgumentOutOfRangeException(nameof(slot)),
    };
  }

  public void ResetAiState()
  {
    Ai0 = 0.0f;
    Ai1 = 0.0f;
    Ai2 = 0.0f;
    LocalAi0 = 0.0f;
    LocalAi1 = 0.0f;
    LocalAi2 = 0.0f;
  }

  private static void ValidateFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
