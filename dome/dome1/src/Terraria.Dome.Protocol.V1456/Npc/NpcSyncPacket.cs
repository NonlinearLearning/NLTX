using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Npc;

public readonly record struct NpcSyncPacket(
  short Identity,
  SimulationVector Position,
  SimulationVector Velocity,
  ushort Target,
  bool DirectionRight,
  bool DirectionYDown,
  float? Ai0,
  float? Ai1,
  float? Ai2,
  float? Ai3,
  bool SpriteDirectionRight,
  short NpcType,
  byte PlayersForScaling,
  bool SpawnedFromStatue,
  float Difficulty,
  bool SpawnNeedsSyncing,
  float ShimmerTransparency,
  int Life,
  int LifeMaximum,
  byte ReleaseOwner,
  bool IsCatchable,
  long Revision);
