using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerFlightStateComponent
{
  public int Wings;
  public int WingsLogic;
  public float WingTime;
  public int WingTimeMax;

  public void SetDefinition(int wings, int wingsLogic, int wingTimeMax)
  {
    if (wings < 0 || wingsLogic < 0 || wingTimeMax < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(wings));
    }

    Wings = wings;
    WingsLogic = wingsLogic;
    WingTimeMax = wingTimeMax;
    WingTime = Math.Clamp(WingTime, 0.0f, wingTimeMax);
  }

  public bool TryConsume(float amount)
  {
    if (!float.IsFinite(amount) || amount < 0.0f || WingTime < amount)
    {
      return false;
    }

    WingTime -= amount;
    return true;
  }

  public void Restore()
  {
    WingTime = WingTimeMax;
  }
}
