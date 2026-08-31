using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerPotionStateComponent
{
  public const ushort PotionSicknessBuffType = 21;

  public int PotionDelayTicks;

  public readonly bool IsBlocked => PotionDelayTicks > 0;

  public void Apply(int delayTicks)
  {
    if (delayTicks <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(delayTicks));
    }

    PotionDelayTicks = delayTicks;
  }

  public void Tick()
  {
    if (PotionDelayTicks < 0)
    {
      PotionDelayTicks = 0;
      return;
    }

    if (PotionDelayTicks > 0)
    {
      PotionDelayTicks--;
    }
  }
}
