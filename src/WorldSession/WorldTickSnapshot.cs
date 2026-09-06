using System;

namespace Terraria.WorldSession.Components;

public sealed class WorldTickSnapshot
{
  private WorldTickSnapshot(
    long revision,
    WorldDescriptorSnapshotValue? descriptor,
    WorldRulesSnapshotValue? rules,
    WorldClockSnapshotValue? clock,
    WorldWeatherSnapshotValue? weather,
    SessionReadinessSnapshotValue? readiness,
    bool isCommitted)
  {
    Revision = revision;
    Descriptor = descriptor;
    Rules = rules;
    Clock = clock;
    Weather = weather;
    Readiness = readiness;
    IsCommitted = isCommitted;
  }

  public static WorldTickSnapshot Uncommitted => new(
    0,
    null,
    null,
    null,
    null,
    null,
    false);

  public static WorldTickSnapshot CreateCommitted(
    long revision,
    WorldDescriptorSnapshotValue descriptor,
    WorldRulesSnapshotValue rules,
    WorldClockSnapshotValue clock,
    WorldWeatherSnapshotValue weather,
    SessionReadinessSnapshotValue readiness)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(revision);
    if (!readiness.CanUpdateEntities)
    {
      throw new ArgumentException(
        "A committed world snapshot requires an updateable session.",
        nameof(readiness));
    }

    return new WorldTickSnapshot(
      revision,
      descriptor,
      rules,
      clock,
      weather,
      readiness,
      true);
  }

  public long Revision { get; }
  public bool IsCommitted { get; }
  public WorldDescriptorSnapshotValue? Descriptor { get; }
  public WorldRulesSnapshotValue? Rules { get; }
  public WorldClockSnapshotValue? Clock { get; }
  public WorldWeatherSnapshotValue? Weather { get; }
  public SessionReadinessSnapshotValue? Readiness { get; }
}
