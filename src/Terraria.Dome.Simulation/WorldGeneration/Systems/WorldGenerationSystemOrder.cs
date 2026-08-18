using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration.Systems;

public enum WorldGenerationSystemId
{
  TerrainBase,
  CaveCarving,
  BiomeSurface,
  OrePlacement,
  StructurePlacement,
  TreePlacement,
  LiquidSource,
  LiquidPropagation,
  TileFrame,
  TileChangeCommit,
  Validation
}

public static class WorldGenerationSystemOrder
{
  public static IReadOnlyList<WorldGenerationSystemId> Systems { get; } =
    new[]
    {
      WorldGenerationSystemId.TerrainBase,
      WorldGenerationSystemId.CaveCarving,
      WorldGenerationSystemId.BiomeSurface,
      WorldGenerationSystemId.OrePlacement,
      WorldGenerationSystemId.StructurePlacement,
      WorldGenerationSystemId.TreePlacement,
      WorldGenerationSystemId.LiquidSource,
      WorldGenerationSystemId.LiquidPropagation,
      WorldGenerationSystemId.TileFrame,
      WorldGenerationSystemId.TileChangeCommit,
      WorldGenerationSystemId.Validation
    };
}
