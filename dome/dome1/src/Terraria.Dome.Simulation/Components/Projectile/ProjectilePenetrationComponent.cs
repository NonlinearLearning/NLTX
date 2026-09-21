using System;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectilePenetrationComponent
{
  public ProjectilePenetrationComponent(int maximumPenetration)
  {
    if (maximumPenetration == 0 || maximumPenetration < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumPenetration));
    }

    MaximumPenetration = maximumPenetration;
    RemainingPenetration = maximumPenetration;
  }

  public int MaximumPenetration { get; }
  public int RemainingPenetration { get; set; }

  public int Penetrate
  {
    get => RemainingPenetration;
    set => RemainingPenetration = value;
  }

  public int MaxPenetrate => MaximumPenetration;
}
