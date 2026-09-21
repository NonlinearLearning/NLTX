using System;

namespace Terraria.Dome.Simulation.Components;

public readonly record struct ProjectileSoundDelayComponent
{
  public ProjectileSoundDelayComponent(int remainingTicks = 0)
  {
    if (remainingTicks < -1)
    {
      throw new ArgumentOutOfRangeException(nameof(remainingTicks));
    }

    RemainingTicks = remainingTicks;
  }

  public int RemainingTicks { get; }

  public int SoundDelay => RemainingTicks;

  public bool IsSoundDelayed => RemainingTicks > 0;
}
