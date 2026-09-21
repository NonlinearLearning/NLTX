using System.Numerics;

namespace Terraria.SpatialMotionPhysics;

public sealed class EntityMotionState
{
  public EntityMotionState(Vector2 position, Vector2 velocity)
  {
    Position = position;
    Velocity = velocity;
  }

  public Vector2 Position { get; internal set; }

  public Vector2 Velocity { get; internal set; }
}
