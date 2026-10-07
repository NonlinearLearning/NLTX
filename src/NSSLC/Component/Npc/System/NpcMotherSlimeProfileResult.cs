using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcMotherSlimeProfileResult(
  Vector2 Position,
  Vector2 Velocity,
  NpcMotherSlimeProfileState State,
  int Direction,
  int AiAction,
  bool ContainedItemSelectionRejected,
  bool NetUpdateRequested,
  bool TargetClosestRequested,
  bool IsFrozen,
  NpcMotherSlimeSourceBranch Branches);
