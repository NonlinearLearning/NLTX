using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileRestrikeDelaySystem
{
  public bool IsBlocked(ProjectileRestrikeDelayComponent delay)
  {
    return delay.RemainingTicks > 0;
  }

  public ProjectileRestrikeDelayComponent Tick(ProjectileRestrikeDelayComponent delay)
  {
    return delay.RemainingTicks <= 1
      ? new ProjectileRestrikeDelayComponent()
      : new ProjectileRestrikeDelayComponent(delay.RemainingTicks - 1);
  }
}
