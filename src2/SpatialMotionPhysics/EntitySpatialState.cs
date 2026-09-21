using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class EntitySpatialState
{
  public EntitySpatialState(Vector2 position, EntityBoundsComponent bounds)
  {
    Position = position;
    Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
  }

  public EntityBoundsComponent Bounds { get; }

  public Vector2 Position { get; internal set; }
}
