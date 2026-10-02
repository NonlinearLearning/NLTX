using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

WorldGenerationPlanComponent plan = new(
  7,
  1,
  new[]
  {
    new GenerationPassDescriptor("terrain", 2.0),
    new GenerationPassDescriptor("structures", 1.0),
  },
  new[] { "structures" });
var manifest = new WorldGenerationManifestComponent("v1", "test");
WorldGenerationPassStateComponent state = new(7);
var progress = new WorldGenerationProgressComponent(
  messageNoFormatting: "Generating %",
  value: 0.25,
  totalWeightedProgress: 9,
  totalWeight: 99,
  currentPassWeight: 4);
WorldGenerationPassExecutionSystem.RefreshCommittedProgress(
  plan,
  in state,
  manifest,
  progress);
Assert(progress.TotalWeight == 2 && progress.TotalWeightedProgress == 0,
  "Committed progress should total only enabled plan entries.");
Assert(progress.Value == 0.25 && progress.CurrentPassWeight == 4 &&
       progress.MessageNoFormatting == "Generating %",
  "Refresh should preserve pass-local progress and message state.");
Assert(WorldGenerationPassExecutionSystem.TryGetNextPass(
    plan,
    in state,
    manifest,
    out GenerationPassDescriptor nextPass) && nextPass.Id == "terrain",
  "The next pass should follow the explicit plan order.");

WorldGenerationPassStartResult started = WorldGenerationPassExecutionSystem.BeginPass(
  plan,
  in state,
  "terrain",
  WorldGenerationStage.Terrain,
  10);
Assert(started.Started && started.State.ActivePassId == "terrain",
  "The enabled pass should become active.");
WorldGenerationPassStateComponent activePassState = started.State;
Assert(WorldGenerationPassExecutionSystem.TryGetNextPass(
    plan,
    in activePassState,
    manifest,
    out nextPass) && nextPass.Id == "terrain",
  "The active pass should remain the next uncommitted plan entry.");

WorldGenerationConfigurationSnapshot configuration = new(8400, 2400, "seed", 1);
WorldGenerationPassExecutionCallback terrainCallback = (
  in WorldGenerationPassStateComponent passState,
  GenerationPassDescriptor descriptor,
  WorldGenerationConfigurationSnapshot passConfiguration,
  Action<string, double> reportProgress) =>
  {
    Assert(passState.ActivePassId == "terrain" && descriptor.Id == "terrain",
      "The runner should receive the active descriptor and state.");
    Assert(passConfiguration.SeedText == "seed",
      "The runner should receive the explicit configuration snapshot.");
    reportProgress("Generating terrain %", 0.6);
    return new WorldGenerationPassRunOutput(
      durationMs: 12,
      hash: 42,
      randomNextValue: 173);
  };
var runner = new RegisteredWorldGenerationPassRunner(
  new[]
  {
    new KeyValuePair<string, WorldGenerationPassExecutionCallback>(
      "terrain",
      terrainCallback),
  });
WorldGenerationPassExecutionAttempt attempt =
  WorldGenerationPassExecutionSystem.ExecuteActivePass(
    plan,
    in activePassState,
    manifest,
    progress,
    configuration,
    runner,
    WorldGenerationStage.Structure,
    checkpointRevision: 11);
Assert(attempt.Succeeded,
  "A successful runner call should produce a committed pass result.");
WorldGenerationPassResultComponent firstResult = attempt.PassResult ??
  throw new InvalidOperationException("The successful pass result was missing.");
state = attempt.State;
Assert(manifest.PassResults.Count == 1 && manifest.PassResults[0] == firstResult,
  "An active pass result should append once in plan order.");
Assert(progress.TotalWeight == 2 && progress.TotalWeightedProgress == 2,
  "Committed weighted progress should include enabled results in the prefix.");
Assert(progress.Value == 0 && progress.CurrentPassWeight == 2 &&
       progress.MessageNoFormatting == "Generating terrain %",
  "A completed pass should reset local progress and keep its final message.");
Assert(firstResult.DurationMs == 12 && firstResult.Hash == 42 &&
       firstResult.RandomNextValue == 173,
  "The runner measurements should be committed without changing their values.");
var catalog = new WorldGenerationPassCatalog(
  7,
  1,
  new[]
  {
    new WorldGenerationPassRegistration(
      new GenerationPassDescriptor("terrain", 2.0),
      terrainCallback),
    new WorldGenerationPassRegistration(
      new GenerationPassDescriptor("structures", 1.0),
      callback: null,
      enabled: false),
  });
Assert(catalog.Plan.PassDescriptors[0].Id == "terrain" &&
       catalog.Plan.PassDescriptors[1].Id == "structures" &&
       catalog.Plan.DisabledPassIds.Count == 1 &&
       catalog.Plan.DisabledPassIds[0] == "structures",
  "The catalog should preserve registration order and disabled pass identity.");
WorldGenerationPassRunOutput catalogOutput = catalog.Runner.Execute(
  in activePassState,
  catalog.Plan.PassDescriptors[0],
  configuration,
  (_, _) => { });
Assert(catalogOutput.DurationMs == 12 && catalogOutput.Hash == 42 &&
       catalogOutput.RandomNextValue == 173,
  "The catalog runner should dispatch the registered callback.");
Assert(state.Stage == WorldGenerationStage.Structure &&
       state.CheckpointRevision == 11 &&
       !state.HasActivePass,
  "Result commit should complete the pass state and publish its checkpoint.");
Assert(WorldGenerationPassExecutionSystem.TryGetNextPass(
    plan,
    in state,
    manifest,
    out nextPass) && nextPass.Id == "structures",
  "The next uncommitted pass should advance only after result commit.");

WorldGenerationPassStartResult skipped = WorldGenerationPassExecutionSystem.BeginPass(
  plan,
  in state,
  "structures",
  WorldGenerationStage.Structure,
  11);
Assert(skipped.Skipped && !skipped.State.HasActivePass,
  "A disabled pass should not become active.");
var skippedResult = new WorldGenerationPassResultComponent(
  7,
  1,
  "structures",
  skipped: true);
WorldGenerationPassExecutionSystem.CommitSkippedPassResult(
  plan,
  in state,
  manifest,
  skippedResult);
Assert(manifest.PassResults.Count == 2 && manifest.PassResults[1] == skippedResult,
  "A caller-provided skipped result should commit in the next plan slot.");
Assert(!WorldGenerationPassExecutionSystem.TryGetNextPass(
    plan,
    in state,
    manifest,
    out _),
  "A fully committed plan should have no next pass.");

var duplicateCommitRejected = false;
try
{
  WorldGenerationPassExecutionSystem.CommitSkippedPassResult(
    plan,
    in state,
    manifest,
    skippedResult);
}
catch (InvalidOperationException)
{
  duplicateCommitRejected = true;
}

Assert(duplicateCommitRejected && manifest.PassResults.Count == 2,
  "A duplicate commit should be rejected without appending another result.");

WorldGenerationPlanComponent failingPlan = new(
  8,
  1,
  new[] { new GenerationPassDescriptor("failing", 1.0) });
WorldGenerationManifestComponent failingManifest = new("v1", "test");
WorldGenerationPassStateComponent failingState = new(8);
WorldGenerationPassStartResult failingStart = WorldGenerationPassExecutionSystem.BeginPass(
  failingPlan,
  in failingState,
  "failing",
  WorldGenerationStage.Terrain,
  readSnapshotRevision: 1);
WorldGenerationPassStateComponent failingActiveState = failingStart.State;
var failingProgress = new WorldGenerationProgressComponent();
WorldGenerationPassExecutionCallback failingCallback = (
  in WorldGenerationPassStateComponent passState,
  GenerationPassDescriptor descriptor,
  WorldGenerationConfigurationSnapshot passConfiguration,
  Action<string, double> reportProgress) =>
  {
    reportProgress("Halfway %", 0.5);
    throw new InvalidOperationException("pass failed");
  };
var failingRunner = new RegisteredWorldGenerationPassRunner(
  new[]
  {
    new KeyValuePair<string, WorldGenerationPassExecutionCallback>(
      "failing",
      failingCallback),
  });
WorldGenerationPassExecutionAttempt failedAttempt =
  WorldGenerationPassExecutionSystem.ExecuteActivePass(
    failingPlan,
    in failingActiveState,
    failingManifest,
    failingProgress,
    configuration,
    failingRunner,
    WorldGenerationStage.Structure,
    checkpointRevision: 2);
Assert(!failedAttempt.Succeeded && failedAttempt.Failure is InvalidOperationException &&
       failedAttempt.State.HasFailure && failingManifest.PassResults.Count == 0 &&
       failingProgress.Value == 0.5,
  "A pass failure should be recorded without committing a success result.");

bool duplicateCallbackRejected = false;
try
{
  new RegisteredWorldGenerationPassRunner(
    new[]
    {
      new KeyValuePair<string, WorldGenerationPassExecutionCallback>(
        "terrain",
        terrainCallback),
      new KeyValuePair<string, WorldGenerationPassExecutionCallback>(
        "terrain",
        terrainCallback),
    });
}
catch (ArgumentException)
{
  duplicateCallbackRejected = true;
}

Assert(duplicateCallbackRejected,
  "Duplicate pass callback registrations should be rejected.");

bool duplicateCatalogRegistrationRejected = false;
try
{
  new WorldGenerationPassCatalog(
    9,
    1,
    new[]
    {
      new WorldGenerationPassRegistration(
        new GenerationPassDescriptor("duplicate", 1.0),
        terrainCallback),
      new WorldGenerationPassRegistration(
        new GenerationPassDescriptor("duplicate", 1.0),
        terrainCallback),
    });
}
catch (ArgumentException)
{
  duplicateCatalogRegistrationRejected = true;
}

Assert(duplicateCatalogRegistrationRejected,
  "Duplicate catalog registrations should be rejected.");

Console.WriteLine("PASS: registered world-generation pass dispatch and commit boundary");
