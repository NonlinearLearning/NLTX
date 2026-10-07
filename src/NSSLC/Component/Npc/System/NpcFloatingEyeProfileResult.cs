using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcFloatingEyeProfileResult(
  Vector2 Velocity,
  int Direction,
  int DirectionY,
  bool NoGravity,
  bool NoTileCollide,
  bool DespawnEncouragementRequested,
  bool TargetClosestRequested,
  bool WetTargetClosestRequested,
  bool DustRequested,
  NpcFloatingEyeSourceBranch Branches);
