using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the diagnostic and integrity data committed for one generation pass.
/// </summary>
public sealed class WorldGenerationPassResultComponent
{
  public WorldGenerationPassResultComponent(
    int durationMs = 0,
    uint? hash = null,
    bool skipped = false)
  {
    if (durationMs < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationMs));
    }

    DurationMs = durationMs;
    Hash = hash;
    Skipped = skipped;
  }

  public int DurationMs { get; private set; }

  public uint? Hash { get; private set; }

  public bool Skipped { get; private set; }
}
