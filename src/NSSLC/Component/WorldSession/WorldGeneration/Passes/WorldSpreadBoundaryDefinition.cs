using System;

namespace Terraria.WorldGeneration.Passes;

public sealed class WorldSpreadBoundaryDefinition
{
  public WorldSpreadBoundaryDefinition(int outerWorldBuffer)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(outerWorldBuffer);
    OuterWorldBuffer = outerWorldBuffer;
  }

  public static WorldSpreadBoundaryDefinition Version4 { get; } = new(
    outerWorldBuffer: 10);

  public int OuterWorldBuffer { get; }
}
