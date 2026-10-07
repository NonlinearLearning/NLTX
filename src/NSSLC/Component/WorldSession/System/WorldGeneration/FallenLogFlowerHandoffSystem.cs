using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns producer and one-shot consumer transitions for the fallen-log handoff.
/// </summary>
public static class FallenLogFlowerHandoffSystem
{
  public static bool TryPublish(
    FallenLogFlowerHandoffComponent component,
    TilePosition position,
    bool placementSucceeded,
    bool randomSelectionAccepted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!placementSucceeded || !randomSelectionAccepted)
    {
      return false;
    }

    component.Publish(position);
    return true;
  }

  public static bool TryConsume(
    FallenLogFlowerHandoffComponent component,
    out TilePosition position)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryConsume(out position);
  }

  public static void Reset(FallenLogFlowerHandoffComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Reset();
  }
}
