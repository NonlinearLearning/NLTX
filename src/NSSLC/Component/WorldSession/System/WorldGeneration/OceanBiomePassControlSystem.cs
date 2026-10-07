using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns the pass-scoped desert tile-check transition.
/// </summary>
public static class OceanBiomePassControlSystem
{
  public static void SetSkipDesertTileCheck(
    OceanBiomePassControlComponent component,
    bool value)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SetSkipDesertTileCheck(value);
  }

  public static void Reset(OceanBiomePassControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }
}
