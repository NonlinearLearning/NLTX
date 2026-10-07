namespace Terraria.Projectile;

public readonly record struct ProjectileNpcDamageCollisionResult(
  ProjectileNpcDamageCandidateStatus CandidateStatus,
  ProjectileCollidingGeometryResult GeometryResult)
{
  public bool CollisionFound =>
    CandidateStatus == ProjectileNpcDamageCandidateStatus.ReadyForCollisionTest &&
    GeometryResult == ProjectileCollidingGeometryResult.Collision;
}
