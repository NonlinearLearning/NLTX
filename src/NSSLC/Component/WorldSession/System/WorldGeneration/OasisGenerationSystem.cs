using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Systems;

/// <summary>
/// Records oasis metadata only after the external terrain/liquid commit succeeds.
/// </summary>
public static class OasisGenerationSystem
{
  public static OasisPlacementHistoryAppendResult AppendAfterSuccessfulCommit(
    OasisPlacementHistoryComponent component,
    TilePosition center,
    int width,
    bool oasisCommitted)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!oasisCommitted)
    {
      return new(
        OasisPlacementHistoryAppendStatus.RejectedCommit,
        ExternalCommitSucceeded: false,
        Appended: false);
    }

    if (component.Count >= OasisPlacementCapacityDefinition.Capacity)
    {
      return new(
        OasisPlacementHistoryAppendStatus.RejectedCapacity,
        ExternalCommitSucceeded: true,
        Appended: false);
    }

    if (!component.TryAppend(center, width))
    {
      return new(
        OasisPlacementHistoryAppendStatus.RejectedWidth,
        ExternalCommitSucceeded: true,
        Appended: false);
    }

    return new(
      OasisPlacementHistoryAppendStatus.Appended,
      ExternalCommitSucceeded: true,
      Appended: true);
  }

  public static bool TryAppendAfterSuccessfulCommit(
    OasisPlacementHistoryComponent component,
    TilePosition center,
    int width,
    bool oasisCommitted)
  {
    return AppendAfterSuccessfulCommit(component, center, width, oasisCommitted).Appended;
  }

  public static void Clear(OasisPlacementHistoryComponent component)
  {
    ArgumentNullException.ThrowIfNull(component);
    component.Clear();
  }
}
