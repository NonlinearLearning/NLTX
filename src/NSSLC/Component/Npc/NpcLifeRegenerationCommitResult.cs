using System;
using System.Collections.Generic;

namespace Terraria.Npc;

public sealed class NpcLifeRegenerationCommitResult
{
  internal NpcLifeRegenerationCommitResult(
    bool computed,
    NpcLifeRegenerationFailureReason failureReason,
    int healingRequested,
    int healingApplied,
    NpcDamageOverTimeResult[] damageEvents)
  {
    Computed = computed;
    FailureReason = failureReason;
    HealingRequested = healingRequested;
    HealingApplied = healingApplied;
    DamageEvents = Array.AsReadOnly(damageEvents);
  }

  public bool Computed { get; }

  public NpcLifeRegenerationFailureReason FailureReason { get; }

  public int HealingRequested { get; }

  public int HealingApplied { get; }

  public IReadOnlyList<NpcDamageOverTimeResult> DamageEvents { get; }

  public int AcceptedDamageEventCount
  {
    get
    {
      int accepted = 0;
      for (int index = 0; index < DamageEvents.Count; index++)
      {
        if (DamageEvents[index].Accepted)
        {
          accepted++;
        }
      }

      return accepted;
    }
  }
}
