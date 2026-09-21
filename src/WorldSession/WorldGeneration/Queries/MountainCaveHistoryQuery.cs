using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads copied mountain-cave history without exposing mutable storage.
/// </summary>
public static class MountainCaveHistoryQuery
{
  public static MountainCaveHistorySnapshot Snapshot(
    MountainCaveHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
