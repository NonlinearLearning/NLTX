using System;

namespace Terraria.WorldSession.NpcProgression.MoonLord;

public sealed class MoonLordEncounterStateComponent
{
  private readonly int _maxMoonLordCountdown;

  public MoonLordEncounterStateComponent(int maxMoonLordCountdown)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maxMoonLordCountdown);
    _maxMoonLordCountdown = maxMoonLordCountdown;
  }

  public int MoonLordCountdown { get; private set; }

  public void SetCountdown(int countdown)
  {
    if (countdown < 0 || countdown > _maxMoonLordCountdown)
    {
      throw new ArgumentOutOfRangeException(
        nameof(countdown),
        "Moon Lord countdown must remain within the encounter definition range.");
    }

    MoonLordCountdown = countdown;
  }

  public void Reset()
  {
    MoonLordCountdown = 0;
  }
}
