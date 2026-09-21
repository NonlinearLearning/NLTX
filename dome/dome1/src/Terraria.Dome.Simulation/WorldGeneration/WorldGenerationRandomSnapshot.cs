using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct WorldGenerationRandomSnapshot
{
  public WorldGenerationRandomSnapshot(uint state, int streamVersion)
  {
    if (streamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(streamVersion));
    }

    State = state;
    StreamVersion = streamVersion;
  }

  public uint State { get; }

  public int StreamVersion { get; }
}
