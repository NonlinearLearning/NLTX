namespace Terraria.Projectile;

/// <summary>
/// Computed owner-dependent spear extension hitbox for AI style 19.
/// </summary>
public readonly record struct ProjectileAi19ExtensionSnapshot(
  bool HasResults,
  bool HasExtensionHitbox,
  ProjectileCollisionTargetRectangle ExtensionHitbox);
