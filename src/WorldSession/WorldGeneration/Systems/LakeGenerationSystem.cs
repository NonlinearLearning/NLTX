using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Records lake history only after the external terrain/liquid commit succeeds.
/// </summary>
public static class LakeGenerationSystem
{
  public static LakePlacementHistoryAppendResult AppendAfterSuccessfulCommit(
    LakePlacementHistoryComponent component,
    int lakeX,
    bool lakeCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!lakeCommitted)
    {
      return new(
        LakePlacementHistoryAppendStatus.RejectedCommit,
        ExternalCommitSucceeded: false,
        Appended: false);
    }

    if (!component.TryAppend(lakeX))
    {
      return new(
        LakePlacementHistoryAppendStatus.RejectedCapacity,
        ExternalCommitSucceeded: true,
        Appended: false);
    }

    return new(
      LakePlacementHistoryAppendStatus.Appended,
      ExternalCommitSucceeded: true,
      Appended: true);
  }

  public static bool TryAppendAfterSuccessfulCommit(
    LakePlacementHistoryComponent component,
    int lakeX,
    bool lakeCommitted)
  {
    return AppendAfterSuccessfulCommit(component, lakeX, lakeCommitted).Appended;
  }

  public static void Clear(LakePlacementHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
