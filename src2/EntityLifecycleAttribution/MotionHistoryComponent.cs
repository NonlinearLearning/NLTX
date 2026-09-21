using System.Numerics;

namespace Terraria.EntityLifecycleAttribution;

public sealed class MotionHistoryComponent
{
  public Vector2 PreviousPosition { get; private set; }

  public Vector2 PreviousVelocity { get; private set; }

  public int PreviousDirection { get; private set; } = 1;

  internal void Capture(Vector2 position, Vector2 velocity, int direction)
  {
    PreviousPosition = position;
    PreviousVelocity = velocity;
    PreviousDirection = direction;
  }
}
