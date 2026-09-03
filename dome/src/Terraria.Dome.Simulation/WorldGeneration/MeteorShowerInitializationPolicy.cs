using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class MeteorShowerInitializationPolicy
{
  private const int MaximumInitialRollExclusive = 751;
  private const int MinimumInitialRoll = 650;
  private const int ShowerCountMultiplier = 4;

  public static MeteorShowerProgression Initialize(LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    int initialRoll = random.Next(MinimumInitialRoll, MaximumInitialRollExclusive);
    return new MeteorShowerProgression(initialRoll * ShowerCountMultiplier);
  }
}
