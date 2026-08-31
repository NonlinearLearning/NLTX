using System;

namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerStealthStateComponent
{
  public bool IsInvisible;
  public float Stealth;
  public bool HasShroomiteStealth;
  public bool IsVortexStealthActive;
  public int StealthTimer;

  public void Set(
    bool isInvisible,
    float stealth,
    bool hasShroomiteStealth,
    bool isVortexStealthActive)
  {
    if (!float.IsFinite(stealth) || stealth < 0.0f || stealth > 1.0f)
    {
      throw new ArgumentOutOfRangeException(nameof(stealth));
    }

    IsInvisible = isInvisible;
    Stealth = stealth;
    HasShroomiteStealth = hasShroomiteStealth;
    IsVortexStealthActive = isVortexStealthActive;
    StealthTimer = 0;
  }
}
