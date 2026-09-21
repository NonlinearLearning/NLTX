using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads the committed ocean and biome constraints without exposing component mutation.
/// </summary>
public static class OceanBiomeConstraintQuery
{
  public static OceanBiomeConstraintSnapshot Snapshot(
    OceanBiomeConstraintComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
