using System;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Persisted sandstorm state from the WorldFile header continuation.
/// </summary>
public sealed class WorldFileSandstormSection
{
  public const string SectionId = "world.sandstorm";

  public WorldFileSandstormSection(
    bool happening,
    int timeLeft,
    float severity,
    float intendedSeverity)
  {
    if (!float.IsFinite(severity))
    {
      throw new ArgumentOutOfRangeException(nameof(severity));
    }

    if (!float.IsFinite(intendedSeverity))
    {
      throw new ArgumentOutOfRangeException(nameof(intendedSeverity));
    }

    Happening = happening;
    TimeLeft = timeLeft;
    Severity = severity;
    IntendedSeverity = intendedSeverity;
  }

  public bool Happening { get; }

  public int TimeLeft { get; }

  public float Severity { get; }

  public float IntendedSeverity { get; }

  public static WorldFileSandstormSection Empty => new(
    happening: false,
    timeLeft: 0,
    severity: 0,
    intendedSeverity: 0);
}
