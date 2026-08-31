using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class WorldEvilSelectionPolicy
{
  public static WorldEvilSelection Select(int worldGenParamEvil, LegacyPassRandomState random)
  {
    ArgumentNullException.ThrowIfNull(random);
    if (worldGenParamEvil is < -1 or > 1)
    {
      throw new ArgumentOutOfRangeException(nameof(worldGenParamEvil));
    }

    bool crimson = random.Next(2) == 0;
    bool generatingRandomEvil = true;
    if (worldGenParamEvil == 0)
    {
      generatingRandomEvil = false;
      crimson = false;
    }
    else if (worldGenParamEvil == 1)
    {
      generatingRandomEvil = false;
      crimson = true;
    }

    return new WorldEvilSelection(crimson, generatingRandomEvil);
  }
}
