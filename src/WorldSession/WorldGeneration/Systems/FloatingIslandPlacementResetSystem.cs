using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Resets floating-island house progress while retaining the sky-lake target.
/// </summary>
public static class FloatingIslandPlacementResetSystem
{
  public static void ResetProgress(FloatingIslandPlacementStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetProgress();
  }
}
