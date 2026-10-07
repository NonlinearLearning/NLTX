using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded ocean-cave treasure recording without applying placement effects.
/// </summary>
public static class OceanCaveTreasureSystem
{
  public static bool TryAppend(
    OceanCaveTreasureStateComponent component,
    TilePosition position)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(position);
  }

  public static bool RecordAfterOceanCaveAttempt(
    OceanCaveTreasureStateComponent component,
    TilePosition position,
    bool treasureGenerated)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (component.Count >= OceanCaveTreasureStateComponent.Capacity)
    {
      component.Clear();
    }

    return treasureGenerated && component.TryAppend(position);
  }

  public static void Clear(OceanCaveTreasureStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
