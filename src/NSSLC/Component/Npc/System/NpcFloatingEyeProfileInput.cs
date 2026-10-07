using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcFloatingEyeProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  Vector2 Position,
  Vector2 Velocity,
  Vector2 OldVelocity,
  int Width,
  int Height,
  float Scale,
  int Direction,
  int DirectionY,
  int TargetSlot,
  bool NoTileCollide,
  bool CollideX,
  bool CollideY,
  bool DayTime,
  bool ZoneGraveyard,
  float WorldSurfacePixels,
  bool Wet,
  int? DustRoll);
