using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcFighterProfileResult(
  Vector2 Velocity,
  NpcFighterProfileState State,
  int Direction,
  int DirectionY,
  int AiAction,
  bool DespawnEncouragementRequested,
  bool TargetClosestRequested,
  bool NetUpdateRequested,
  NpcFighterSourceBranch Branches,
  bool JumpRequested,
  float JumpVelocityY,
  bool DoorOpenRequested,
  int DoorTileX,
  int DoorTileY,
  int DoorDirection);
