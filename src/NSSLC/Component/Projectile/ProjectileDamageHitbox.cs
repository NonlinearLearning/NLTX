namespace Terraria.Projectile;

/// <summary>
/// Integer projectile hitbox passed to NPC and Player collision adapters.
/// Non-positive dimensions are preserved because legacy Rectangle inflation
/// can shrink a hitbox below zero.
/// </summary>
public readonly record struct ProjectileDamageHitbox(
  int X,
  int Y,
  int Width,
  int Height);
