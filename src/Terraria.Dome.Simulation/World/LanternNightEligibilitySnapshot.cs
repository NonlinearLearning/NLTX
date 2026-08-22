using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct LanternNightNpcSnapshot(bool IsActive, bool IsBoss, int TypeId);

public sealed class LanternNightEligibilitySnapshot
{
  public LanternNightEligibilitySnapshot(
    bool isPumpkinMoon,
    bool isSnowMoon,
    int moonLordCountdown,
    IReadOnlyList<LanternNightNpcSnapshot> npcs)
  {
    ArgumentNullException.ThrowIfNull(npcs);
    ArgumentOutOfRangeException.ThrowIfNegative(moonLordCountdown);

    IsPumpkinMoon = isPumpkinMoon;
    IsSnowMoon = isSnowMoon;
    MoonLordCountdown = moonLordCountdown;
    Npcs = npcs;
  }

  public bool IsPumpkinMoon { get; }
  public bool IsSnowMoon { get; }
  public int MoonLordCountdown { get; }
  public IReadOnlyList<LanternNightNpcSnapshot> Npcs { get; }
}
