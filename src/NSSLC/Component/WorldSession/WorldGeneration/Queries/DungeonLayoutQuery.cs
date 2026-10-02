using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads dungeon layout state and exposes the source-compatible active-record failure boundary.
/// </summary>
public static class DungeonLayoutQuery
{
  public static DungeonLayoutSnapshot Snapshot(
    DungeonLayoutControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }

  public static DungeonRecordSnapshot CurrentRecord(
    DungeonLayoutControlComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.Records[component.CurrentDungeon];
  }
}
