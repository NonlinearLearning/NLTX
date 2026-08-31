using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.StatusEffects.Snapshots;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct NpcStatusEffectEnvelope(
  int ReplicationId,
  long Revision,
  IReadOnlyList<StatusEffectSnapshot> Effects)
{
  public const int MaximumEffectCount = 44;

  public void Validate()
  {
    ArgumentNullException.ThrowIfNull(Effects);
    if (ReplicationId <= 0 || Revision < 0 || Effects.Count > MaximumEffectCount)
    {
      throw new ArgumentOutOfRangeException(nameof(Effects));
    }

    for (int index = 0; index < Effects.Count; index++)
    {
      StatusEffectSnapshot effect = Effects[index];
      if (effect.TargetKind != StatusEffectTargetKind.Npc || effect.TargetId != ReplicationId ||
          effect.Revision != Revision || effect.Type == 0 || effect.RemainingTicks <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(Effects));
      }
    }
  }
}
