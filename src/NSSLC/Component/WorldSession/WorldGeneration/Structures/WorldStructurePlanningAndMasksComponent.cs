using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;

namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Stores the candidate structure-planning collections for a world-generation scope.
/// </summary>
public sealed class WorldStructurePlanningAndMasksComponent
{
  public WorldStructurePlanningAndMasksComponent(
    long generationId,
    IReadOnlyList<WorldGenerationRectangle>? plannedStructures = null,
    IReadOnlyList<WorldGenerationRectangle>? protectedStructures = null)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    GenerationId = generationId;
    PlannedStructures = plannedStructures is null
      ? []
      : new List<WorldGenerationRectangle>(plannedStructures).AsReadOnly();
    ProtectedStructures = protectedStructures is null
      ? []
      : new List<WorldGenerationRectangle>(protectedStructures).AsReadOnly();
  }

  public long GenerationId { get; }

  public IReadOnlyList<WorldGenerationRectangle> PlannedStructures { get; }

  public IReadOnlyList<WorldGenerationRectangle> ProtectedStructures { get; }
}
