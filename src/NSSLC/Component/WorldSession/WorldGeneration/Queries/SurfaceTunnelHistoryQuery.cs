using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

/// <summary>
/// Reads copied surface-tunnel history without exposing mutable storage.
/// </summary>
public static class SurfaceTunnelHistoryQuery
{
  public static SurfaceTunnelHistorySnapshot Snapshot(
    SurfaceTunnelHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.CreateSnapshot();
  }
}
