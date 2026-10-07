using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded surface-tunnel history recording without applying tunnel effects.
/// </summary>
public static class SurfaceTunnelHistorySystem
{
  public static SurfaceTunnelHistoryAppendResult AppendAfterSuccessfulScan(
    SurfaceTunnelHistoryComponent component,
    int centerX,
    bool scanAccepted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!scanAccepted)
    {
      return new(
        SurfaceTunnelHistoryAppendStatus.RejectedScan,
        ScanAccepted: false,
        Appended: false);
    }

    if (!component.TryAppend(centerX))
    {
      return new(
        SurfaceTunnelHistoryAppendStatus.RejectedCapacity,
        ScanAccepted: true,
        Appended: false);
    }

    return new(
      SurfaceTunnelHistoryAppendStatus.Appended,
      ScanAccepted: true,
      Appended: true);
  }

  public static bool TryAppendAfterSuccessfulScan(
    SurfaceTunnelHistoryComponent component,
    int centerX,
    bool scanAccepted)
  {
    return AppendAfterSuccessfulScan(component, centerX, scanAccepted).Appended;
  }

  public static bool TryAppend(
    SurfaceTunnelHistoryComponent component,
    int centerX)
  {
    return TryAppendAfterSuccessfulScan(component, centerX, scanAccepted: true);
  }

  public static void Clear(SurfaceTunnelHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
