using System;

namespace Terraria.WorldSession.Environment.Storm;

public sealed class SandstormStateComponent
{
  public const int MaximumDurationTicks = 86400;

  public SandstormStateComponent(
    bool active = false,
    int remainingTicks = 0,
    float severity = 0.0f,
    float targetSeverity = 0.0f)
  {
    Active = active;
    RemainingTicks = remainingTicks;
    Severity = severity;
    TargetSeverity = targetSeverity;
    Validate();
  }

  public bool Active { get; internal set; }

  public int RemainingTicks { get; internal set; }

  public float Severity { get; internal set; }

  public float TargetSeverity { get; internal set; }

  public void Validate()
  {
    if (RemainingTicks is < 0 or > MaximumDurationTicks)
    {
      throw new ArgumentOutOfRangeException(nameof(RemainingTicks));
    }

    ValidateSeverity(Severity, nameof(Severity));
    ValidateSeverity(TargetSeverity, nameof(TargetSeverity));
  }

  private static void ValidateSeverity(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
