using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the committed ocean-cave treasure positions without exposing mutable storage.
/// </summary>
public static class OceanCaveTreasureQuery
{
  public static OceanCaveTreasureSnapshot Snapshot(
    OceanCaveTreasureStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
