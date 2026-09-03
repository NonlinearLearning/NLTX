using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation;

public sealed class SimulationSnapshot
{
  public SimulationSnapshot(
    long tick,
    IReadOnlyList<PlayerSnapshot> players,
    IReadOnlyList<NpcSnapshot> npcs,
    IReadOnlyList<ProjectileSnapshot> projectiles)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(projectiles);
    Npcs = npcs;
    Players = players;
    Projectiles = projectiles;
    Tick = tick;
  }

  public IReadOnlyList<NpcSnapshot> Npcs { get; }
  public IReadOnlyList<PlayerSnapshot> Players { get; }
  public IReadOnlyList<ProjectileSnapshot> Projectiles { get; }
  public long Tick { get; }

  public NpcSnapshot FindNpc(NpcHandle npc)
  {
    for (int index = 0; index < Npcs.Count; index++)
    {
      if (Npcs[index].Npc == npc)
      {
        return Npcs[index];
      }
    }

    throw new KeyNotFoundException($"Npc {npc.Value} does not exist.");
  }

  public PlayerSnapshot FindPlayer(PlayerHandle player)
  {
    for (int index = 0; index < Players.Count; index++)
    {
      if (Players[index].Player == player)
      {
        return Players[index];
      }
    }

    throw new KeyNotFoundException($"Player {player.Value} does not exist.");
  }
}
