using System;

namespace Terraria.Dome.Simulation.WorldObjects;

public readonly record struct ChestLockDefinition(ushort KeyItemType, bool ConsumesKey)
{
  public static ChestLockDefinition GoldKey { get; } = new(327, true);
  public static ChestLockDefinition ShadowKey { get; } = new(329, false);

  public void Validate()
  {
    if (KeyItemType == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(KeyItemType));
    }
  }
}
