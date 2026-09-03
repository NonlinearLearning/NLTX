using System;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TileEntityIdentityComponent(int EntityId, byte Type)
{
  public TileEntityIdentityComponent Validate()
  {
    if (EntityId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(EntityId));
    }

    return this;
  }
}
