namespace Terraria.DeathPenaltyAndRevenge;

public readonly record struct RevengeRespawnDecision(
  RevengeRespawnDecisionKind Kind)
{
  public bool ArmsAttempt => Kind is
    RevengeRespawnDecisionKind.ForceExpire or
    RevengeRespawnDecisionKind.RequestSpawn;

  public bool RemovesMarker => Kind is
    RevengeRespawnDecisionKind.RemoveExpired or
    RevengeRespawnDecisionKind.RemoveInvalid;
}
