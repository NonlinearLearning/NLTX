using System;

namespace Terraria.Dome.Simulation.Wiring.Components;

public sealed class MechanismRetryState
{
  public int Attempts { get; private set; }
  public int CooldownTicks { get; private set; }
  public bool IsAborted { get; private set; }

  public bool TrySchedule(MechanismRetryPolicy policy, int cooldownTicks, int maximumAttempts)
  {
    if (cooldownTicks < 0 || maximumAttempts <= 0 || IsAborted)
    {
      return false;
    }

    if (policy == MechanismRetryPolicy.Abort || Attempts >= maximumAttempts)
    {
      IsAborted = true;
      return false;
    }

    Attempts++;
    CooldownTicks = policy == MechanismRetryPolicy.RetryNextTick
      ? Math.Max(1, cooldownTicks)
      : cooldownTicks;
    return true;
  }

  public void Tick()
  {
    if (CooldownTicks > 0)
    {
      CooldownTicks--;
    }
  }

  public void Reset()
  {
    Attempts = 0;
    CooldownTicks = 0;
    IsAborted = false;
  }
}
