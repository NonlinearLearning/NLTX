using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Advances the hell-chest cursor only after the external chest commit succeeds.
/// </summary>
public static class HellChestLootCycleSystem
{
  public static bool TryAdvanceAfterSuccessfulPlacement(
    HellChestLootCycleComponent component,
    bool placementSucceeded)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAdvanceAfterSuccessfulPlacement(placementSucceeded);
  }
}
