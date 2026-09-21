using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Resets generation-scoped unique dungeon reward results.
/// </summary>
public static class DungeonSpecialRewardGenerationResetSystem
{
  public static void Reset(DungeonSpecialRewardGenerationComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetState();
  }
}
