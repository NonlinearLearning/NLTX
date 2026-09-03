using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Systems;

namespace Terraria.Dome.Simulation.Npc.Systems;

public sealed class NpcEventSpawnSystem
{
  private readonly WorldInvasionSpawnEligibilitySystem _eligibility = new();

  public IReadOnlyList<SpawnNpcCommand> Evaluate(
    WorldProgressionState progression,
    NpcEventSpawnDefinition definition,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players,
    SimulationVector position,
    int activeNpcCount,
    int maximumNpcCount)
  {
    return EvaluateCore(
      progression,
      definition,
      definitions: null,
      players,
      position,
      activeNpcCount,
      maximumNpcCount);
  }

  public IReadOnlyList<SpawnNpcCommand> EvaluateTable(
    WorldProgressionState progression,
    NpcEventSpawnTable table,
    NpcDefinitionRegistry definitions,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players,
    SimulationVector position,
    int activeNpcCount,
    int maximumNpcCount)
  {
    ArgumentNullException.ThrowIfNull(table);
    ArgumentNullException.ThrowIfNull(definitions);
    if (!table.TryGet(progression.InvasionType, out IReadOnlyList<NpcEventSpawnDefinition> entries))
    {
      return [];
    }

    List<SpawnNpcCommand> commands = new();
    for (int index = 0; index < entries.Count; index++)
    {
      IReadOnlyList<SpawnNpcCommand> entryCommands = Evaluate(
        progression,
        entries[index],
        definitions,
        players,
        position,
        activeNpcCount + commands.Count,
        maximumNpcCount);
      commands.AddRange(entryCommands);
      if (activeNpcCount + commands.Count >= maximumNpcCount)
      {
        break;
      }
    }

    return commands;
  }

  public IReadOnlyList<SpawnNpcCommand> Evaluate(
    WorldProgressionState progression,
    NpcEventSpawnDefinition definition,
    NpcDefinitionRegistry definitions,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players,
    SimulationVector position,
    int activeNpcCount,
    int maximumNpcCount)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    return EvaluateCore(
      progression,
      definition,
      definitions,
      players,
      position,
      activeNpcCount,
      maximumNpcCount);
  }

  private IReadOnlyList<SpawnNpcCommand> EvaluateCore(
    WorldProgressionState progression,
    NpcEventSpawnDefinition definition,
    NpcDefinitionRegistry? definitions,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players,
    SimulationVector position,
    int activeNpcCount,
    int maximumNpcCount)
  {
    ArgumentNullException.ThrowIfNull(progression);
    ArgumentNullException.ThrowIfNull(players);
    if (activeNpcCount < 0 || maximumNpcCount < 0 ||
        !float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(activeNpcCount));
    }

    if (progression.InvasionType != definition.EventType ||
        (definitions is not null &&
         !definitions.TryGet(definition.NpcDefinitionId, out _)) ||
        !_eligibility.CanSpawn(
          progression.InvasionType,
          progression.InvasionSize,
          progression.InvasionDelayTicks) ||
        activeNpcCount >= maximumNpcCount ||
        !HasQualifiedPlayer(players))
    {
      return [];
    }

    int budget = Math.Min(
      definition.MaximumPerTick,
      maximumNpcCount - activeNpcCount);
    List<SpawnNpcCommand> commands = new(budget);
    for (int index = 0; index < budget; index++)
    {
      commands.Add(new SpawnNpcCommand(
        definition.NpcDefinitionId,
        position,
        NpcSpawnSource.Event,
        definition.DifficultyScale));
    }

    return commands;
  }

  private static bool HasQualifiedPlayer(IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    for (int index = 0; index < players.Count; index++)
    {
      WorldInvasionPlayerSnapshot player = players[index];
      if (player.IsActive && player.MaximumHealth >= 200)
      {
        return true;
      }
    }

    return false;
  }
}
