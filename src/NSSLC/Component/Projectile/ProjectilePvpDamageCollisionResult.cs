namespace Terraria.Projectile;

public readonly record struct ProjectilePvpDamageCollisionResult(
  ProjectilePvpDamageCandidateStatus CandidateStatus,
  ProjectileCollidingGeometryResult GeometryResult)
{
  public bool CollisionFound =>
    CandidateStatus == ProjectilePvpDamageCandidateStatus.ReadyForCollisionTest &&
    GeometryResult == ProjectileCollidingGeometryResult.Collision;
}
