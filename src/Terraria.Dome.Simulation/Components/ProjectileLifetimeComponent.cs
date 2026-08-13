namespace Terraria.Dome.Simulation.Components;

public struct ProjectileLifetimeComponent
{
  public ProjectileLifetimeComponent(int remainingTicks)
  {
    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks;
}
