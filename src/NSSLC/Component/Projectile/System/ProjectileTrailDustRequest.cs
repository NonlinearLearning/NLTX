using System.Numerics;

namespace Terraria.Projectile;

public readonly record struct ProjectileTrailDustRequest(
  int ProjectileType,
  Vector2 Position,
  float Rotation);
