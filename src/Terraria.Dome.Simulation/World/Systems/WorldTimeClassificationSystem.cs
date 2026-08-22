using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public sealed class WorldTimeClassificationSystem
{
  public WorldMoonPhase GetMoonPhase(int moonPhase)
  {
    if (moonPhase < (int)WorldMoonPhase.Full ||
        moonPhase > (int)WorldMoonPhase.ThreeQuartersAtRight)
    {
      throw new ArgumentOutOfRangeException(nameof(moonPhase));
    }

    return (WorldMoonPhase)moonPhase;
  }

  public bool IsGameplayDayTime(bool isDayTime, bool isRemixWorld)
  {
    return !isRemixWorld && isDayTime;
  }
}
