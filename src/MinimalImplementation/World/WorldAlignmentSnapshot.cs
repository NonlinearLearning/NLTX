using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>Captures packet 57 values and the world version they describe.</summary>
public sealed class WorldAlignmentSnapshot
{
  internal WorldAlignmentSnapshot(
    ulong worldEpoch,
    CommitVersion version,
    WorldAlignmentMetrics metrics)
  {
    if (worldEpoch == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(worldEpoch));
    }

    if (!version.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    WorldEpoch = worldEpoch;
    Version = version;
    Metrics = metrics;
  }

  public ulong WorldEpoch { get; }

  public CommitVersion Version { get; }

  public WorldAlignmentMetrics Metrics { get; }
}
