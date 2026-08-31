using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Player.Commands;

namespace Terraria.Dome.Simulation.Npc.Systems;

public readonly record struct NpcContactCandidate(
  NpcHandle Npc,
  SimulationVector Position,
  ColliderComponent Collider,
  NpcFaction Faction,
  bool IsActive);

public readonly record struct PlayerContactCandidate(
  PlayerHandle Player,
  SimulationVector Position,
  ColliderComponent Collider,
  bool IsActive,
  int CooldownTicks);

public sealed class NpcContactEffectSystem
{
  public IReadOnlyList<DamagePlayerCommand> ProduceDamageCommands(
    IReadOnlyList<NpcContactCandidate> npcs,
    IReadOnlyList<PlayerContactCandidate> players,
    int damage)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentNullException.ThrowIfNull(players);
    if (damage <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(damage));
    }

    List<DamagePlayerCommand> commands = new();
    for (int npcIndex = 0; npcIndex < npcs.Count; npcIndex++)
    {
      NpcContactCandidate npc = npcs[npcIndex];
      if (!npc.IsActive || !npc.Npc.IsValid || npc.Faction != NpcFaction.Hostile ||
          !IsValidGeometry(npc.Position, npc.Collider))
      {
        continue;
      }

      for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
      {
        PlayerContactCandidate player = players[playerIndex];
        if (!player.IsActive || !player.Player.IsValid || player.CooldownTicks > 0 ||
            !IsValidGeometry(player.Position, player.Collider) ||
            !Overlaps(npc.Position, npc.Collider, player.Position, player.Collider))
        {
          continue;
        }

        commands.Add(new DamagePlayerCommand(player.Player, damage));
      }
    }

    return commands;
  }

  private static bool Overlaps(
    SimulationVector firstPosition,
    ColliderComponent firstCollider,
    SimulationVector secondPosition,
    ColliderComponent secondCollider)
  {
    return firstPosition.X < secondPosition.X + secondCollider.Width &&
      firstPosition.X + firstCollider.Width > secondPosition.X &&
      firstPosition.Y < secondPosition.Y + secondCollider.Height &&
      firstPosition.Y + firstCollider.Height > secondPosition.Y;
  }

  private static bool IsValidGeometry(SimulationVector position, ColliderComponent collider)
  {
    return float.IsFinite(position.X) && float.IsFinite(position.Y) &&
      float.IsFinite(collider.Width) && collider.Width > 0.0f &&
      float.IsFinite(collider.Height) && collider.Height > 0.0f;
  }
}
