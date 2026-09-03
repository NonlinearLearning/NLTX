using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerSleepComponent
{
  public const int TimeToFullyFallAsleep = 120;

  public bool IsSleeping { get; private set; }

  public int TimeSleeping { get; private set; }

  public PlayerSleepWakeReason LastWakeReason { get; private set; }

  public bool FullyFallenAsleep => IsSleeping && TimeSleeping >= TimeToFullyFallAsleep;

  public bool StartSleeping()
  {
    if (IsSleeping)
    {
      return false;
    }

    IsSleeping = true;
    TimeSleeping = 0;
    LastWakeReason = PlayerSleepWakeReason.None;
    return true;
  }

  public bool StopSleeping(PlayerSleepWakeReason reason)
  {
    if (!Enum.IsDefined(reason) || reason == PlayerSleepWakeReason.None)
    {
      throw new ArgumentOutOfRangeException(nameof(reason));
    }

    if (!IsSleeping)
    {
      return false;
    }

    IsSleeping = false;
    TimeSleeping = 0;
    LastWakeReason = reason;
    return true;
  }

  public void Advance()
  {
    if (!IsSleeping)
    {
      TimeSleeping = 0;
      return;
    }

    if (TimeSleeping < TimeToFullyFallAsleep)
    {
      TimeSleeping++;
    }
  }
}
