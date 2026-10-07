using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcMotherSlimeProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  Vector2 Position,
  Vector2 Velocity,
  NpcMotherSlimeProfileState State,
  int Direction,
  int TargetSlot,
  bool DayTime,
  bool IsDamaged,
  bool IsBelowSurface,
  bool SlimeRain,
  bool Wet,
  bool CollideX,
  bool CollideY,
  Vector2 OldVelocity,
  bool SolidCollision);
