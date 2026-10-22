using MinimalImplementation.Combat.Identity;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Required local-player follow-up for NewProjectile's TryUpdateChannel call. The owner
/// applies it once after any packet-27 broadcast returned by the same spawn commit.
/// </summary>
public sealed record ProjectileChannelTrackingIntent(
    CombatEntityIdentity Projectile,
    byte Owner,
    int RuntimeSlot);
