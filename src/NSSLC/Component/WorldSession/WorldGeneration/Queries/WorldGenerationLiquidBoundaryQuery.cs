using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the generation liquid-boundary snapshot without propagating liquid.
/// </summary>
public static class WorldGenerationLiquidBoundaryQuery
{
  public static WorldGenerationLiquidBoundarySnapshot Snapshot(
    WorldGenerationLiquidBoundaryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
