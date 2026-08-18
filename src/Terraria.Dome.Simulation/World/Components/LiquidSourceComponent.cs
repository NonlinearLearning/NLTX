using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LiquidSourceComponent
{
  public LiquidSourceComponent(int x, int y, byte liquidType, byte amount, string source)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(source);
    if (amount == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    X = x;
    Y = y;
    LiquidType = liquidType;
    Amount = amount;
    Source = source;
  }

  public int X { get; }
  public int Y { get; }
  public byte LiquidType { get; }
  public byte Amount { get; }
  public string Source { get; }
}
