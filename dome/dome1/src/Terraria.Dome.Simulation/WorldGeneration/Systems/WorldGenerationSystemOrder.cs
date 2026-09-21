using System;
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
  private static readonly IReadOnlyList<WorldGenerationSystemId> DefaultSystems =
    Array.AsReadOnly(
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
    });

  public static IReadOnlyList<WorldGenerationSystemId> Systems { get; } =
    DefaultSystems;

  public static IReadOnlyList<WorldGenerationSystemId> RegisterDefaults()
  {
    return DefaultSystems;
  }
}
