using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Commits one complete liquid-boundary snapshot for its generation session.
/// </summary>
public static class WorldGenerationLiquidBoundarySystem
{
  public static void Commit(
    WorldGenerationLiquidBoundaryComponent component,
    in WorldGenerationLiquidBoundarySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (snapshot.GenerationId != component.GenerationId)
    {
      throw new ArgumentException(
        "Liquid boundaries cannot be committed to another generation.",
        nameof(snapshot));
    }

    component.ReplaceBoundaries(snapshot.LavaLine, snapshot.WaterLine);
  }
}
