using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcEyeOfCthulhuProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  NpcEyeOfCthulhuProfileState State,
  Vector2 Position,
  Vector2 Velocity,
  Vector2 TargetCenter,
  int Width,
  int Height,
  int Life,
  int LifeMax,
  bool TargetIsAvailable,
  bool TargetIsDead,
  bool DayTime,
  bool ExpertMode,
  bool GetGoodWorld,
  int DustRoll)
{
  public Vector2 TargetPosition { get; init; }
}
