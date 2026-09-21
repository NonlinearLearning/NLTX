using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Resets generation-scoped dungeon layout state at the dungeon setup barrier.
/// </summary>
public static class DungeonLayoutResetSystem
{
  public static void Reset(DungeonLayoutControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.ResetState();
  }
}
