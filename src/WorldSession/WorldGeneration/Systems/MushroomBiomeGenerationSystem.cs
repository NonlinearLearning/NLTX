using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns mushroom-anchor recording without applying ShroomPatch or other tile effects.
/// </summary>
public static class MushroomBiomeGenerationSystem
{
  public static bool TryAppendAfterSuccessfulPatchCommit(
    MushroomBiomeAnchorStateComponent component,
    TilePosition anchor,
    bool patchesCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    return patchesCommitted && component.TryAppend(anchor);
  }

  public static void Clear(MushroomBiomeAnchorStateComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
