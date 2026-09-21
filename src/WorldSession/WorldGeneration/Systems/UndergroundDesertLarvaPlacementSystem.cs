using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded underground-desert larva coordinate recording without tile effects.
/// </summary>
public static class UndergroundDesertLarvaPlacementSystem
{
  public static bool TryAppend(
    UndergroundDesertLarvaPlacementComponent component,
    TilePosition position)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(position);
  }

  public static void Clear(
    UndergroundDesertLarvaPlacementComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
