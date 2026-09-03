using System;

namespace Terraria.Dome.Simulation.Components;

public struct ProjectileUpdateCountComponent
{
  public int Count;

  public int NumUpdates
  {
    get => Count;
    set
    {
      if (value < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      Count = value;
    }
  }

  public int Updates => Count;

  public void Advance()
  {
    if (Count < int.MaxValue)
    {
      Count++;
    }
  }
}
