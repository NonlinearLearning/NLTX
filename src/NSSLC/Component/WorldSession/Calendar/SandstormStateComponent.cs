using System;

namespace Terraria.WorldSession.Calendar;

public sealed class SandstormStateComponent
{
  public const int MaximumDurationTicks = 86400;

  public SandstormStateComponent(
    bool happening = false,
    int timeLeft = 0,
    float severity = 0.0f,
    float intendedSeverity = 0.0f)
  {
    Happening = happening;
    TimeLeft = timeLeft;
    Severity = severity;
    IntendedSeverity = intendedSeverity;
    Validate();
  }

  public bool Happening;
  public int TimeLeft;
  public float Severity;
  public float IntendedSeverity;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(TimeLeft);
    if (TimeLeft > MaximumDurationTicks)
    {
      throw new ArgumentOutOfRangeException(nameof(TimeLeft));
    }

    ValidateSeverity(Severity, nameof(Severity));
    ValidateSeverity(IntendedSeverity, nameof(IntendedSeverity));
  }

  private static void ValidateSeverity(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value < 0.0f || value > 1.0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
