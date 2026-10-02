using System;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerSnapshot
{
  public RevengeMarkerSnapshot(
    RevengeMarkerId markerId,
    RevengeTargetSnapshotComponent target,
    int expiresAtGameTime,
    bool forceExpire,
    bool respawnAttemptLocked,
    uint revision = 0)
  {
    if (!markerId.IsAssigned)
    {
      throw new ArgumentException(
        "A marker snapshot must have an assigned ID.",
        nameof(markerId));
    }

    ArgumentNullException.ThrowIfNull(target);
    if (expiresAtGameTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresAtGameTime));
    }

    MarkerId = markerId;
    Target = target;
    ExpiresAtGameTime = expiresAtGameTime;
    ForceExpire = forceExpire;
    RespawnAttemptLocked = respawnAttemptLocked;
    Revision = revision;
  }

  public RevengeMarkerId MarkerId { get; }

  public RevengeTargetSnapshotComponent Target { get; }

  public int ExpiresAtGameTime { get; }

  public bool ForceExpire { get; }

  public bool RespawnAttemptLocked { get; }

  public uint Revision { get; }

  public bool IsExpiredAt(int gameTime)
  {
    return ForceExpire || gameTime >= ExpiresAtGameTime;
  }

  public RevengeMarkerSnapshot WithLifecycle(
    bool forceExpire,
    bool respawnAttemptLocked)
  {
    return new RevengeMarkerSnapshot(
      MarkerId,
      Target,
      ExpiresAtGameTime,
      forceExpire,
      respawnAttemptLocked,
      Revision);
  }

  internal RevengeMarkerSnapshot WithRevision(uint revision)
  {
    return new RevengeMarkerSnapshot(
      MarkerId,
      Target,
      ExpiresAtGameTime,
      ForceExpire,
      RespawnAttemptLocked,
      revision);
  }
}
