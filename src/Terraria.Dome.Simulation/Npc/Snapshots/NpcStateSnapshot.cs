using System;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Simulation.Npc.Snapshots;

public readonly record struct NpcStateSnapshot(
  NpcReplicationSnapshot Replication,
  int DefinitionId,
  int NetId,
  int MaximumHealth,
  int Facing,
  int TargetStableId,
  bool HasTarget,
  NpcBehaviorStateComponent Behavior,
  NpcSpawnStateComponent Spawn,
  NpcLifecycleComponent Lifecycle,
  bool HasHome,
  NpcHomeComponent Home,
  bool HasSegment,
  NpcSegmentComponent Segment,
  NpcFaction Faction = NpcFaction.Hostile,
  NpcCategory Category = NpcCategory.Enemy)
{
  private const int DefaultTimeLeft = 750;

  public NpcReplicationSnapshot ToReplicationSnapshot()
  {
    return Replication with
    {
      DefinitionId = DefinitionId,
      MaximumHealth = MaximumHealth,
      Facing = Facing,
      TargetStableId = TargetStableId,
      HasTarget = HasTarget,
      BehaviorId = Behavior.BehaviorId,
      SpawnSource = Spawn.Source,
      DifficultyScale = Spawn.DifficultyScale,
      ReleaseOwner = Spawn.ReleaseOwner,
      SpawnedFromStatue = Spawn.SpawnedFromStatue,
      TimeLeft = Lifecycle.TimeLeft,
      DespawnReason = Lifecycle.DespawnReason,
      HasHome = HasHome,
      Home = Home,
      HasSegment = HasSegment,
      Segment = Segment
    };
  }

  public static NpcStateSnapshot FromReplication(NpcReplicationSnapshot snapshot)
  {
    if (snapshot.MaximumHealth <= 0 && snapshot.Health == int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(
        nameof(snapshot),
        "NPC health cannot derive a representable maximum health value.");
    }

    int definitionId = snapshot.DefinitionId > 0 ? snapshot.DefinitionId : snapshot.NpcType;
    int maximumHealth = snapshot.MaximumHealth > 0
      ? snapshot.MaximumHealth
      : Math.Max(1, snapshot.Health + 1);
    NpcBehaviorStateComponent behavior = new(
      snapshot.BehaviorId,
      new NpcChaseState(1.0f, 0.0f),
      new NpcTownHomeState(default, true, 0));
    NpcSpawnStateComponent spawn = new(
      snapshot.SpawnSource,
      snapshot.DifficultyScale > 0.0f ? snapshot.DifficultyScale : 1.0f,
      snapshot.ReleaseOwner,
      snapshot.SpawnedFromStatue);
    int timeLeft = snapshot.IsActive && snapshot.TimeLeft <= 0
      ? DefaultTimeLeft
      : snapshot.TimeLeft;
    NpcLifecycleComponent lifecycle = new(snapshot.IsActive, timeLeft);
    lifecycle.DespawnReason = snapshot.DespawnReason;
    return new NpcStateSnapshot(
      snapshot,
      definitionId,
      snapshot.NpcType,
      maximumHealth,
      snapshot.Facing,
      snapshot.TargetStableId,
      snapshot.HasTarget,
      behavior,
      spawn,
      lifecycle,
      snapshot.HasHome,
      snapshot.Home,
      snapshot.HasSegment,
      snapshot.Segment);
  }
}
