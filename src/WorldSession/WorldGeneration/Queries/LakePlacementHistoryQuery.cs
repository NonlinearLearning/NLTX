using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads copied lake placement history without exposing mutable storage.
/// </summary>
public static class LakePlacementHistoryQuery
{
  public static LakePlacementHistorySnapshot Snapshot(
    LakePlacementHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
