using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Applies the source-compatible active-dungeon selection command.
/// </summary>
public static class DungeonSelectionControlSystem
{
  public static void Select(
    DungeonLayoutControlComponent component,
    int requestedIndex)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.SelectCurrentDungeon(requestedIndex);
  }
}
