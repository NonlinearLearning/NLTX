using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.Npc.Components;
using Terraria.Dome.Simulation.Npc.Definitions;

namespace Terraria.Dome.Simulation;

public readonly record struct NpcReplicationSnapshot(
  int ReplicationId,
  int NpcType,
  SimulationVector Position,
  SimulationVector Velocity,
  int Health,
  bool IsActive,
  long Revision,
  WorldSectionCoordinates Section,
  int DefinitionId = 0,
  int MaximumHealth = 0,
  int Facing = -1,
  int TargetStableId = 0,
  bool HasTarget = false,
  NpcBehaviorId BehaviorId = NpcBehaviorId.OrdinaryChase,
  NpcSpawnSource SpawnSource = NpcSpawnSource.Command,
  float DifficultyScale = 1.0f,
  int ReleaseOwner = 0,
  bool SpawnedFromStatue = false,
  int TimeLeft = 0,
  NpcDespawnReason DespawnReason = NpcDespawnReason.None,
  bool HasHome = false,
  NpcHomeComponent Home = default,
  bool HasSegment = false,
  NpcSegmentComponent Segment = default);
