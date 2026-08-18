using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LiquidDefinition
{
  public LiquidDefinition(string id, byte type, byte maxAmount)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(id);
    if (maxAmount == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maxAmount));
    }

    Id = id;
    Type = type;
    MaxAmount = maxAmount;
  }

  public string Id { get; }
  public byte Type { get; }
  public byte MaxAmount { get; }
}
