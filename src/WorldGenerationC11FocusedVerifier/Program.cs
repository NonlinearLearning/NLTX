using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestLakeHistoryBoundary();
      TestOasisHistoryBoundary();
      TestExplicitCommitOutcomes();
      Console.WriteLine("C11 lake and oasis focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestLakeHistoryBoundary()
  {
    Require(
      LakePlacementCapacityDefinition.Capacity == 50 &&
      LakePlacementCapacityDefinition.EffectiveEntryLimit == 49,
      "lake capacity definitions");

    LakePlacementHistoryComponent component =
      new(generationId: 51);
    Require(component.Count == 0, "lake history starts empty");
    Require(
      !LakeGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        lakeX: 100,
        lakeCommitted: false),
      "failed lake commit must not append");
    Require(component.Count == 0, "failed lake commit preserves count");
    Require(
      LakeGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        lakeX: 100,
        lakeCommitted: true),
      "successful lake commit must append");

    MountainCaveHistoryComponent caveHistoryForLakeBoundary =
      new(generationId: 51);
    Require(caveHistoryForLakeBoundary.TryAppend(500, 400), "lake cave history seed");
    MountainCaveHistoryComponent caveHistoryForDistanceBoundary =
      new(generationId: 51);
    Require(caveHistoryForDistanceBoundary.TryAppend(300, 400), "distance cave history seed");
    SurfaceTunnelHistoryComponent tunnelHistory =
      new(generationId: 51);
    Require(tunnelHistory.TryAppend(500), "tunnel history seed");

    Require(
      LakeOasisAvoidanceQuery.IsLakeCandidateBlocked(
        LakePlacementHistoryQuery.Snapshot(component),
        MountainCaveHistoryQuery.Snapshot(caveHistoryForLakeBoundary),
        SurfaceTunnelHistoryQuery.Snapshot(tunnelHistory),
        candidateX: 249,
        lakeSpacing: 150,
        caveAndTunnelSpacing: 100),
      "lake spacing must use strict less-than distance");
    Require(
      !LakeOasisAvoidanceQuery.IsLakeCandidateBlocked(
        LakePlacementHistoryQuery.Snapshot(component),
        MountainCaveHistoryQuery.Snapshot(caveHistoryForLakeBoundary),
        SurfaceTunnelHistoryQuery.Snapshot(tunnelHistory),
        candidateX: 250,
        lakeSpacing: 150,
        caveAndTunnelSpacing: 100),
      "lake spacing must allow the exact boundary");
    Require(
      LakeOasisAvoidanceQuery.IsLakeCandidateBlocked(
        LakePlacementHistoryQuery.Snapshot(component),
        MountainCaveHistoryQuery.Snapshot(caveHistoryForDistanceBoundary),
        SurfaceTunnelHistoryQuery.Snapshot(tunnelHistory),
        candidateX: 399,
        lakeSpacing: 150,
        caveAndTunnelSpacing: 100),
      "cave avoidance must use strict less-than distance");
    Require(
      !LakeOasisAvoidanceQuery.IsLakeCandidateBlocked(
        LakePlacementHistoryQuery.Snapshot(component),
        MountainCaveHistoryQuery.Snapshot(caveHistoryForDistanceBoundary),
        SurfaceTunnelHistoryQuery.Snapshot(tunnelHistory),
        candidateX: 400,
        lakeSpacing: 150,
        caveAndTunnelSpacing: 100),
      "cave avoidance must allow the exact boundary");

    MountainCaveHistoryComponent otherGenerationCaves =
      new(generationId: 52);
    RequireThrows<ArgumentException>(
      () => LakeOasisAvoidanceQuery.IsLakeCandidateBlocked(
        LakePlacementHistoryQuery.Snapshot(component),
        MountainCaveHistoryQuery.Snapshot(otherGenerationCaves),
        SurfaceTunnelHistoryQuery.Snapshot(tunnelHistory),
        candidateX: 250,
        lakeSpacing: 150,
        caveAndTunnelSpacing: 100),
      "lake avoidance must reject mixed-generation histories");

    while (component.Count < LakePlacementCapacityDefinition.EffectiveEntryLimit)
    {
      Require(
        LakeGenerationSystem.TryAppendAfterSuccessfulCommit(
          component,
          lakeX: 1000 + component.Count,
          lakeCommitted: true),
        "lake append within effective capacity");
    }

    Require(
      !LakeGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        lakeX: 9999,
        lakeCommitted: true),
      "lake effective capacity rejection");

    LakePlacementHistorySnapshot snapshot =
      LakePlacementHistoryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 51, "lake generation");
    Require(snapshot.Capacity == 50, "lake declared capacity");
    Require(snapshot.EffectiveEntryLimit == 49, "lake effective capacity");
    Require(snapshot.Count == 49, "lake count");
    Require(snapshot.LakeX[0] == 100, "lake insertion order");
    Require(snapshot.LakeX[48] == 1048, "lake last recorded x");
    Require(
      ((IList<int>)snapshot.LakeX).IsReadOnly,
      "lake snapshot must be read-only");

    LakeGenerationSystem.Clear(component);
    Require(component.Count == 0, "lake clear");
    Require(snapshot.Count == 49, "lake snapshot isolation");
  }

  private static void TestOasisHistoryBoundary()
  {
    Require(
      OasisPlacementCapacityDefinition.Capacity == 20 &&
      OasisHeightDefinition.Height == 20,
      "oasis capacity definitions");

    OasisPlacementHistoryComponent component =
      new(generationId: 52);
    TilePosition firstCenter = new(100, 200);
    Require(
      !OasisGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        firstCenter,
        width: 45,
        oasisCommitted: false),
      "failed oasis commit must not append");
    Require(component.Count == 0, "failed oasis commit preserves count");
    Require(
      OasisGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        firstCenter,
        width: 45,
        oasisCommitted: true),
      "successful oasis commit must append");

    OasisPlacementHistorySnapshot firstSnapshot =
      OasisPlacementHistoryQuery.Snapshot(component);
    Require(
      OasisPlacementQuery.HasCenterWithinDistance(
        firstSnapshot,
        new TilePosition(449, 200),
        distance: 350),
      "oasis spacing must use strict less-than distance");
    Require(
      !OasisPlacementQuery.HasCenterWithinDistance(
        firstSnapshot,
        new TilePosition(450, 200),
        distance: 350),
      "oasis spacing must allow the exact boundary");

    Require(
      !OasisGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        new TilePosition(300, 400),
        width: 44,
        oasisCommitted: true),
      "an invalid oasis width must not append");

    while (component.Count < OasisPlacementCapacityDefinition.Capacity)
    {
      Require(
        OasisGenerationSystem.TryAppendAfterSuccessfulCommit(
          component,
          new TilePosition(1000 + component.Count, 2000 + component.Count),
          width: 45 + component.Count % 16,
          oasisCommitted: true),
        "oasis append within capacity");
    }

    Require(
      !OasisGenerationSystem.TryAppendAfterSuccessfulCommit(
        component,
        new TilePosition(9000, 9000),
        width: 60,
        oasisCommitted: true),
      "full oasis metadata must not append after an external success");

    OasisPlacementHistorySnapshot snapshot =
      OasisPlacementHistoryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 52, "oasis generation");
    Require(snapshot.Capacity == 20, "oasis declared capacity");
    Require(snapshot.Count == 20, "oasis count");
    Require(snapshot.Centers[0] == firstCenter, "oasis center pairing");
    Require(snapshot.Widths[0] == 45, "oasis width pairing");
    Require(snapshot.Centers[19].X == 1019, "oasis last center");
    Require(
      snapshot.Widths[19] >= 45 && snapshot.Widths[19] <= 60,
      "oasis width must stay within the Version4 range");
    Require(
      ((IList<TilePosition>)snapshot.Centers).IsReadOnly &&
      ((IList<int>)snapshot.Widths).IsReadOnly,
      "oasis snapshot must be read-only");

    OasisGenerationSystem.Clear(component);
    Require(component.Count == 0, "oasis clear");
    Require(snapshot.Count == 20, "oasis snapshot isolation");
  }

  private static void TestExplicitCommitOutcomes()
  {
    LakePlacementHistoryComponent lakeHistory =
      new(generationId: 53);
    LakePlacementHistoryAppendResult lakeCommitRejected =
      LakeGenerationSystem.AppendAfterSuccessfulCommit(
        lakeHistory,
        lakeX: 10,
        lakeCommitted: false);
    Require(
      lakeCommitRejected.Status == LakePlacementHistoryAppendStatus.RejectedCommit &&
      !lakeCommitRejected.ExternalCommitSucceeded &&
      !lakeCommitRejected.Appended,
      "lake commit rejection must be explicit");

    for (int index = 0;
      index < LakePlacementCapacityDefinition.EffectiveEntryLimit;
      index++)
    {
      Require(
        LakeGenerationSystem.AppendAfterSuccessfulCommit(
            lakeHistory,
            lakeX: 100 + index,
            lakeCommitted: true)
          .Appended,
        "lake explicit append within capacity");
    }

    LakePlacementHistoryAppendResult lakeCapacityRejected =
      LakeGenerationSystem.AppendAfterSuccessfulCommit(
        lakeHistory,
        lakeX: 9999,
        lakeCommitted: true);
    Require(
      lakeCapacityRejected.Status == LakePlacementHistoryAppendStatus.RejectedCapacity &&
      lakeCapacityRejected.ExternalCommitSucceeded &&
      !lakeCapacityRejected.Appended,
      "lake full history must preserve external commit success");

    OasisPlacementHistoryComponent oasisHistory =
      new(generationId: 54);
    OasisPlacementHistoryAppendResult oasisCommitRejected =
      OasisGenerationSystem.AppendAfterSuccessfulCommit(
        oasisHistory,
        new TilePosition(10, 20),
        width: 45,
        oasisCommitted: false);
    Require(
      oasisCommitRejected.Status == OasisPlacementHistoryAppendStatus.RejectedCommit &&
      !oasisCommitRejected.ExternalCommitSucceeded &&
      !oasisCommitRejected.Appended,
      "oasis commit rejection must be explicit");

    OasisPlacementHistoryAppendResult oasisWidthRejected =
      OasisGenerationSystem.AppendAfterSuccessfulCommit(
        oasisHistory,
        new TilePosition(10, 20),
        width: 44,
        oasisCommitted: true);
    Require(
      oasisWidthRejected.Status == OasisPlacementHistoryAppendStatus.RejectedWidth &&
      oasisWidthRejected.ExternalCommitSucceeded &&
      !oasisWidthRejected.Appended,
      "oasis width rejection must be explicit");

    for (int index = 0;
      index < OasisPlacementCapacityDefinition.Capacity;
      index++)
    {
      Require(
        OasisGenerationSystem.AppendAfterSuccessfulCommit(
            oasisHistory,
            new TilePosition(100 + index, 200 + index),
            width: 45,
            oasisCommitted: true)
          .Appended,
        "oasis explicit append within capacity");
    }

    OasisPlacementHistoryAppendResult oasisCapacityRejected =
      OasisGenerationSystem.AppendAfterSuccessfulCommit(
        oasisHistory,
        new TilePosition(9999, 9999),
        width: 60,
        oasisCommitted: true);
    Require(
      oasisCapacityRejected.Status == OasisPlacementHistoryAppendStatus.RejectedCapacity &&
      oasisCapacityRejected.ExternalCommitSucceeded &&
      !oasisCapacityRejected.Appended,
      "oasis full history must preserve external commit success");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(Action action, string message)
    where TException : Exception
  {
    try
    {
      action.Invoke();
    }
    catch (TException)
    {
      return;
    }

    throw new InvalidOperationException(message);
  }
}
