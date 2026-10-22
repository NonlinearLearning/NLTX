using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>
/// Trusted player facts for one Arrow update, never packet input. G2 captures MagicQuiver
/// from the actual owner slot, including slot 255, and guards that player snapshot through
/// movement commit. These fields bind the facts to the projectile generation and revision;
/// they do not replace the external player's revision guard.
/// </summary>
public sealed record ProjectileArrowOwnerFacts(
    CombatEntityIdentity Entity,
    long SourceProjectileRevision,
    byte Owner,
    SessionEpoch? OwnerEpoch,
    bool MagicQuiver);
