using System;

namespace Terraria.Dome.Simulation.Combat.Components;

public struct DefenseComponent
{
  public DefenseComponent(int value)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public int Value;
}
