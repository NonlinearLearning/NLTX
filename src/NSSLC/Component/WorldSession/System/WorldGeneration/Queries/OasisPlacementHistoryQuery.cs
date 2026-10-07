using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads copied oasis placement history without exposing mutable storage.
/// </summary>
public static class OasisPlacementHistoryQuery
{
  public static OasisPlacementHistorySnapshot Snapshot(
    OasisPlacementHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
