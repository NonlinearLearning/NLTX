namespace Terraria.Dome.Simulation.Components;

public struct ProjectileFriendlyStateComponent
{
  public ProjectileFriendlyStateComponent(bool isFriendly)
  {
    IsFriendly = isFriendly;
  }

  public bool IsFriendly { get; set; }
}
