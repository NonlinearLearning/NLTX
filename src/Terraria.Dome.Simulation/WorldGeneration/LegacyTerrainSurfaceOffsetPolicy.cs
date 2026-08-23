using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTerrainSurfaceOffsetPolicy
{
  public static double Next(
    LegacyPassRandomState random,
    LegacyTerrainFeatureKind feature,
    bool specialWorld)
  {
    ArgumentNullException.ThrowIfNull(random);
    return feature switch
    {
      LegacyTerrainFeatureKind.Plateau => NextPlateau(random, specialWorld),
      LegacyTerrainFeatureKind.Hill => NextHill(random, specialWorld),
      LegacyTerrainFeatureKind.Dale => NextDale(random, specialWorld),
      LegacyTerrainFeatureKind.Mountain => NextMountain(random, specialWorld),
      LegacyTerrainFeatureKind.Valley => NextValley(random, specialWorld),
      _ => throw new ArgumentOutOfRangeException(nameof(feature))
    };
  }

  private static double NextPlateau(LegacyPassRandomState random, bool specialWorld)
  {
    double offset = 0.0;
    int chance = specialWorld ? 6 : 7;
    while (random.Next(chance) == 0)
    {
      offset += random.Next(-1, 2);
    }

    return offset;
  }

  private static double NextHill(LegacyPassRandomState random, bool specialWorld)
  {
    double offset = 0.0;
    int declineChance = specialWorld ? 3 : 4;
    while (random.Next(declineChance) == 0)
    {
      offset -= 1.0;
    }

    while (random.Next(10) == 0)
    {
      offset += 1.0;
    }

    return offset;
  }

  private static double NextDale(LegacyPassRandomState random, bool specialWorld)
  {
    double offset = 0.0;
    int inclineChance = specialWorld ? 3 : 4;
    while (random.Next(inclineChance) == 0)
    {
      offset += 1.0;
    }

    while (random.Next(10) == 0)
    {
      offset -= 1.0;
    }

    return offset;
  }

  private static double NextMountain(LegacyPassRandomState random, bool specialWorld)
  {
    double offset = 0.0;
    int declineChance = specialWorld ? 3 : 2;
    while (random.Next(declineChance) == 0)
    {
      offset -= 1.0;
    }

    while (random.Next(6) == 0)
    {
      offset += 1.0;
    }

    return offset;
  }

  private static double NextValley(LegacyPassRandomState random, bool specialWorld)
  {
    double offset = 0.0;
    int inclineChance = specialWorld ? 3 : 2;
    while (random.Next(inclineChance) == 0)
    {
      offset += 1.0;
    }

    while (random.Next(5) == 0)
    {
      offset -= 1.0;
    }

    return offset;
  }
}
