namespace Terraria.SpatialMotionPhysics;

public static class TeleportCommitSystem
{
  public static void Apply(
    EntityMotionState motion,
    TeleportCommitCommand command)
  {
    ArgumentNullException.ThrowIfNull(motion);
    motion.Position = command.Position;
    motion.Velocity = command.Velocity;
  }
}
