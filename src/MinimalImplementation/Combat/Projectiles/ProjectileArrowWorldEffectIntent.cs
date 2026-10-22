using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Explicit P03/P06 world-effect request produced after Arrow movement.</summary>
public sealed record ProjectileArrowWorldEffectIntent(
    CombatEntityIdentity Projectile,
    long ProjectileRevision,
    CommitVersion ProjectileCommitVersion,
    bool SwitchTilesChanged,
    int SwitchTilesObjectType,
    bool CutTilesApplied)
{
  public bool HasEffect => SwitchTilesChanged || CutTilesApplied;
}
