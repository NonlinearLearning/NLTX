using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcFighterProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  Vector2 Position,
  Vector2 Velocity,
  NpcFighterProfileState State,
  int Direction,
  int TargetSlot,
  float Scale,
  bool DayTime,
  bool IsBelowSurface,
  bool IsGrounded,
  bool JustHit,
  NpcFighterTraversalInput Traversal = default,
  Vector2 TargetCenter = default,
  int TargetHeight = 0,
  bool TargetIsAvailable = false,
  int Height = 0,
  int DirectionY = 1);
