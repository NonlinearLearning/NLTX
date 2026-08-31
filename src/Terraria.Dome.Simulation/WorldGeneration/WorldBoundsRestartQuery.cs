using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldBoundsRestartResult(
  bool RequiresClear,
  PreviousWorldBoundsSnapshot Previous,
  WorldBoundsComponent Current);

public static class WorldBoundsRestartQuery
{
  public static WorldBoundsRestartResult Evaluate(
    PreviousWorldBoundsSnapshot previous,
    WorldBoundsComponent current)
  {
    bool requiresClear = previous.Width > current.Width || previous.Height > current.Height;
    return new WorldBoundsRestartResult(requiresClear, previous, current);
  }
}
