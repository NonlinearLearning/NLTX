using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class SignRevisionComponent
{
  public SignRevisionComponent(long value = 1)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public long Value { get; private set; }

  public bool TryIncrement()
  {
    if (Value == long.MaxValue)
    {
      return false;
    }

    Value++;
    return true;
  }
}
