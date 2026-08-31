using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct TileFrameBudget
{
  public TileFrameBudget(int requestedCount, int committedCount, int maximumCount)
  {
    if (requestedCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(requestedCount));
    }

    if (committedCount < 0 || committedCount > requestedCount)
    {
      throw new ArgumentOutOfRangeException(nameof(committedCount));
    }

    if (maximumCount < requestedCount)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumCount));
    }

    RequestedCount = requestedCount;
    CommittedCount = committedCount;
    MaximumCount = maximumCount;
  }

  public int RequestedCount { get; }

  public int CommittedCount { get; }

  public int MaximumCount { get; }
}
