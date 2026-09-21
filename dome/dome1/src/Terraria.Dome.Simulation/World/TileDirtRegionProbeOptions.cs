using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct TileDirtRegionProbeOptions
{
  public const int DefaultMaximumTiles = 3500;

  public TileDirtRegionProbeOptions(int maximumTiles = DefaultMaximumTiles)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTiles);
    MaximumTiles = maximumTiles;
  }

  public int MaximumTiles { get; }
}
