namespace Terraria.Projectile;

/// <summary>
/// Owner Player values read by Version4 AI_019_Spears_GetExtensionHitbox.
/// </summary>
public readonly record struct ProjectileAi19OwnerSnapshot(
  bool HasResults,
  int ItemAnimation,
  int ItemAnimationMax,
  float MeleeSpeed);
