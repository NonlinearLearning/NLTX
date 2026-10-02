using System;

namespace Terraria.WorldSession.Environment.Ritual;

public sealed class CultistRitualStateComponent
{
  public CultistRitualStateComponent(int delayTicks = 0, int recheckTicks = 0)
  {
    DelayTicks = delayTicks;
    RecheckTicks = recheckTicks;
    Validate();
  }

  public int DelayTicks { get; internal set; }

  public int RecheckTicks { get; internal set; }

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(DelayTicks);
    ArgumentOutOfRangeException.ThrowIfNegative(RecheckTicks);
  }
}
