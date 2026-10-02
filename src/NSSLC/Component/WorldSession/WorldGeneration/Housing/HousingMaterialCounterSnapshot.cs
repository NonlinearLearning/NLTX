using System;

namespace Terraria.WorldGeneration.Housing;

public readonly record struct HousingMaterialCounterSnapshot
{
  public HousingMaterialCounterSnapshot(
    int lavaCount,
    int iceCount,
    int sandCount,
    int rockCount,
    int shroomCount)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(lavaCount);
    ArgumentOutOfRangeException.ThrowIfNegative(iceCount);
    ArgumentOutOfRangeException.ThrowIfNegative(sandCount);
    ArgumentOutOfRangeException.ThrowIfNegative(rockCount);
    ArgumentOutOfRangeException.ThrowIfNegative(shroomCount);

    LavaCount = lavaCount;
    IceCount = iceCount;
    SandCount = sandCount;
    RockCount = rockCount;
    ShroomCount = shroomCount;
  }

  public int LavaCount { get; }

  public int IceCount { get; }

  public int SandCount { get; }

  public int RockCount { get; }

  public int ShroomCount { get; }
}
