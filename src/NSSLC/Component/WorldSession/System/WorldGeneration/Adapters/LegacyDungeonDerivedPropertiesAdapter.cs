using System;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

namespace Terraria.WorldGeneration.Adapters;

/// <summary>
/// Keeps the legacy property-shaped access path on the C08 dungeon authority.
/// </summary>
public static class LegacyDungeonDerivedPropertiesAdapter
{
  public static void SetCurrentDungeon(
    DungeonLayoutControlComponent component,
    int requestedIndex)
  {
    ArgumentNullException.ThrowIfNull(component);
    DungeonSelectionControlSystem.Select(component, requestedIndex);
  }

  public static int GetCurrentDungeon(
    DungeonLayoutControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return DungeonDerivedPropertiesQuery.CurrentDungeon(
      DungeonLayoutQuery.Snapshot(component));
  }

  public static DungeonRecordSnapshot GetCurrentDungeonGenVars(
    DungeonLayoutControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return DungeonDerivedPropertiesQuery.CurrentDungeonGenVars(
      DungeonLayoutQuery.Snapshot(component));
  }
}
