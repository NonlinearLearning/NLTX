using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Records surface ore-patch history only after an external patch commit succeeds.
/// </summary>
public static class SurfaceOrePatchHistorySystem
{
  public static SurfaceOrePatchHistoryAppendResult AppendAfterSuccessfulCommit(
    SurfaceOrePatchHistoryComponent component,
    int x,
    bool patchCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!patchCommitted)
    {
      return new(
        SurfaceOrePatchHistoryAppendStatus.RejectedCommit,
        ExternalCommitSucceeded: false,
        Appended: false);
    }

    if (!component.TryAppend(x))
    {
      return new(
        SurfaceOrePatchHistoryAppendStatus.RejectedCapacity,
        ExternalCommitSucceeded: true,
        Appended: false);
    }

    return new(
      SurfaceOrePatchHistoryAppendStatus.Appended,
      ExternalCommitSucceeded: true,
      Appended: true);
  }

  public static bool TryAppendAfterSuccessfulCommit(
    SurfaceOrePatchHistoryComponent component,
    int x,
    bool patchCommitted)
  {
    return AppendAfterSuccessfulCommit(component, x, patchCommitted).Appended;
  }

  public static bool TryAppend(
    SurfaceOrePatchHistoryComponent component,
    int x)
  {
    return TryAppendAfterSuccessfulCommit(component, x, patchCommitted: true);
  }

  public static void Clear(SurfaceOrePatchHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
