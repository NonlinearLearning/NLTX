using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the diagnostic and integrity data committed for one generation pass.
/// </summary>
public sealed class WorldGenerationPassResultComponent
{
  public WorldGenerationPassResultComponent(
    long generationId,
    int passIndex,
    string passId,
    int durationMs = 0,
    uint? hash = null,
    bool skipped = false,
    int? randomNextValue = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (passIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(passIndex));
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    if (durationMs < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(durationMs));
    }

    GenerationId = generationId;
    PassIndex = passIndex;
    PassId = passId;
    DurationMs = durationMs;
    Hash = hash;
    Skipped = skipped;
    RandomNextValue = randomNextValue;
  }

  public long GenerationId { get; }

  public int PassIndex { get; }

  public string PassId { get; }

  public int DurationMs { get; private set; }

  public uint? Hash { get; private set; }

  public bool Skipped { get; private set; }

  public int? RandomNextValue { get; }
}
