using System.Collections.Generic;
using Arch.Core;
using Terraria.Dome.Simulation.Components;
using Terraria.Dome.Simulation.Npc;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Npc.Systems;

namespace Terraria.Dome.Simulation;

public sealed partial class DomeSimulation
{
  private void RunNpcSystemPipeline(
    NpcSystemStage firstStage,
    NpcSystemStage lastStage,
    List<string> executedStages)
  {
    _npcSystemPipeline.ExecuteRange(firstStage, lastStage, registration =>
    {
      executedStages.Add(registration.Name);
      switch (registration.Stage)
      {
        case NpcSystemStage.SpawnCommit:
          CommitNpcSpawnCommands();
          break;
        case NpcSystemStage.TargetSelection:
          SelectNpcTargets();
          break;
        case NpcSystemStage.MovementIntent:
          ApplyNpcAi();
          break;
        case NpcSystemStage.MovementAndCollision:
          MoveActiveNpcs();
          break;
        case NpcSystemStage.ContactEffect:
          DetectNpcContactDamage();
          break;
        case NpcSystemStage.DamageResolution:
          CommitNpcDamageCommands();
          break;
        case NpcSystemStage.Lifecycle:
          CommitNpcDespawnCommands();
          AdvanceNpcLifecycles();
          break;
        case NpcSystemStage.Death:
          RefreshNpcReplications();
          PublishNpcDeaths();
          break;
        case NpcSystemStage.Loot:
          CommitNpcLoot();
          break;
        case NpcSystemStage.Replication:
          RefreshNpcReplications();
          break;
        default:
          break;
      }
    });
  }

  private void PublishNpcDeaths()
  {
    foreach (KeyValuePair<NpcHandle, Entity> entry in _npcs)
    {
      if (_publishedNpcDeaths.Contains(entry.Key) ||
          !_npcReplications.TryGetValue(entry.Key, out NpcReplicationSnapshot replication) ||
          replication.IsActive)
      {
        continue;
      }

      HealthComponent health = World.Get<HealthComponent>(entry.Value);
      NpcLifecycleComponent lifecycle = World.Get<NpcLifecycleComponent>(entry.Value);
      if (lifecycle.DespawnReason != NpcDespawnReason.Killed)
      {
        continue;
      }

      NpcDefinitionComponent definitionComponent = World.Get<NpcDefinitionComponent>(entry.Value);
      if (!_npcDefinitions.TryGet(definitionComponent.DefinitionId, out NpcDefinition definition) ||
          !_npcLootSystem.IsRegistered(definition.LootTableId))
      {
        continue;
      }

      NpcDeathResult death = _npcDeathSystem.Evaluate(new NpcDeathInput(
        entry.Key,
        health.Current,
        WasActive: true,
        Position: replication.Position,
        LootTableId: definition.LootTableId));
      if (death.Published)
      {
        _pendingNpcDeaths.Add(death);
        _publishedNpcDeaths.Add(entry.Key);
      }
    }
  }

  private void CommitNpcLoot()
  {
    for (int index = 0; index < _pendingNpcDeaths.Count; index++)
    {
      NpcDeathResult death = _pendingNpcDeaths[index];
      SpawnNpcLoot(death);
    }

    _pendingNpcDeaths.Clear();
  }
}
