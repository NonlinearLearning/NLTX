namespace Terraria.Projectile;

/// <summary>
/// External snapshots needed to evaluate Version4 projectile PVP gates.
/// Ownership and local Player facts map owner == Main.myPlayer,
/// Main.player[Main.myPlayer].hostile, and that Player's team. The hit-check
/// flag maps CanHitWithMeleeWeapon(target); the pet flag maps
/// Main.projPet[projectile.type].
/// </summary>
public readonly record struct ProjectilePvpDamageGateContext(
  bool IsProjectilePet,
  bool IsDamageOwnerLocalPlayer,
  bool LocalDamageOwnerIsHostile,
  int LocalDamageOwnerTeam,
  bool OwnerHitCheckAllowsTarget)
{
  /// <summary>
  /// Whether this projectile's per-player immunity blocks the selected target.
  /// The compatibility facade fills this from the projectile component.
  /// </summary>
  public bool ProjectileIsImmune { get; init; }
}
