using System;

namespace Terraria.Dome.Simulation.Wiring.Components;

public readonly record struct PressurePlateComponent
{
  public PressurePlateComponent(int mechanismId, int x, int y, int radius, bool requiresPlayer)
  {
    if (mechanismId <= 0 || radius < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(mechanismId));
    }

    MechanismId = mechanismId;
    X = x;
    Y = y;
    Radius = radius;
    RequiresPlayer = requiresPlayer;
  }

  public int MechanismId { get; }
  public int X { get; }
  public int Y { get; }
  public int Radius { get; }
  public bool RequiresPlayer { get; }
}
