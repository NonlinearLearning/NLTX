using System;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyIdentityComponent(int EntityId)
{
  public TrainingDummyIdentityComponent Validate()
  {
    if (EntityId <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(EntityId));
    }

    return this;
  }
}
