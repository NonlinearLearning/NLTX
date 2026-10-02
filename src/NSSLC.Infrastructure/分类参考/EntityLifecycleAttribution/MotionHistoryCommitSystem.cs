namespace Terraria.EntityLifecycleAttribution;

public sealed class MotionHistoryCommitSystem
{
  public void Capture(
    LocationComponent location,
    VelocityComponent velocity,
    DirectionComponent direction,
    MotionHistoryComponent history)
  {
    ArgumentNullException.ThrowIfNull(location);
    ArgumentNullException.ThrowIfNull(velocity);
    ArgumentNullException.ThrowIfNull(direction);
    ArgumentNullException.ThrowIfNull(history);

    history.Capture(location.Position, velocity.Velocity, direction.Direction);
  }
}
