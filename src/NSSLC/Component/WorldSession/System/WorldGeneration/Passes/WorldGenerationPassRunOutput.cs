using System;

namespace Terraria.WorldGeneration.Passes;

/// <summary>
/// Carries measurements produced by a completed pass invocation.
/// </summary>
public sealed class WorldGenerationPassRunOutput
{
  public WorldGenerationPassRunOutput(
    int durationMs = 0,
    uint? hash = null,
    int? randomNextValue = null)
  {
    if (durationMs < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationMs));
    }

    DurationMs = durationMs;
    Hash = hash;
    RandomNextValue = randomNextValue;
  }

  public int DurationMs { get; }

  public uint? Hash { get; }

  public int? RandomNextValue { get; }
}
