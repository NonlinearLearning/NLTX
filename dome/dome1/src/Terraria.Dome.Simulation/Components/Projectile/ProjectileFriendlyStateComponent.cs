namespace Terraria.Dome.Simulation.Components;

public struct ProjectileFriendlyStateComponent
{
  public ProjectileFriendlyStateComponent(bool isFriendly)
  {
    IsFriendly = isFriendly;
  }

  public bool IsFriendly { get; set; }

  public bool Friendly
  {
    get => IsFriendly;
    set => IsFriendly = value;
  }
}
