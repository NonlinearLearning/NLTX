using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Owns bounded mountain-cave history recording without applying cave effects.
/// </summary>
public static class MountainCaveHistorySystem
{
  public static MountainCaveHistoryAppendResult AppendAfterSuccessfulCommit(
    MountainCaveHistoryComponent component,
    int x,
    int y,
    bool caveCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!caveCommitted)
    {
      return new(
        MountainCaveHistoryAppendStatus.RejectedCommit,
        ExternalCommitSucceeded: false,
        Appended: false);
    }

    if (!component.TryAppend(x, y))
    {
      return new(
        MountainCaveHistoryAppendStatus.RejectedCapacity,
        ExternalCommitSucceeded: true,
        Appended: false);
    }

    return new(
      MountainCaveHistoryAppendStatus.Appended,
      ExternalCommitSucceeded: true,
      Appended: true);
  }

  public static bool TryAppendAfterSuccessfulCommit(
    MountainCaveHistoryComponent component,
    int x,
    int y,
    bool caveCommitted)
  {
    return AppendAfterSuccessfulCommit(component, x, y, caveCommitted).Appended;
  }

  public static bool TryAppend(
    MountainCaveHistoryComponent component,
    int x,
    int y)
  {
    return TryAppendAfterSuccessfulCommit(component, x, y, caveCommitted: true);
  }

  public static void Clear(MountainCaveHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
