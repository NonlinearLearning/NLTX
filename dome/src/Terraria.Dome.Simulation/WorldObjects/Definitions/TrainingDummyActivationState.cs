using System;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public sealed class TrainingDummyActivationState
{
  public int RetryCooldownTicks { get; private set; }
  public bool IsActive { get; private set; }
  public long Revision { get; private set; } = 1;

  public bool TryActivate()
  {
    if (RetryCooldownTicks > 0)
    {
      return false;
    }

    return TryTransition(true);
  }

  public bool TryDeactivate()
  {
    return TryTransition(false);
  }

  public void SetRetryCooldown(int ticks)
  {
    if (ticks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(ticks));
    }

    RetryCooldownTicks = ticks;
  }

  public void Tick()
  {
    if (RetryCooldownTicks > 0)
    {
      RetryCooldownTicks--;
    }
  }

  private bool TryTransition(bool active)
  {
    if (IsActive == active || Revision == long.MaxValue)
    {
      return false;
    }

    IsActive = active;
    Revision++;
    return true;
  }
}
