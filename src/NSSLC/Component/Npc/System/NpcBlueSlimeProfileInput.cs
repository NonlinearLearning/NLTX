using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcBlueSlimeProfileInput(
  Vector2 Position,
  Vector2 Velocity,
  NpcBlueSlimeProfileState State,
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
  float Gravity,
  bool IsClient,
  bool CanContainItems,
  float Value,
  int BaseDefense);
