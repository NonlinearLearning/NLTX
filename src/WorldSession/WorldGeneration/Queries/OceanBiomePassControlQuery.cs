using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the current ocean-biome pass control without changing its phase state.
/// </summary>
public static class OceanBiomePassControlQuery
{
  public static bool IsDesertTileCheckSkipped(
    OceanBiomePassControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.SkipDesertTileCheck;
  }
}
