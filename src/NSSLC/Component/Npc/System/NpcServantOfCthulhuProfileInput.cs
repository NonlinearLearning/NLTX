using System.Numerics;

namespace Terraria.Npc;

public readonly record struct NpcServantOfCthulhuProfileInput(
  int TypeId,
  int NetId,
  int AiStyle,
  Vector2 Position,
  Vector2 Velocity,
  Vector2 TargetCenter,
  int Width,
  int Height);
