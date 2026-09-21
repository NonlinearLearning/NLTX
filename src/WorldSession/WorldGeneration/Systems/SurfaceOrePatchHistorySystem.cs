using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Records surface ore-patch history only after an external patch commit succeeds.
/// </summary>
public static class SurfaceOrePatchHistorySystem
{
  public static bool TryAppend(
    SurfaceOrePatchHistoryComponent component,
    int x)
  {
    ArgumentNullException.ThrowIfNull(component);
    return component.TryAppend(x);
  }

  public static bool TryAppendAfterSuccessfulCommit(
    SurfaceOrePatchHistoryComponent component,
    int x,
    bool patchCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    return patchCommitted && component.TryAppend(x);
  }

  public static void Clear(SurfaceOrePatchHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
