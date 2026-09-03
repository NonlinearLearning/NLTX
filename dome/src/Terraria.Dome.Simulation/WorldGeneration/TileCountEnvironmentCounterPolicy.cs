using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class TileCountEnvironmentCounterPolicy
{
  public static TileCountEnvironmentCounters AddActiveTile(
    TileCountEnvironmentCounters counters,
    ushort tileType)
  {
    if (counters.LavaCount < 0 || counters.IceCount < 0 || counters.SandCount < 0 ||
        counters.RockCount < 0 || counters.ShroomCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(counters));
    }

    int lavaCount = counters.LavaCount;
    int iceCount = counters.IceCount;
    int sandCount = counters.SandCount;
    int rockCount = counters.RockCount;
    int shroomCount = counters.ShroomCount;
    switch (tileType)
    {
      case 70:
        shroomCount = checked(shroomCount + 1);
        break;
      case 1:
        rockCount = checked(rockCount + 1);
        break;
      case 147:
      case 161:
        iceCount = checked(iceCount + 1);
        break;
      case 53:
      case 396:
      case 397:
        sandCount = checked(sandCount + 1);
        break;
    }

    return new TileCountEnvironmentCounters(
      lavaCount,
      iceCount,
      sandCount,
      rockCount,
      shroomCount);
  }

  public static TileCountEnvironmentCounters AddLavaLiquid(
    TileCountEnvironmentCounters counters)
  {
    if (counters.LavaCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(counters));
    }

    return counters with { LavaCount = checked(counters.LavaCount + 1) };
  }
}
