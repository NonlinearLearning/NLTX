using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldInvasionStartEligibilitySystem
{
  public bool CanStart(
    int invasionType,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    ArgumentNullException.ThrowIfNull(players);
    if (invasionType is < 1 or > 4)
    {
      return false;
    }

    return CountQualifiedPlayers(players) > 0;
  }

  public int CountQualifiedPlayers(IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    ArgumentNullException.ThrowIfNull(players);
    int qualifiedPlayers = 0;
    for (int index = 0; index < players.Count; index++)
    {
      WorldInvasionPlayerSnapshot player = players[index];
      if (player.IsActive && player.MaximumHealth >= 200)
      {
        qualifiedPlayers++;
      }
    }

    return qualifiedPlayers;
  }
}
