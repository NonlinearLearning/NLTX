using System.Numerics;

namespace Terraria.Player;

public readonly record struct PlayerSpatialSnapshot(
  Vector2 Position,
  Vector2 Velocity,
  int Width,
  int Height,
  bool MountActive,
  float MountPlayerOffsetHitbox,
  int MountHeightBoost,
  bool PortableStoolInUse,
  int PortableStoolHeightBoost,
  float PortableStoolVisualYOffset,
  float GfxOffY,
  bool Frozen,
  bool Webbed,
  bool Stoned);
