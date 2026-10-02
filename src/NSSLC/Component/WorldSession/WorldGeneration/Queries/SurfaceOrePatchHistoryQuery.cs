using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads copied surface ore-patch history without exposing mutable storage.
/// </summary>
public static class SurfaceOrePatchHistoryQuery
{
  public static SurfaceOrePatchHistorySnapshot Snapshot(
    SurfaceOrePatchHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
