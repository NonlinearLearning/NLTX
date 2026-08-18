using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public sealed class ChestRevisionComponent
{
  public ChestRevisionComponent(long value = 1)
  {
    if (value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    Value = value;
  }

  public long Value { get; private set; }

  public void Increment()
  {
    Value = checked(Value + 1);
  }
}
