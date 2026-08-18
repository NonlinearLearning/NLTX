namespace Terraria.Dome.Simulation.Components;

public struct ProjectilePenetrationComponent
{
  public ProjectilePenetrationComponent(int maximumPenetration)
  {
    MaximumPenetration = maximumPenetration;
    RemainingPenetration = maximumPenetration;
  }

  public int MaximumPenetration { get; }
  public int RemainingPenetration { get; set; }
}
