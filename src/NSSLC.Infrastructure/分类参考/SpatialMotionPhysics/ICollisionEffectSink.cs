using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public interface ICollisionEffectSink
{
  void OnTileCollision(TileHandle tile, Vector2 impactPoint);

  void OnEntityCollision(EntityReference entity, Vector2 impactPoint);
}
