using Terraria.Dome.Simulation.Components;

namespace Terraria.Dome.Simulation.Projectile.Systems;

public sealed class ProjectileSoundDelayPolicy
{
  public bool IsDelayed(ProjectileSoundDelayComponent delay)
  {
    return delay.RemainingTicks > 0;
  }

  public ProjectileSoundDelayComponent Tick(ProjectileSoundDelayComponent delay)
  {
    return delay.RemainingTicks > 0
      ? new ProjectileSoundDelayComponent(delay.RemainingTicks - 1)
      : delay;
  }
}
