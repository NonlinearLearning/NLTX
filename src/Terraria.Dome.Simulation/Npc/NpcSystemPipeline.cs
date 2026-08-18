using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Npc;

public enum NpcSystemStage
{
  SpawnEligibility,
  SpawnCommit,
  TargetSelection,
  Behavior,
  MovementIntent,
  MovementAndCollision,
  ContactEffect,
  DamageResolution,
  Lifecycle,
  Death,
  Loot,
  Replication
}

public readonly record struct NpcSystemRegistration(NpcSystemStage Stage, string Name);

public sealed class NpcSystemPipeline
{
  private static readonly IReadOnlyList<NpcSystemRegistration> _registrations =
  [
    new(NpcSystemStage.SpawnEligibility, "NpcSpawnEligibilitySystem"),
    new(NpcSystemStage.SpawnCommit, "NpcSpawnCommitSystem"),
    new(NpcSystemStage.TargetSelection, "NpcTargetSelectionSystem"),
    new(NpcSystemStage.Behavior, "NpcBehaviorSystem"),
    new(NpcSystemStage.MovementIntent, "NpcMovementIntentSystem"),
    new(NpcSystemStage.MovementAndCollision, "MovementSystem/TileCollisionSystem"),
    new(NpcSystemStage.ContactEffect, "NpcContactEffectSystem"),
    new(NpcSystemStage.DamageResolution, "DamageResolutionSystem"),
    new(NpcSystemStage.Lifecycle, "NpcLifecycleSystem"),
    new(NpcSystemStage.Death, "NpcDeathSystem"),
    new(NpcSystemStage.Loot, "NpcLootSystem"),
    new(NpcSystemStage.Replication, "NpcReplicationSystem")
  ];

  public IReadOnlyList<string> SystemNames
  {
    get
    {
      List<string> names = new(_registrations.Count);
      for (int index = 0; index < _registrations.Count; index++)
      {
        names.Add(_registrations[index].Name);
      }

      return names;
    }
  }

  public void Execute(Action<NpcSystemRegistration> executeStage)
  {
    ExecuteRange(NpcSystemStage.SpawnEligibility, NpcSystemStage.Replication, executeStage);
  }

  public void ExecuteRange(
    NpcSystemStage firstStage,
    NpcSystemStage lastStage,
    Action<NpcSystemRegistration> executeStage)
  {
    ArgumentNullException.ThrowIfNull(executeStage);
    int firstIndex = (int)firstStage;
    int lastIndex = (int)lastStage;
    if (firstIndex < 0 || lastIndex >= _registrations.Count || firstIndex > lastIndex)
    {
      throw new ArgumentOutOfRangeException(nameof(firstStage));
    }

    for (int index = firstIndex; index <= lastIndex; index++)
    {
      executeStage.Invoke(_registrations[index]);
    }
  }

  public void ValidateRegistration()
  {
    if (_registrations.Count != 12)
    {
      throw new InvalidOperationException("NPC tick pipeline must have exactly twelve stages.");
    }
  }
}
