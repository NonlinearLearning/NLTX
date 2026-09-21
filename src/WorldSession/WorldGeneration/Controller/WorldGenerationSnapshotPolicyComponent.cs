using System;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the snapshot cadence selected for the current generation run.
/// </summary>
public sealed class WorldGenerationSnapshotPolicyComponent
{
  public enum Frequency : sbyte
  {
    None = -1,
    Manual,
    Automatic,
    Always,
  }

  public WorldGenerationSnapshotPolicyComponent(
    Frequency snapshotFrequency = Frequency.None)
  {
    if (!Enum.IsDefined(snapshotFrequency))
    {
      throw new ArgumentOutOfRangeException(nameof(snapshotFrequency));
    }

    SnapshotFrequency = snapshotFrequency;
  }

  public Frequency SnapshotFrequency { get; private set; }
}
