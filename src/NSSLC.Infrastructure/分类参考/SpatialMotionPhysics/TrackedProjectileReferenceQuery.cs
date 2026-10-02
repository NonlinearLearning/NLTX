namespace Terraria.SpatialMotionPhysics;

public static class TrackedProjectileReferenceQuery
{
  public static bool IsTracking(TrackedProjectileReferenceValue reference)
  {
    return reference.ProjectileLocalIndex >= 0 &&
      reference.ProjectileOwnerIndex >= 0 &&
      reference.ProjectileIdentity >= 0 &&
      reference.ProjectileType >= 0;
  }
}
