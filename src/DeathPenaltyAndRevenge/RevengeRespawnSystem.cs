using System;

namespace Terraria.DeathPenaltyAndRevenge;

public static class RevengeRespawnSystem
{
  public static RevengeRespawnDecision Evaluate(
    bool isExpired,
    bool isInvalid,
    bool intersectsPlayerOuterBox,
    bool respawnAttemptLocked,
    bool wouldBeDiscouraged)
  {
    if (isExpired)
    {
      return new RevengeRespawnDecision(RevengeRespawnDecisionKind.RemoveExpired);
    }

    if (isInvalid)
    {
      return new RevengeRespawnDecision(RevengeRespawnDecisionKind.RemoveInvalid);
    }

    if (!intersectsPlayerOuterBox)
    {
      return new RevengeRespawnDecision(RevengeRespawnDecisionKind.UnlockAttempt);
    }

    if (respawnAttemptLocked)
    {
      return new RevengeRespawnDecision(RevengeRespawnDecisionKind.NoAction);
    }

    if (wouldBeDiscouraged)
    {
      return new RevengeRespawnDecision(RevengeRespawnDecisionKind.ForceExpire);
    }

    return new RevengeRespawnDecision(RevengeRespawnDecisionKind.RequestSpawn);
  }

  public static void ApplyAttemptState(
    RevengeRespawnAttemptComponent attemptState,
    RevengeRespawnDecision decision)
  {
    ArgumentNullException.ThrowIfNull(attemptState);

    switch (decision.Kind)
    {
      case RevengeRespawnDecisionKind.UnlockAttempt:
        attemptState.SetAttemptedRespawn(false);
        break;
      case RevengeRespawnDecisionKind.ForceExpire:
        attemptState.SetAttemptedRespawn(true);
        attemptState.MarkForceExpired();
        break;
      case RevengeRespawnDecisionKind.RequestSpawn:
        attemptState.SetAttemptedRespawn(true);
        break;
    }
  }
}
