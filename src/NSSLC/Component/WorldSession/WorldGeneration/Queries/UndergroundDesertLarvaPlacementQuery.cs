using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads committed larva placement coordinates without exposing mutable storage.
/// </summary>
public static class UndergroundDesertLarvaPlacementQuery
{
  public static UndergroundDesertLarvaPlacementSnapshot Snapshot(
    UndergroundDesertLarvaPlacementComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
