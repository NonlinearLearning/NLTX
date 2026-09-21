using System;

namespace Terraria.WorldSession.Calendar;

public sealed class AmbientWindStateComponent
{
  public const float MaximumTargetSpeed = 0.8f;

  public AmbientWindStateComponent(
    float currentSpeed = 0.0f,
    float targetSpeed = 0.0f,
    int changeTimer = 0,
    int extremeChangeTimer = 0)
  {
    CurrentSpeed = currentSpeed;
    TargetSpeed = targetSpeed;
    ChangeTimer = changeTimer;
    ExtremeChangeTimer = extremeChangeTimer;
    Validate();
  }

  public float CurrentSpeed { get; internal set; }

  public float TargetSpeed { get; internal set; }

  public int ChangeTimer { get; internal set; }

  public int ExtremeChangeTimer { get; internal set; }

  public void Validate()
  {
    if (!float.IsFinite(CurrentSpeed))
    {
      throw new ArgumentOutOfRangeException(nameof(CurrentSpeed));
    }

    if (!float.IsFinite(TargetSpeed) || MathF.Abs(TargetSpeed) > MaximumTargetSpeed)
    {
      throw new ArgumentOutOfRangeException(nameof(TargetSpeed));
    }
  }
}
