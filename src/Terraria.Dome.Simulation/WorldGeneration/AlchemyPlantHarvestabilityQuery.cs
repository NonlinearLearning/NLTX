using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class AlchemyPlantHarvestabilityQuery
{
  public static bool IsHarvestable(
    int style,
    int y,
    bool dayTime,
    bool bloodMoon,
    bool raining,
    float cloudAlpha,
    double time,
    int worldSurface,
    bool remixWorld,
    int maxTilesY,
    int moonPhase)
  {
    bool daytime = dayTime;
    return style switch
    {
      0 => daytime,
      1 => !daytime,
      3 => !daytime && (bloodMoon || moonPhase == 0),
      4 => raining || cloudAlpha > 0f,
      5 => HarvestableAtNight(y, raining, time, worldSurface, remixWorld, maxTilesY),
      _ => false
    };
  }

  private static bool HarvestableAtNight(
    int y,
    bool raining,
    double time,
    int worldSurface,
    bool remixWorld,
    int maxTilesY)
  {
    bool underground = remixWorld ? y < maxTilesY - 350 : y > worldSurface;
    return (!raining || underground) && time > 40500.0;
  }
}
