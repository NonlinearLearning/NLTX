using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded surface-tunnel history recording without applying tunnel effects.
/// </summary>
public static class SurfaceTunnelHistorySystem
{
  public static bool TryAppend(
    SurfaceTunnelHistoryComponent component,
    int centerX)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(centerX);
  }

  public static void Clear(SurfaceTunnelHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
