using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct TileRegionProbeOptions
{
  public const int DefaultMaximumTiles = 3500;

  public TileRegionProbeOptions(
    bool jungle = false,
    bool lavaOk = false,
    int maximumTiles = DefaultMaximumTiles)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumTiles);
    Jungle = jungle;
    LavaOk = lavaOk;
    MaximumTiles = maximumTiles;
  }

  public bool Jungle { get; }
  public bool LavaOk { get; }
  public int MaximumTiles { get; }
}
