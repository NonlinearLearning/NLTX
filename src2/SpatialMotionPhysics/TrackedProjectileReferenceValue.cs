namespace Terraria.SpatialMotionPhysics;

public readonly record struct TrackedProjectileReferenceValue(
  int ProjectileLocalIndex,
  int ProjectileOwnerIndex,
  int ProjectileIdentity,
  int ProjectileType)
{
  public static TrackedProjectileReferenceValue Create(
    int localIndex,
    int ownerIndex,
    int identity,
    int type)
  {
    return new TrackedProjectileReferenceValue(
      localIndex,
      ownerIndex,
      identity,
      type);
  }
}
