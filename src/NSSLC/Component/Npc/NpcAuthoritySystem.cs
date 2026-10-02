using System;
using Terraria.Combat;
using Terraria.Npc.Network;

namespace Terraria.Npc;

public sealed class NpcAuthoritySystem
{
  public bool CommitDeathPhaseDecision(
    NpcDeathLifecycleResult? deathLifecycle,
    NpcBehaviorComponent behavior,
    ref DamageAcceptancePolicyComponent damagePolicy,
    NpcNetworkSyncIntentComponent networkSyncIntent)
  {
    ArgumentNullException.ThrowIfNull(behavior);
    ArgumentNullException.ThrowIfNull(networkSyncIntent);

    if (deathLifecycle is not NpcDeathLifecycleResult result ||
        result.IsTerminal ||
        result.PhaseDecision is not NpcDeathPhaseDecision decision ||
        decision.RequiredInputMissing ||
        !decision.StateChanged)
    {
      return false;
    }

    behavior.ApplyAiState(decision.AiAfter);
    if (decision.RejectAllDamage)
    {
      damagePolicy.RejectAllDamage = true;
    }

    if (decision.RejectHostileDamage)
    {
      damagePolicy.RejectHostileDamage = true;
    }

    if (decision.ReplicationSyncRequested)
    {
      networkSyncIntent.Mark();
    }

    return true;
  }
}
