using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Restores Version4 underground-desert generation sentinels for one session.
/// </summary>
public static class UndergroundDesertResetSystem
{
  public static void Reset(
    UndergroundDesertStructureComponent structure,
    UndergroundDesertLarvaPlacementComponent larva,
    in UndergroundDesertResetInput input)
  {
    ArgumentNullException.ThrowIfNull(structure);
    ArgumentNullException.ThrowIfNull(larva);
    if (structure.GenerationId != larva.GenerationId)
    {
      throw new ArgumentException(
        "Underground-desert state must belong to the same generation.",
        nameof(larva));
    }

    structure.ReplaceLayout(
      default,
      default,
      input.WorldHeight,
      0,
      input.WorldWidth,
      0);
    larva.Clear();
  }
}
