namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeMarkerLifecycleComponent
{
  public RevengeMarkerLifecycleComponent(int expiresAtGameTime)
  {
    ExpiresAtGameTime = expiresAtGameTime;
  }

  public int ExpiresAtGameTime { get; }

  public bool ForceExpire { get; private set; }

  public bool RespawnAttemptLocked { get; private set; }

  public bool IsExpiredAt(int currentGameTime)
  {
    return ForceExpire || currentGameTime >= ExpiresAtGameTime;
  }

  internal void MarkForceExpired()
  {
    ForceExpire = true;
  }

  internal void SetRespawnAttemptLocked(bool locked)
  {
    RespawnAttemptLocked = locked;
  }
}
