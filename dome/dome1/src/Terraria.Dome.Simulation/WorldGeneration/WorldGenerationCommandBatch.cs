using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration.Commands;

namespace Terraria.Dome.Simulation.WorldGeneration;

/// <summary>
/// Typed mutation intents produced by one generation pass.
/// </summary>
public sealed class WorldGenerationCommandBatch
{
  public WorldGenerationCommandBatch(
    IReadOnlyList<TileChangeCommand>? tileCommands = null,
    IReadOnlyList<LiquidChangeCommand>? liquidCommands = null,
    IReadOnlyList<StructurePlacementCommand>? structureCommands = null)
  {
    TileCommands = Copy(tileCommands);
    LiquidCommands = Copy(liquidCommands);
    StructureCommands = Copy(structureCommands);
  }

  public IReadOnlyList<TileChangeCommand> TileCommands { get; }

  public IReadOnlyList<LiquidChangeCommand> LiquidCommands { get; }

  public IReadOnlyList<StructurePlacementCommand> StructureCommands { get; }

  private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T>? values)
  {
    return values is null
      ? Array.Empty<T>()
      : new List<T>(values).AsReadOnly();
  }
}
