using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Snapshots;
using Terraria.Dome.Simulation.WorldModel.Systems;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcSpawnEligibilitySystem
{
  private readonly WorldInvasionSpawnEligibilitySystem _invasionSpawnEligibilitySystem = new();

  public IReadOnlyList<SpawnNpcCommand> Evaluate(NpcSpawnSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<SpawnNpcCommand> commands = new();
    if (!snapshot.SpawnAuthorityEnabled)
    {
      return commands;
    }

    HashSet<int> requestedIds = new(snapshot.ExistingReplicationIds);
    double activeSlots = snapshot.ActiveNpcSlots;
    int protectedCount = snapshot.ProtectedSlotCount;
    for (int index = 0; index < snapshot.Candidates.Count; index++)
    {
      NpcSpawnCandidate candidate = snapshot.Candidates[index];
      SpawnNpcCommand command = candidate.Command;
      bool canSpawnEnemiesNear = candidate.PlayerReadiness.HasValue
        ? NpcSpawnPlayerReadinessQuery.CanSpawnEnemiesNear(candidate.PlayerReadiness.Value)
        : candidate.CanSpawnEnemiesNear;
      if (candidate.IsOccupied ||
          !canSpawnEnemiesNear ||
          command.DefinitionId <= 0 ||
          command.DifficultyScale <= 0.0f ||
          !float.IsFinite(command.Position.X) ||
          !float.IsFinite(command.Position.Y) ||
          !float.IsFinite(candidate.NpcSlotCost) ||
          candidate.NpcSlotCost < 0.0f)
      {
        continue;
      }

      if (candidate.IsInvasionCandidate)
      {
        if (!snapshot.InvasionState.HasValue)
        {
          continue;
        }

        NpcInvasionSpawnState invasionState = snapshot.InvasionState.Value;
        if (!_invasionSpawnEligibilitySystem.CanSpawn(
          invasionState.InvasionType,
          invasionState.InvasionSize,
          invasionState.InvasionDelayTicks) ||
            invasionState.ReachedInvasionBossCap)
        {
          continue;
        }
      }

      if (candidate.IsProtectedSlot)
      {
        if (protectedCount <= 0)
        {
          continue;
        }

        protectedCount--;
      }

      double nextActiveSlots = activeSlots + candidate.NpcSlotCost;
      if (activeSlots >= snapshot.MaximumNpcCount ||
          nextActiveSlots > snapshot.MaximumNpcCount)
      {
        break;
      }

      if (command.RequestedReplicationId > 0 &&
          !requestedIds.Add(command.RequestedReplicationId))
      {
        continue;
      }

      commands.Add(command);
      activeSlots = nextActiveSlots;
    }

    return commands;
  }
}
