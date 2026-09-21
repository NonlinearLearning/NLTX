namespace Terraria.DeathPenaltyAndRevenge;

public sealed class RevengeRespawnAttemptComponent
{
  public bool ForceExpire { get; private set; }

  public bool AttemptedRespawn { get; private set; }

  public bool IsLocked => AttemptedRespawn;

  internal void MarkForceExpired()
  {
    ForceExpire = true;
  }

  internal void SetAttemptedRespawn(bool attempted)
  {
    AttemptedRespawn = attempted;
  }
}
