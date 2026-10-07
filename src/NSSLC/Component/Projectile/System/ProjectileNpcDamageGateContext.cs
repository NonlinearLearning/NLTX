namespace Terraria.Projectile;

/// <summary>
/// External snapshots needed to evaluate Version4 projectile-to-NPC gates.
/// Immunity flags are true when the corresponding cooldown currently blocks
/// this target. The registry-backed facade overload reads ordinary projectile
/// local immunity from its component and static immunity from the target slot
/// and game update count. Typed-input callers provide the local-immunity value
/// directly. For types 626-628, HasLocalNpcImmunity must reflect the array
/// selected by Version4: the shared Stardust Dragon head array when found,
/// otherwise the projectile's own local array.
/// Owner facts map Main/player checks at the call
/// site: local ownership maps owner == Main.myPlayer, melee cooldown maps
/// CanHitNPCWithMeleeHit, target permission maps
/// CanNPCBeHitByPlayerOrPlayerProjectile, and the hit-check result maps
/// CanHitWithMeleeWeapon. Pet and guide/clothier flags map Main.projPet[type],
/// Player.killGuide, and Player.killClothier.
/// </summary>
public readonly record struct ProjectileNpcDamageGateContext(
  bool IsProjectilePet,
  bool IsDamageOwnerLocalPlayer,
  bool HasLocalNpcImmunity,
  bool HasStaticNpcImmunity,
  bool OwnerMeleeHitCooldownAllowsTarget,
  bool OwnerDamageRulesAllowTarget,
  bool OwnerHitCheckAllowsTarget,
  bool OwnerCanDamageGuide,
  bool OwnerCanDamageClothier);
