namespace Terraria.Projectile;

/// <summary>Declares projectile collision participation and owner exclusion policy.</summary>
public readonly record struct ProjectileCollisionPolicyComponent(
    bool CollideWithTiles,
    bool CollideWithEntities,
    bool IgnoreOwner,
    bool IgnoreSameFaction)
{
}
