using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Snapshots;

public sealed class NpcStatusEffectStateSnapshot
{
  public NpcStatusEffectStateSnapshot(
    int replicationId,
    long revision,
    IReadOnlyList<StatusEffectSnapshot> effects)
  {
    ArgumentNullException.ThrowIfNull(effects);
    if (replicationId <= 0 || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(replicationId));
    }

    for (int index = 0; index < effects.Count; index++)
    {
      StatusEffectSnapshot effect = effects[index];
      if (effect.TargetKind != StatusEffectTargetKind.Npc || effect.TargetId != replicationId ||
          effect.Revision != revision || effect.Type == 0 || effect.RemainingTicks <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(effects));
      }
    }

    ReplicationId = replicationId;
    Revision = revision;
    Effects = Array.AsReadOnly([.. effects]);
  }

  public IReadOnlyList<StatusEffectSnapshot> Effects { get; }

  public int ReplicationId { get; }

  public long Revision { get; }
}
