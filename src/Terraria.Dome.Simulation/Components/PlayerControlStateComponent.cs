using System;

namespace Terraria.Dome.Simulation.Components;

public struct PlayerControlStateComponent
{
  public int FireCooldownTicks;
  public int ItemUseCooldownTicks;

  public void ApplyAttackCooldown(int frames)
  {
    if (frames < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(frames));
    }

    if (FireCooldownTicks < frames)
    {
      FireCooldownTicks = frames;
    }
  }
}
