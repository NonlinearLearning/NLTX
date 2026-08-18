using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Snapshots;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcSpawnEligibilitySystem
{
  public IReadOnlyList<SpawnNpcCommand> Evaluate(NpcSpawnSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    List<SpawnNpcCommand> commands = new();
    HashSet<int> requestedIds = new(snapshot.ExistingReplicationIds);
    int activeCount = snapshot.ActiveNpcCount;
    int protectedCount = snapshot.ProtectedSlotCount;
    for (int index = 0; index < snapshot.Candidates.Count; index++)
    {
      NpcSpawnCandidate candidate = snapshot.Candidates[index];
      SpawnNpcCommand command = candidate.Command;
      if (candidate.IsOccupied || command.DefinitionId <= 0 || command.DifficultyScale <= 0.0f)
      {
        continue;
      }

      if (candidate.IsProtectedSlot)
      {
        if (protectedCount <= 0)
        {
          continue;
        }

        protectedCount--;
      }

      if (activeCount >= snapshot.MaximumNpcCount)
      {
        break;
      }

      if (command.RequestedReplicationId > 0 &&
          !requestedIds.Add(command.RequestedReplicationId))
      {
        continue;
      }

      commands.Add(command);
      activeCount++;
    }

    return commands;
  }
}
