using System;

namespace Terraria.WorldGeneration.Components;

public sealed class WorldGenerationLiquidBoundaryComponent
{
  public WorldGenerationLiquidBoundaryComponent(
    long generationId,
    int lavaLine = 0,
    int waterLine = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    LavaLine = lavaLine;
    WaterLine = waterLine;
  }

  public long GenerationId { get; }

  public int LavaLine { get; private set; }

  public int WaterLine { get; private set; }

  internal void ReplaceBoundaries(int lavaLine, int waterLine)
  {
    LavaLine = lavaLine;
    WaterLine = waterLine;
  }

  public WorldGenerationLiquidBoundarySnapshot CreateSnapshot()
  {
    return new WorldGenerationLiquidBoundarySnapshot(
      GenerationId,
      LavaLine,
      WaterLine);
  }
}
