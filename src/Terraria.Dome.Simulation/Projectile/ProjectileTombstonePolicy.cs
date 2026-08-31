namespace Terraria.Dome.Simulation.Projectile;

public static class ProjectileTombstonePolicy
{
  public const long RetentionTicks = 30;

  public static long CalculateRetentionUntil(long tombstoneTick)
  {
    if (tombstoneTick < 0 || tombstoneTick > long.MaxValue - RetentionTicks)
    {
      return long.MaxValue;
    }

    return tombstoneTick + RetentionTicks;
  }

  public static bool IsRetained(ProjectileReplicationSnapshot snapshot, long currentTick)
  {
    return !snapshot.IsActive && currentTick < snapshot.TombstoneRetainedUntilTick;
  }
}
