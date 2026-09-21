using System.Numerics;

namespace Terraria.EntityLifecycleAttribution;

public sealed class VelocityComponent
{
  public VelocityComponent(Vector2 velocity)
  {
    Velocity = velocity;
  }

  public Vector2 Velocity { get; private set; }

  public void SetVelocity(Vector2 velocity)
  {
    Velocity = velocity;
  }
}
