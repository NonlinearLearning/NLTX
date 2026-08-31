using System;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct DungeonGenerationContext
{
  public DungeonGenerationContext(int sequence)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(sequence);
    Sequence = sequence;
  }

  public int Sequence { get; }
}
