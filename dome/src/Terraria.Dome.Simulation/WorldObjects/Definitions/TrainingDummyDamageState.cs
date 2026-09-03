using System;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyDamageState(int LastDamage, long HitCount)
{
  public TrainingDummyDamageState Apply(int damage)
  {
    if (damage < 0 || HitCount == long.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(damage));
    }

    return new TrainingDummyDamageState(damage, HitCount + 1);
  }
}
