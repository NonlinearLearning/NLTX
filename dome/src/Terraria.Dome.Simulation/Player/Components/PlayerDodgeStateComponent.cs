using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerDodgeStateComponent
{
  public bool IsEnabled;
  public float Remaining;

  public void Enable(float charges)
  {
    if (!float.IsFinite(charges) || charges < 0.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(charges));
    }

    IsEnabled = true;
    Remaining = charges;
  }

  public bool TryConsume()
  {
    if (!IsEnabled || Remaining <= 0.0f)
    {
      return false;
    }

    Remaining -= 1.0f;
    return true;
  }

  public void Clear()
  {
    IsEnabled = false;
    Remaining = 0.0f;
  }
}
