using System;

namespace Terraria.Dome.Simulation.WorldModel.Systems;

public readonly record struct WorldInvasionTravelResult(double Position, bool Arrived);

public sealed class WorldInvasionTravelSystem
{
  public WorldInvasionTravelResult Advance(double position, int spawnTileX, float dayRate)
  {
    if (!double.IsFinite(position) || !float.IsFinite(dayRate))
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }

    double target = spawnTileX;
    if (position == target)
    {
      return new WorldInvasionTravelResult(target, true);
    }

    double step = Math.Max(dayRate, 1.0f);
    double next = position > target ? position - step : position + step;
    if ((position > target && next <= target) || (position < target && next >= target))
    {
      return new WorldInvasionTravelResult(target, true);
    }

    return new WorldInvasionTravelResult(next, false);
  }
}
