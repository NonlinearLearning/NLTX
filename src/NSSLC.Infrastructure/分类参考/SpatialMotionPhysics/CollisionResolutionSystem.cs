namespace Terraria.SpatialMotionPhysics;

public static class CollisionResolutionSystem
{
  public static void Resolve(
    BallCollisionPayload payload,
    ICollisionEffectSink sink)
  {
    ArgumentNullException.ThrowIfNull(sink);
    sink.OnTileCollision(payload.Tile, payload.ImpactPoint);
    sink.OnEntityCollision(payload.Entity, payload.ImpactPoint);
  }
}
