using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.StatusEffects.Snapshots;

public sealed class PlayerStatusEffectStateSnapshot
{
  public PlayerStatusEffectStateSnapshot(
    int playerId,
    long revision,
    IReadOnlyList<StatusEffectSnapshot> effects)
  {
    ArgumentNullException.ThrowIfNull(effects);
    if (playerId <= 0 || revision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(playerId));
    }

    for (int index = 0; index < effects.Count; index++)
    {
      StatusEffectSnapshot effect = effects[index];
      if (effect.TargetKind != StatusEffectTargetKind.Player || effect.TargetId != playerId ||
          effect.Revision != revision || effect.Type == 0 || effect.RemainingTicks <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(effects));
      }
    }

    PlayerId = playerId;
    Revision = revision;
    Effects = Array.AsReadOnly([.. effects]);
  }

  public IReadOnlyList<StatusEffectSnapshot> Effects { get; }

  public int PlayerId { get; }

  public long Revision { get; }
}
