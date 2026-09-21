using System;

namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerLifecycleComponent
{
  private readonly RevengeRespawnAttemptComponent _respawnAttemptState = new();

  public RevengeMarkerLifecycleComponent(int expiresAtGameTime)
  {
    if (expiresAtGameTime < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expiresAtGameTime));
    }

    ExpiresAtGameTime = expiresAtGameTime;
  }

  public int ExpiresAtGameTime { get; }

  public bool ForceExpire => _respawnAttemptState.ForceExpire;

  public bool RespawnAttemptLocked => _respawnAttemptState.IsLocked;

  public bool IsExpiredAt(int currentGameTime)
  {
    return ForceExpire || currentGameTime >= ExpiresAtGameTime;
  }

  internal void MarkForceExpired()
  {
    _respawnAttemptState.MarkForceExpired();
  }

  internal void SetRespawnAttemptLocked(bool locked)
  {
    _respawnAttemptState.SetAttemptedRespawn(locked);
  }

  internal RevengeRespawnAttemptComponent RespawnAttemptState => _respawnAttemptState;
}
