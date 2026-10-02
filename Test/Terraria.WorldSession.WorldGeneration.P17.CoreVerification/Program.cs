using System;
using System.Collections.Generic;
using Saved = Terraria.WorldSession.Components;
using Terraria.WorldGeneration;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertThrows<TException>(Action action, string message)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(message);
}

static void VerifyLifecycleBoundary()
{
  WorldGenerationLifecycleComponent lifecycle =
    WorldGenerationLifecycleSystem.Create(7);
  lifecycle = WorldGenerationLifecycleSystem.BeginLoading(in lifecycle, 7);
  lifecycle = WorldGenerationLifecycleSystem.BeginGenerating(in lifecycle, 7);
  lifecycle = WorldGenerationLifecycleSystem.MarkReady(in lifecycle, 7);

  Assert(lifecycle.IsReady, "A valid generation must reach Ready.");
  Assert(lifecycle.GenerationRevision == 3,
    "Each lifecycle transition must advance the revision.");
  AssertThrows<ArgumentException>(
    () => WorldGenerationLifecycleSystem.BeginUnloading(in lifecycle, 8),
    "A stale generation must not mutate lifecycle state.");
  AssertThrows<InvalidOperationException>(
    () => WorldGenerationLifecycleSystem.MarkReady(in lifecycle, 7),
    "A Ready generation must not re-enter Ready.");

  lifecycle = WorldGenerationLifecycleSystem.BeginUnloading(in lifecycle, 7);
  lifecycle = WorldGenerationLifecycleSystem.Reset(in lifecycle, 7);
  Assert(lifecycle.Phase == WorldPreparationState.Uninitialized,
    "Reset must return a session to the uninitialized boundary.");
  Assert(lifecycle.GenerationRevision == 5,
    "Unload and reset must also advance the lifecycle revision.");
}

static void VerifyPassOrderingAndControlBoundary()
{
  WorldGenerationPlanComponent plan = new(
    generationId: 7,
    planVersion: 1,
    passDescriptors: new[]
    {
      new GenerationPassDescriptor("Terrain", 1.0d, version: 2),
      new GenerationPassDescriptor("Disabled", 0.5d, version: 3)
    },
    disabledPassIds: new[] { "Disabled" });
  WorldGenerationPassStateComponent state = new(7);
  WorldGenerationManifestComponent manifest =
    new(version: "test", gitSha: "core");
  WorldGenerationManifestComponent invalidManifest =
    new(
      version: "test",
      gitSha: "core",
      passResults: new[]
      {
        new WorldGenerationPassResultComponent(7, 0, "Terrain", skipped: true)
      });

  AssertThrows<InvalidOperationException>(
    () => WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in state,
      invalidManifest,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 1),
    "A manifest prefix must preserve disabled pass semantics.");

  AssertThrows<InvalidOperationException>(
    () => WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in state,
      manifest,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 1),
    "A pass cannot skip the next uncommitted plan entry.");

  WorldGenerationPassStartResult skipped =
    WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in state,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 1);
  Assert(skipped.Skipped && !skipped.State.HasActivePass,
    "A disabled pass must be skipped without becoming active.");

  WorldGenerationPassStartResult started =
    WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in state,
      manifest,
      "Terrain",
      WorldGenerationStage.Terrain,
      readSnapshotRevision: 2);
  WorldGenerationPassStateComponent startedState = started.State;
  Assert(started.Started && started.State.PassVersion == 2,
    "An enabled pass must expose its descriptor version.");
  Assert(started.State.ReadSnapshotRevision == 2,
    "A pass must retain the snapshot revision it read.");

  AssertThrows<InvalidOperationException>(
    () => WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in startedState,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 3),
    "A disabled pass must not bypass an active pass guard.");

  WorldGenerationPassResultComponent terrainResult =
    new(7, 0, "Terrain", durationMs: 12, hash: 42);
  WorldGenerationPassStateComponent completed =
    WorldGenerationPassExecutionSystem.CompletePassWithResult(
      plan,
      in startedState,
      manifest,
      terrainResult,
      WorldGenerationStage.Structure,
      checkpointRevision: 4);
  Assert(!completed.HasActivePass && completed.CheckpointRevision == 4,
    "Completing a pass must publish its checkpoint and clear active identity.");
  Assert(manifest.PassResults.Count == 1 && manifest.PassResults[0] == terrainResult,
    "A successful result must be appended exactly once.");

  WorldGenerationPassResultComponent skippedResult =
    new(7, 1, "Disabled", skipped: true);
  WorldGenerationPassStartResult orderedSkipped =
    WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in completed,
      manifest,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 5);
  Assert(orderedSkipped.Skipped && !orderedSkipped.State.HasActivePass,
    "A disabled pass must be skipped at the next manifest position.");
  WorldGenerationPassExecutionSystem.CommitSkippedPassResult(
    plan,
    in completed,
    manifest,
    skippedResult);
  Assert(manifest.PassResults.Count == 2,
    "A disabled result must commit at the next plan index.");
  AssertThrows<InvalidOperationException>(
    () => WorldGenerationPassExecutionSystem.BeginPass(
      plan,
      in completed,
      manifest,
      "Disabled",
      WorldGenerationStage.Structure,
      readSnapshotRevision: 6),
    "A fully committed plan must not begin another pass.");
  AssertThrows<InvalidOperationException>(
    () => WorldGenerationPassExecutionSystem.CommitSkippedPassResult(
      plan,
      in completed,
      manifest,
      skippedResult),
    "A result prefix must reject duplicate commits.");
}

static void VerifyLayerMetricsSnapshotIsolation()
{
  int[] snowMin = { 10, 20 };
  int[] snowMax = { 11, 21 };
  WorldLayerMetricsComponent component = new(generationId: 7);
  WorldLayerMetricsSystem.Commit(
    component,
    new WorldLayerMetricsSnapshot(
      7,
      -1,
      100d,
      120d,
      140d,
      300d,
      320d,
      340d,
      400,
      500,
      600,
      700,
      snowMin,
      snowMax));
  snowMin[0] = -100;
  snowMax[0] = -101;

  WorldLayerMetricsSnapshot snapshot = WorldLayerMetricsQuery.Snapshot(component);
  Assert(snapshot.WorldSurface == 120d && snapshot.RockLayer == 320d,
    "Layer scalar values must be committed as one snapshot.");
  Assert(snapshot.SnowMinX[0] == 10 && snapshot.SnowMaxX[0] == 11,
    "Layer snow columns must be copied at the commit boundary.");
  AssertThrows<NotSupportedException>(
    () => ((IList<int>)snapshot.SnowMinX)[0] = 99,
    "A layer snapshot must not expose a mutable snow array.");
  AssertThrows<ArgumentException>(
    () => WorldLayerMetricsSystem.Commit(
      component,
      new WorldLayerMetricsSnapshot(
        8,
        -1,
        1d,
        2d,
        3d,
        4d,
        5d,
        6d,
        7,
        8,
        9,
        10,
        new[] { 1 },
        new[] { 2 })),
    "A stale layer snapshot must be rejected.");
}

static void VerifyDungeonDerivedIndexBoundary()
{
  DungeonLayoutControlComponent component = new(7);
  DungeonLayoutControlSystem.Commit(
    component,
    new DungeonLayoutSnapshot(
      7,
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      new[] { new DungeonRecordSnapshot(100), new DungeonRecordSnapshot(200) },
      1));

  DungeonSelectionControlSystem.Select(component, -1);
  DungeonLayoutSnapshot snapshot = DungeonLayoutQuery.Snapshot(component);
  Assert(DungeonDerivedPropertiesQuery.CurrentDungeon(snapshot) == 0,
    "CurrentDungeon must preserve the source lower-bound clamp.");
  Assert(DungeonDerivedPropertiesQuery.CurrentDungeonGenVars(snapshot).RecordId == 100,
    "The derived dungeon record must use the same selected index.");

  DungeonSelectionControlSystem.Select(component, 2);
  AssertThrows<ArgumentOutOfRangeException>(
    () => DungeonLayoutQuery.CurrentRecord(component),
    "An upper index must retain the source collection failure boundary.");
  AssertThrows<ArgumentException>(
    () => DungeonLayoutControlSystem.Commit(
      component,
      new DungeonLayoutSnapshot(
        8,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        Array.Empty<DungeonRecordSnapshot>(),
        0)),
    "A stale dungeon snapshot must be rejected.");
}

static void VerifySavedOreTierRepairAndProjection()
{
  Saved.WorldSavedOreTierStateComponent state =
    new(Saved.OreTierState.Uninitialized);
  Saved.WorldSavedOreTierRepairSystem.Apply(
    state,
    new Saved.WorldSavedOreTierTileCounts(
      CopperVanillaCount: 10,
      CopperAlternateCount: 10,
      IronVanillaCount: 20,
      IronAlternateCount: 10,
      SilverVanillaCount: 30,
      SilverAlternateCount: 30,
      GoldVanillaCount: 40,
      GoldAlternateCount: 20));
  Saved.OreTierState repaired = Saved.WorldSavedOreTierQuery.Snapshot(state);
  Assert(repaired.Copper == 166 && repaired.Iron == 6 &&
         repaired.Silver == 168 && repaired.Gold == 8,
    "Low-tier repair must preserve the source strict-greater tie rule.");

  Saved.WorldSavedOreTierCommitSystem commitSystem =
    new(state);
  commitSystem.CommitGeneration(new Saved.WorldSavedOreTierGenerationCommit(
    166,
    167,
    168,
    169));
  commitSystem.CommitAltar(new Saved.WorldSavedOreTierAltarCommit(
    107,
    108,
    109));
  Saved.OreTierState committed = Saved.WorldSavedOreTierQuery.Snapshot(state);
  Assert(committed == new Saved.OreTierState(166, 167, 168, 169, 107, 108, 109),
    "Generation and altar commits must preserve all seven fields.");

  Saved.WorldSavedOreTierNetworkFields packet =
    new Saved.NetMessageSavedOreTierAdapter().Encode(in committed);
  Assert(packet.Copper == 166 && packet.Gold == 169 && packet.Adamantite == 109,
    "The outbound projection must preserve the seven-field order and width.");
}

static void VerifySavedOreTierLoadBoundary()
{
  Saved.WorldSavedOreTierStateComponent state =
    new(new Saved.OreTierState(7, 6, 9, 8, 107, 108, 111));
  Saved.WorldFileSavedOreTierAdapter adapter = new();
  Saved.OreTierState loaded = Saved.WorldSavedOreTierLoadSystem.Load(
    state,
    adapter,
    new Saved.WorldSavedOreTierFileInput(
      VersionNumber: 23,
      AltarCount: 0,
      Cobalt: null,
      Mythril: null,
      Adamantite: null,
      Copper: null,
      Iron: null,
      Silver: null,
      Gold: null),
    new Saved.WorldSavedOreTierTileCounts(
      CopperVanillaCount: 10,
      CopperAlternateCount: 10,
      IronVanillaCount: 20,
      IronAlternateCount: 10,
      SilverVanillaCount: 30,
      SilverAlternateCount: 30,
      GoldVanillaCount: 40,
      GoldAlternateCount: 20));
  Assert(loaded == new Saved.OreTierState(166, 6, 168, 8, -1, -1, -1),
    "World-file load must apply version sentinels before low-tier repair.");
  Assert(Saved.WorldSavedOreTierQuery.Snapshot(state) == loaded,
    "World-file load must publish the repaired snapshot exactly once.");

  Saved.OreTierState beforeFailure = state.Value;
  AssertThrows<ArgumentException>(
    () => Saved.WorldSavedOreTierLoadSystem.Load(
      state,
      adapter,
      new Saved.WorldSavedOreTierFileInput(
        VersionNumber: Saved.WorldFileSavedOreTierAdapter.FirstFourDirectReadVersion,
        AltarCount: 1,
        Cobalt: 107,
        Mythril: null,
        Adamantite: 111,
        Copper: 7,
        Iron: 6,
        Silver: 9,
        Gold: 8),
      default),
    "A missing direct-read field must fail before state commit.");
  Assert(state.Value == beforeFailure,
    "A failed world-file read must preserve the previous snapshot.");
}

static void VerifyMountainCaveAppendAfterCommitBoundary()
{
  MountainCaveHistoryComponent component = new(generationId: 7);
  MountainCaveHistoryAppendResult rejected =
    MountainCaveHistorySystem.AppendAfterSuccessfulCommit(
      component,
      x: 120,
      y: 80,
      caveCommitted: false);
  Assert(rejected.Status == MountainCaveHistoryAppendStatus.RejectedCommit &&
         !rejected.Appended && component.Count == 0,
    "A failed cave commit must not append history.");

  MountainCaveHistoryAppendResult appended =
    MountainCaveHistorySystem.AppendAfterSuccessfulCommit(
      component,
      x: 120,
      y: 80,
      caveCommitted: true);
  Assert(appended.Status == MountainCaveHistoryAppendStatus.Appended &&
         appended.Appended && component.Count == 1,
    "A successful cave commit must append one history entry.");

  MountainCaveHistorySnapshot snapshot =
    MountainCaveHistoryQuery.Snapshot(component);
  Assert(snapshot.XOrigins[0] == 120 && snapshot.YOrigins[0] == 80,
    "Mountain cave history must preserve paired insertion order.");

  for (int index = component.Count; index < MountainCaveHistoryComponent.Capacity; index++)
  {
    Assert(
      MountainCaveHistorySystem.TryAppendAfterSuccessfulCommit(
        component,
        index,
        index + 1,
        caveCommitted: true),
      "A successful cave commit must append while capacity remains.");
  }

  MountainCaveHistoryAppendResult full =
    MountainCaveHistorySystem.AppendAfterSuccessfulCommit(
      component,
      x: 999,
      y: 1000,
      caveCommitted: true);
  Assert(full.Status == MountainCaveHistoryAppendStatus.RejectedCapacity &&
         !full.Appended && component.Count == MountainCaveHistoryComponent.Capacity,
    "A full cave history must reject without changing its bounded prefix.");
}

static void VerifySurfaceHistoryAcceptanceBoundaries()
{
  SurfaceTunnelHistoryComponent tunnels = new(generationId: 7);
  SurfaceTunnelHistoryAppendResult rejectedScan =
    SurfaceTunnelHistorySystem.AppendAfterSuccessfulScan(
      tunnels,
      centerX: 400,
      scanAccepted: false);
  Assert(rejectedScan.Status == SurfaceTunnelHistoryAppendStatus.RejectedScan &&
         !rejectedScan.Appended && tunnels.Count == 0,
    "A rejected ten-point tunnel scan must not append history.");

  SurfaceTunnelHistoryAppendResult acceptedScan =
    SurfaceTunnelHistorySystem.AppendAfterSuccessfulScan(
      tunnels,
      centerX: 420,
      scanAccepted: true);
  Assert(acceptedScan.Appended && tunnels.Count == 1 &&
         SurfaceTunnelHistoryQuery.Snapshot(tunnels).CenterX[0] == 420,
    "An accepted tunnel scan must record its center before tile effects.");

  for (int index = tunnels.Count;
       index < SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity;
       index++)
  {
    Assert(
      SurfaceTunnelHistorySystem.TryAppendAfterSuccessfulScan(
        tunnels,
        index,
        scanAccepted: true),
      "Accepted tunnel scans must append while the effective capacity remains.");
  }

  SurfaceTunnelHistoryAppendResult fullTunnelHistory =
    SurfaceTunnelHistorySystem.AppendAfterSuccessfulScan(
      tunnels,
      centerX: 999,
      scanAccepted: true);
  Assert(fullTunnelHistory.Status == SurfaceTunnelHistoryAppendStatus.RejectedCapacity &&
         fullTunnelHistory.ScanAccepted && !fullTunnelHistory.Appended &&
         tunnels.Count == SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity,
    "Tunnel history must preserve the source capacity-minus-one boundary.");

  SurfaceOrePatchHistoryComponent orePatches = new(generationId: 7);
  SurfaceOrePatchHistoryAppendResult rejectedPatch =
    SurfaceOrePatchHistorySystem.AppendAfterSuccessfulCommit(
      orePatches,
      x: 600,
      patchCommitted: false);
  Assert(rejectedPatch.Status == SurfaceOrePatchHistoryAppendStatus.RejectedCommit &&
         !rejectedPatch.Appended && orePatches.Count == 0,
    "A failed OrePatch call must not append history.");

  SurfaceOrePatchHistoryAppendResult committedPatch =
    SurfaceOrePatchHistorySystem.AppendAfterSuccessfulCommit(
      orePatches,
      x: 620,
      patchCommitted: true);
  Assert(committedPatch.Appended && orePatches.Count == 1 &&
         SurfaceOrePatchHistoryQuery.Snapshot(orePatches).PatchX[0] == 620,
    "A successful OrePatch call must append its X coordinate.");

  for (int index = orePatches.Count;
       index < SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity;
       index++)
  {
    Assert(
      SurfaceOrePatchHistorySystem.TryAppendAfterSuccessfulCommit(
        orePatches,
        index,
        patchCommitted: true),
      "Successful OrePatch calls must append while the effective capacity remains.");
  }

  SurfaceOrePatchHistoryAppendResult fullOreHistory =
    SurfaceOrePatchHistorySystem.AppendAfterSuccessfulCommit(
      orePatches,
      x: 999,
      patchCommitted: true);
  Assert(fullOreHistory.Status == SurfaceOrePatchHistoryAppendStatus.RejectedCapacity &&
         fullOreHistory.ExternalCommitSucceeded && !fullOreHistory.Appended &&
         orePatches.Count == SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity,
    "A successful patch with full history must report the unrecorded external commit.");
}

VerifyLifecycleBoundary();
VerifyPassOrderingAndControlBoundary();
VerifyLayerMetricsSnapshotIsolation();
VerifyDungeonDerivedIndexBoundary();
VerifySavedOreTierRepairAndProjection();
VerifySavedOreTierLoadBoundary();
VerifyMountainCaveAppendAfterCommitBoundary();
VerifySurfaceHistoryAcceptanceBoundaries();

Console.WriteLine(
  "PASS: P17 lifecycle, ordered pass commit, C02, C13, and C14 core slice");
