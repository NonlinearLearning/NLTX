using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class KingSlimeReadinessSystem
{
  private const int MinimumDefenseExclusive = 8;
  private const int MinimumHealthExclusive = 140;

  public bool HasReadyPlayer(IReadOnlyList<KingSlimePlayerSnapshot> players)
  {
    ArgumentNullException.ThrowIfNull(players);
    for (int index = 0; index < players.Count; index++)
    {
      KingSlimePlayerSnapshot player = players[index];
      if (player.IsActive &&
          player.MaximumHealth > MinimumHealthExclusive &&
          player.Defense > MinimumDefenseExclusive)
      {
        return true;
      }
    }

    return false;
  }
}
