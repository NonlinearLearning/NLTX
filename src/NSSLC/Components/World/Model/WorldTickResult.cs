using Terraria.Physics;

namespace Terraria.World;

/// <summary>Result of one explicit world tick attempt.</summary>
public readonly record struct WorldTickResult(
    bool Applied,
    long Tick,
    SpatialWorldRevision Revision,
    string? Failure = null) {
  public static WorldTickResult NotRun => new(false, -1, new SpatialWorldRevision(0, -1));

  internal static WorldTickResult Success(long tick, SpatialWorldRevision revision) =>
      new(true, tick, revision);

  internal static WorldTickResult Rejected(
      long tick,
      SpatialWorldRevision revision,
      string failure) {
    ArgumentException.ThrowIfNullOrWhiteSpace(failure);
    return new(false, tick, revision, failure);
  }
}
