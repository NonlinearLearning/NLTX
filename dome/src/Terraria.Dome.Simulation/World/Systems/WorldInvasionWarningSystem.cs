using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionWarningResult(int WarningTicks, bool ShouldWarn);

public sealed class WorldInvasionWarningSystem
{
  private const int WarningIntervalTicks = 3600;

  public WorldInvasionWarningResult Advance(int warningTicks, bool arrived, bool moved)
  {
    if (warningTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(warningTicks));
    }

    if (arrived)
    {
      return new WorldInvasionWarningResult(warningTicks, true);
    }

    if (!moved)
    {
      return new WorldInvasionWarningResult(warningTicks, false);
    }

    int remaining = warningTicks - 1;
    return remaining <= 0
      ? new WorldInvasionWarningResult(WarningIntervalTicks, true)
      : new WorldInvasionWarningResult(remaining, false);
  }
}
