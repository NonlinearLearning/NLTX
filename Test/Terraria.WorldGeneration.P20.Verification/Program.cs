using Terraria.Content;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Structures;
using Terraria.WorldGeneration.Systems;
using System.Threading;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void ActionExecutionPreservesOrderAndFailurePolicy()
{
  WorldGenerationAction[] actions =
  [
    new(
      7,
      1,
      WorldGenerationActionPayload.FromTileSet(
        WorldGenerationTileSetActionsCommand.SetTile(
          new TilePosition(1, 1),
          3))),
    new(
      7,
      2,
      WorldGenerationActionPayload.FromWallMutation(
        WorldGenerationWallMutationActionsCommand.SetWall(
          new TilePosition(1, 1),
          4))),
    new(
      7,
      3,
      WorldGenerationActionPayload.FromTilePlacementAndPaint(
        WorldGenerationTilePlacementAndPaintActionsCommand.PlaceTile(
          new TilePosition(2, 1),
          5))),
  ];
  RecordingActionCommitPort port = new(rejectedSequence: 2);

  WorldGenerationActionExecutionResult continued =
    WorldGenerationActionExecutionSystem.Execute(
      7,
      actions,
      port,
      WorldGenerationActionFailurePolicy.ContinueAfterFailure);

  Assert(continued.Records.Count == 3, "Continue policy must visit every action.");
  Assert(continued.GenerationId == 7, "Execution results must preserve generation scope.");
  Assert(continued.FirstFailedIndex == 1, "The first failed action index must be recorded.");
  Assert(!continued.Succeeded, "A rejected commit must make the execution unsuccessful.");
  Assert(continued.ContinuedAfterFailure, "Continue policy must expose continued execution.");
  Assert(continued.UncommittedActions.Count == 0, "Continue policy must leave no uncommitted actions.");
  Assert(
    port.Sequences.SequenceEqual([1L, 2L, 3L]),
    "The commit port must observe the source action order.");

  port = new RecordingActionCommitPort(rejectedSequence: 2);
  WorldGenerationActionExecutionResult stopped =
    WorldGenerationActionExecutionSystem.Execute(
      7,
      actions,
      port,
      WorldGenerationActionFailurePolicy.StopOnFailure);

  Assert(stopped.StoppedOnFailure, "Stop policy must stop at the first rejected action.");
  Assert(stopped.Records.Count == 2, "Stop policy must not commit later actions.");
  Assert(
    stopped.UncommittedActions.Count == 1 && stopped.UncommittedActions[0].Sequence == 3,
    "Stop policy must retain the later action as uncommitted.");
}

static void ActionCommitRouterSelectsOneOwnerPort()
{
  RoutingRecorder recorder = new();
  WorldGenerationActionCommitRouter router = new(
    new WorldGenerationActionCommitRouter.Ports(
      recorder,
      recorder,
      recorder,
      recorder,
      recorder,
      recorder));

  WorldGenerationAction tileAction = new(
    9,
    4,
    WorldGenerationActionPayload.FromTileSet(
      WorldGenerationTileSetActionsCommand.SetTile(
        new TilePosition(4, 5),
        7)));
  WorldGenerationAction liquidAction = new(
    9,
    5,
    WorldGenerationActionPayload.FromLiquidAndNeighbor(
      WorldGenerationLiquidAndNeighborActionsCommand.SetLiquid(
        new TilePosition(4, 5),
        1,
        255)));

  Assert(router.Commit(in tileAction).Accepted, "Tile owner route must accept the action.");
  Assert(router.Commit(in liquidAction).Accepted, "Liquid owner route must accept the action.");
  Assert(
    recorder.Routes.SequenceEqual(["tile", "liquid"]),
    "Each action must route to exactly one owner port in source order.");
  Assert(recorder.LastGenerationId == 9 && recorder.LastSequence == 5,
    "The router must preserve generation and sequence identity.");
}

static void ActionPreparationAndProjectionsStaySeparated()
{
  WorldGenerationAction[] actions =
  [
    new(
      13,
      1,
      WorldGenerationActionPayload.FromTileSet(
        WorldGenerationTileSetActionsCommand.SetTile(
          new TilePosition(1, 1),
          3))),
  ];
  WorldGenerationActionExecutionBatch batch =
    WorldGenerationActionExecutionSystem.Prepare(13, actions);
  RecordingActionCommitPort port = new(rejectedSequence: long.MaxValue);

  Assert(port.Sequences.Count == 0, "Preparing a batch must not commit actions.");
  actions[0] = default;
  Assert(
    batch.Actions[0].Sequence == 1 && batch.Actions[0].GenerationId == 13,
    "The prepared batch must own an immutable copy of its action input.");

  WorldGenerationActionExecutionResult result =
    WorldGenerationActionExecutionSystem.Commit(batch, port);
  WorldGenerationActionResultProjection projection =
    WorldGenerationActionResultProjection.From(result);
  Assert(projection.RecordCount == 1, "The result projection must preserve records.");
  Assert(projection.GenerationId == 13, "The result projection must preserve generation scope.");
  Assert(projection.CommittedCount == 1, "The result projection must preserve commit counts.");
  Assert(projection.CompletedAllActions, "A successful batch must be complete.");

  WorldGenerationActionExecutionResult empty =
    new(
      Array.Empty<WorldGenerationActionExecutionRecord>(),
      firstFailedIndex: null,
      stoppedOnFailure: false);
  Assert(
    empty.UncommittedActions.Count == 0,
    "An omitted uncommitted action list must project as an empty list.");
  Assert(
    empty.GenerationId is null,
    "The compatibility result constructor must preserve unknown generation scope.");

  List<WorldGenerationActionExecutionRecord> mutableRecords =
  [
    new(1, WorldGenerationActionPayloadKind.TileSet, true, null),
  ];
  WorldGenerationActionExecutionResult copiedResult =
    new(mutableRecords, firstFailedIndex: null, stoppedOnFailure: false);
  mutableRecords.Clear();
  Assert(
    copiedResult.Records.Count == 1,
    "Execution results must defensively copy caller-owned record lists.");

  bool rejectedForeignAction = false;
  try
  {
    _ = new WorldGenerationActionExecutionResult(
      13,
      Array.Empty<WorldGenerationActionExecutionRecord>(),
      firstFailedIndex: null,
      stoppedOnFailure: true,
      uncommittedActions:
      [
        new(
          14,
          1,
          WorldGenerationActionPayload.FromTileSet(
            WorldGenerationTileSetActionsCommand.SetTile(
              new TilePosition(1, 1),
              2))),
      ]);
  }
  catch (ArgumentException)
  {
    rejectedForeignAction = true;
  }

  Assert(
    rejectedForeignAction,
    "A scoped result must reject uncommitted actions from another generation.");
}

static void ActionExecutionHonorsCancellationBarrier()
{
  WorldGenerationAction[] actions =
  [
    new(
      21,
      1,
      WorldGenerationActionPayload.FromTileSet(
        WorldGenerationTileSetActionsCommand.SetTile(
          new TilePosition(1, 1),
          3))),
    new(
      21,
      2,
      WorldGenerationActionPayload.FromWallMutation(
        WorldGenerationWallMutationActionsCommand.SetWall(
          new TilePosition(1, 1),
          4))),
    new(
      21,
      3,
      WorldGenerationActionPayload.FromTilePlacementAndPaint(
        WorldGenerationTilePlacementAndPaintActionsCommand.PlaceTile(
          new TilePosition(1, 1),
          5))),
  ];

  using CancellationTokenSource cancellation = new();
  RecordingActionCommitPort port = new(
    rejectedSequence: long.MaxValue,
    afterCommit: cancellation.Cancel);
  WorldGenerationActionExecutionResult result =
    WorldGenerationActionExecutionSystem.Execute(
      21,
      actions,
      port,
      cancellationToken: cancellation.Token);

  Assert(result.Cancelled, "Cancellation must stop the action commit barrier.");
  Assert(!result.Succeeded && !result.CompletedAllActions,
    "A cancelled execution cannot be reported as successful or complete.");
  Assert(result.CancellationReason == "CancellationRequested",
    "A cancellation observed between actions must expose its reason.");
  Assert(port.Sequences.SequenceEqual([1L]),
    "Cancellation must prevent the next action from reaching its owner port.");
  Assert(
    result.UncommittedActions.Count == 2 &&
    result.UncommittedActions[0].Sequence == 2 &&
    result.UncommittedActions[1].Sequence == 3,
    "Cancellation must preserve the current uncommitted suffix in source order.");

  WorldGenerationActionResultProjection projection =
    WorldGenerationActionResultProjection.From(result);
  Assert(projection.Cancelled &&
    projection.CancellationReason == "CancellationRequested",
    "The result projection must preserve cancellation state and reason.");

  WorldGenerationActionExecutionResult portCancelled =
    WorldGenerationActionExecutionSystem.Execute(
      21,
      actions,
      new ThrowingCancellationCommitPort());
  Assert(portCancelled.Cancelled &&
    portCancelled.CancellationReason == "CommitPortCancelled",
    "An owner port cancellation must become an explicit cancelled result.");
  Assert(
    portCancelled.UncommittedActions.Count == actions.Length &&
    portCancelled.UncommittedActions[0].Sequence == 1,
    "A cancellation thrown before acknowledgement must preserve the full suffix.");
}

static void PaintValidationPreservesReferenceFailureRules()
{
  WorldGenerationTilePlacementAndPaintActionsCommand invalidTilePaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.SetTilePaint(
      new TilePosition(2, 2),
      paintId: 0);
  Assert(!invalidTilePaint.IsWellFormed,
    "SetTilePaint must reject paint zero like the reference Fail() path.");

  bool threw = false;
  try
  {
    invalidTilePaint.Validate();
  }
  catch (ArgumentException)
  {
    threw = true;
  }

  Assert(threw, "An invalid SetTilePaint command must fail validation.");

  WorldGenerationTilePlacementAndPaintActionsCommand invalidWallPaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.SetWallPaint(
      new TilePosition(2, 2),
      paintId: 0);
  Assert(!invalidWallPaint.IsWellFormed,
    "SetWallPaint must reject paint zero like the reference Fail() path.");

  WorldGenerationTilePlacementAndPaintActionsCommand invalidCombinedPaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.SetTileAndWallPaint(
      new TilePosition(2, 2),
      paintId: 0);
  Assert(!invalidCombinedPaint.IsWellFormed,
    "SetTileAndWallPaint must reject paint zero like the reference Fail() path.");
  threw = false;
  try
  {
    invalidCombinedPaint.Validate();
  }
  catch (ArgumentException)
  {
    threw = true;
  }

  Assert(threw, "An invalid combined paint command must fail validation.");

  WorldGenerationTilePlacementAndPaintActionsCommand clearTilePaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.ClearTilePaint(
      new TilePosition(2, 2));
  clearTilePaint.Validate();
  Assert(
    clearTilePaint.Kind ==
      WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.ClearTilePaint &&
    clearTilePaint.PaintId == 0,
    "ClearTilePaint must expose an explicit zero-paint clear operation.");

  WorldGenerationTilePlacementAndPaintActionsCommand clearWallPaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.ClearWallPaint(
      new TilePosition(2, 2));
  clearWallPaint.Validate();
  Assert(
    clearWallPaint.Kind ==
      WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.ClearWallPaint,
    "ClearWallPaint must remain a distinct operation from SetWallPaint.");

  WorldGenerationTilePlacementAndPaintActionsCommand clearCombinedPaint =
    WorldGenerationTilePlacementAndPaintActionsCommand.ClearTileAndWallPaint(
      new TilePosition(2, 2));
  clearCombinedPaint.Validate();
  Assert(
    clearCombinedPaint.Kind ==
      WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.ClearTileAndWallPaint,
    "ClearTileAndWallPaint must remain a distinct operation from SetTileAndWallPaint.");
}

static void WallMutationPreservesRemoveWallIntent()
{
  WorldGenerationWallMutationActionsCommand clearWall =
    WorldGenerationWallMutationActionsCommand.ClearWall(
      new TilePosition(3, 4),
      frameNeighbors: true);
  WorldGenerationWallMutationActionsCommand removeWall =
    WorldGenerationWallMutationActionsCommand.RemoveWall(
      new TilePosition(3, 4));

  clearWall.Validate();
  removeWall.Validate();
  Assert(
    clearWall.Kind ==
      WorldGenerationWallMutationActionsCommand.OperationKind.ClearWall &&
    clearWall.FrameNeighbors,
    "ClearWall must preserve its optional neighbor-framing intent.");
  Assert(
    removeWall.Kind ==
      WorldGenerationWallMutationActionsCommand.OperationKind.RemoveWall &&
    removeWall.WallType == 0 &&
    !removeWall.FrameSelf &&
    !removeWall.FrameNeighbors,
    "RemoveWall must remain a distinct direct wall-zero intent without framing.");
}

static void StructureReservationIsAtomicAndGenerationScoped()
{
  GridTileSnapshotReader reader = new(new WorldGenerationRectangle(0, 0, 10, 10));
  HashSet<ushort> validTiles = [1];
  WorldStructureReservationSystem system = new();

  ReservationResult first = system.Reserve(
    new WorldStructureReservationRequest(
      11,
      "first",
      new WorldGenerationRectangle(2, 2, 2, 2),
      Padding: 1,
      IsProtected: true),
    reader,
    validTiles);
  Assert(first.Accepted, "The first protected reservation should be accepted.");

  ReservationResult overlap = system.Reserve(
    new WorldStructureReservationRequest(
      11,
      "overlap",
      new WorldGenerationRectangle(3, 3, 1, 1)),
    reader,
    validTiles);
  Assert(!overlap.Accepted, "A protected overlap must be rejected atomically.");

  ReservationResult otherGeneration = system.Reserve(
    new WorldStructureReservationRequest(
      12,
      "same-coordinates",
      new WorldGenerationRectangle(3, 3, 1, 1)),
    reader,
    validTiles);
  Assert(
    otherGeneration.Accepted,
    "Reservations in another generation must not conflict.");

  reader.Set(
    new TilePosition(8, 8),
    new StructureTileSnapshot(true, true, 9));
  ReservationResult invalidTile = system.Reserve(
    new WorldStructureReservationRequest(
      11,
      "invalid-tile",
      new WorldGenerationRectangle(8, 8, 1, 1)),
    reader,
    validTiles);
  Assert(!invalidTile.Accepted, "An active invalid tile must reject placement.");

  WorldStructureReservationSnapshot snapshot = system.CreateSnapshot(11);
  Assert(snapshot.Count == 1, "Rejected reservations must not enter the snapshot.");
  Assert(system.Reset(11) == 1, "Reset must release only the selected generation's protection.");
  WorldStructureReservationSnapshot afterReset = system.CreateSnapshot(11);
  Assert(
    afterReset.Count == 1 && !afterReset.Reservations[0].Request.IsProtected,
    "Reset must retain ordinary structure history while releasing protection.");
  ReservationResult afterResetPlacement = system.Reserve(
    new WorldStructureReservationRequest(
      11,
      "after-reset",
      new WorldGenerationRectangle(2, 2, 2, 2),
      Padding: 1,
      IsProtected: true),
    reader,
    validTiles);
  Assert(
    afterResetPlacement.Accepted,
    "A protected reservation may reuse space after protection is reset.");
  Assert(system.CreateSnapshot(12).Count == 1, "Other generations must remain intact.");
  Assert(system.DiscardGeneration(12) == 1, "Discard must remove only the requested generation.");
  Assert(system.CreateSnapshot(12).Count == 0, "Discarded generation must have no reservations.");
  Assert(system.CreateSnapshot(11).Count == 2, "Discard must preserve other generation state.");
}

static void WorldGenRangePreservesInclusiveScaling()
{
  RecordingRandomSource random = new(12);
  WorldGenRangeQuery.Result result = WorldGenRangeQuery.GetRandom(
    new WorldGenRangeQuery.Input(
      10,
      12,
      WorldGenRangeQuery.ScalingMode.None,
      4200,
      1200,
      GenerationRandomStream.WorldGeneration),
    random);

  Assert(result.ScaledMinimum == 10, "None scaling must preserve the minimum.");
  Assert(result.ScaledMaximum == 12, "None scaling must preserve the maximum.");
  Assert(result.Value == 12, "The explicit random value must be returned.");
  Assert(
    random.Requests.SequenceEqual([(GenerationRandomStream.WorldGeneration, 10, 13)]),
    "WorldGenRange must pass an inclusive maximum as an exclusive upper bound.");

  Assert(
    WorldGenRangeQuery.ScaleValue(
      3,
      WorldGenRangeQuery.ScalingMode.WorldWidth,
      8400,
      1200) == 6,
    "WorldWidth scaling must use the 4200-tile reference width.");
}

static void ReservationSnapshotProjectionPreservesScope()
{
  GridTileSnapshotReader reader = new(new WorldGenerationRectangle(0, 0, 10, 10));
  WorldStructureReservationSystem system = new();
  ReservationResult result = system.Reserve(
    new WorldStructureReservationRequest(
      17,
      "projection",
      new WorldGenerationRectangle(2, 2, 2, 2),
      Padding: 1,
      IsProtected: true),
    reader,
    new HashSet<ushort> { 1 });
  Assert(result.Accepted, "The projection fixture reservation must be accepted.");

  WorldStructureReservationSnapshotProjection projection =
    WorldStructureReservationSnapshotProjection.From(
      system.CreateSnapshot(17));
  Assert(projection.GenerationId == 17 && projection.Count == 1,
    "The reservation projection must preserve generation scope and count.");
  Assert(
    projection.Reservations[0].ReservedBounds ==
      new WorldGenerationRectangle(1, 1, 4, 4),
    "The reservation projection must preserve padded bounds.");
}

static void ShapeTraversalPreservesChainAndOutputOrder()
{
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape =
    new(
    [
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
    ]);
  int secondActionCalls = 0;
  ShapeTraversalQuery.Result result = ShapeTraversalQuery.Execute(
    shape,
    new TilePosition(10, 20),
    [
      new ShapeTraversalQuery.ActionDescriptor(
        "first",
        _ => ShapeTraversalQuery.ActionResult.Success()),
      new ShapeTraversalQuery.ActionDescriptor(
        "second",
        position =>
        {
          secondActionCalls++;
          return position.X == 11
            ? ShapeTraversalQuery.ActionResult.Failure("intentional")
            : ShapeTraversalQuery.ActionResult.Success();
        }),
    ],
    new WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy(QuitOnFail: false));

  Assert(result.Points.Length == 2, "Shape data must visit unique ordered points.");
  Assert(result.ShapeOutput.Length == 2, "Shape output must record each visited point.");
  Assert(result.FirstFailedPointIndex == 1, "The first failed shape point must be recorded.");
  Assert(!result.Succeeded && result.Completed, "Continue mode must complete after failure.");
  Assert(secondActionCalls == 2, "The second action must run once per visited point.");
  Assert(result.ActionOutputs[0].RelativePoints.Length == 2, "The first action output must include both points.");
  Assert(result.ActionOutputs[1].RelativePoints.Length == 1, "A failed action must not write action output.");

  secondActionCalls = 0;
  ShapeTraversalQuery.Result stopped = ShapeTraversalQuery.Execute(
    shape,
    new TilePosition(10, 20),
    [
      new ShapeTraversalQuery.ActionDescriptor(
        "first",
        _ => ShapeTraversalQuery.ActionResult.Success()),
      new ShapeTraversalQuery.ActionDescriptor(
        "second",
        _ =>
        {
          secondActionCalls++;
          return ShapeTraversalQuery.ActionResult.Failure("intentional");
        }),
    ],
    new WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy(QuitOnFail: true));

  Assert(stopped.StoppedOnFailure, "QuitOnFail must stop shape traversal.");
  Assert(stopped.Points.Length == 1, "QuitOnFail must stop before the next shape point.");
  Assert(secondActionCalls == 1, "Stopped traversal must not evaluate later points.");

  ShapeTraversalQuery.Result callbackStopped = ShapeTraversalQuery.Execute(
    shape,
    new TilePosition(10, 20),
    [
      new ShapeTraversalQuery.ActionDescriptor(
        "throws",
        _ => throw new InvalidOperationException("callback failure")),
    ],
    new WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy(QuitOnFail: false),
    ShapeTraversalQuery.OutputPolicy.LegacyCompatible,
    GenerationRandomStream.WorldGeneration,
    ShapeTraversalQuery.CallbackExceptionPolicy.TreatAsFailureAndStop);

  Assert(
    callbackStopped.StoppedOnFailure && callbackStopped.Points.Length == 1,
    "TreatAsFailureAndStop must stop even when QuitOnFail is false.");
}

static void ConditionSearchPreservesAllAnyAndNotFound()
{
  WorldGenerationConditionsAndSearchesQuery.TileWorldSnapshot snapshot =
    new(
      new WorldGenerationConditionsAndSearchesQuery.TileWorldBounds(4, 1),
      new Dictionary<TilePosition, WorldGenerationConditionsAndSearchesQuery.TileSnapshot>
      {
        [new TilePosition(0, 0)] =
          new(true, true, 1, false, 0, 0, false),
        [new TilePosition(1, 0)] =
          new(true, true, 2, false, 0, 0, false),
        [new TilePosition(2, 0)] =
          new(true, true, 3, false, 0, 0, false),
        [new TilePosition(3, 0)] =
          new(true, false, 0, false, 0, 0, false),
      });

  WorldGenerationConditionsAndSearchesQuery.SearchDefinition allSearch =
    new WorldGenerationConditionsAndSearchesQuery.Searches.Right(
      4,
      WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(1),
      WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(2));
  WorldGenerationConditionsAndSearchesQuery.SearchResult allResult =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(0, 0),
      allSearch);
  Assert(
    !allResult.Found &&
      allResult.TerminationReason ==
        WorldGenerationConditionsAndSearchesQuery.SearchTerminationReason.NoMatch,
    "RequireAll default must reject a point that matches only one condition.");

  WorldGenerationConditionsAndSearchesQuery.SearchDefinition anySearch =
    allSearch.RequireAll(false);
  WorldGenerationConditionsAndSearchesQuery.SearchResult anyResult =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(0, 0),
      anySearch);
  Assert(
    anyResult.Found && anyResult.Position == new TilePosition(0, 0),
    "RequireAll(false) must accept the first point matching any condition.");

  WorldGenerationConditionsAndSearchesQuery.SearchResult notFound =
    WorldGenerationConditionsAndSearchesQuery.Find(
      snapshot,
      new TilePosition(0, 0),
      new WorldGenerationConditionsAndSearchesQuery.Searches.Right(
        4,
        WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(99)));
  Assert(
    !notFound.Found &&
      notFound.Position == WorldGenerationConditionsAndSearchesQuery.NOT_FOUND,
    "Find must preserve the NOT_FOUND sentinel when no condition matches.");

  bool found = WorldGenerationSearchCompatibilityAdapter.TryFind(
    snapshot,
    new TilePosition(0, 0),
    anySearch,
    out TilePosition foundPosition);
  Assert(
    found && foundPosition == new TilePosition(0, 0),
    "The compatibility adapter must map a found search to bool plus position.");

  bool missing = WorldGenerationSearchCompatibilityAdapter.TryFind(
    snapshot,
    new TilePosition(0, 0),
    new WorldGenerationConditionsAndSearchesQuery.Searches.Right(
      4,
      WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(99)),
    out TilePosition missingPosition);
  Assert(
    !missing && missingPosition == WorldGenerationConditionsAndSearchesQuery.NOT_FOUND,
    "The compatibility adapter must map no match to false plus NOT_FOUND.");
}

static void ShapeModifiersKeepExplicitCoordinatesAndRandomInputs()
{
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape =
    new(
    [
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(-1, 0),
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
    ]);

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition scaled =
    WorldGenerationShapeModifierStateDefinitionQuery.Scale(
      shape,
      new WorldGenerationShapeModifierStateDefinitionQuery.ShapeScaleDefinition(2));
  string scaledPoints = string.Join(';', scaled.Points);
  Assert(
    scaled.Count == 12 && scaled.Contains(
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(-2, 1)),
    $"ShapeScale must expand relative coordinates with the reference doubling rule " +
    $"(count={scaled.Count}, points={scaledPoints}).");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition expanded =
    WorldGenerationShapeModifierStateDefinitionQuery.Expand(
      new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(
        [new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0)]),
      new WorldGenerationShapeModifierStateDefinitionQuery.ExpandDefinition(1, 1));
  Assert(expanded.Count == 9, "Expand must include the complete inclusive rectangle.");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition transformed =
    WorldGenerationShapeModifierStateDefinitionQuery.Flip(
      WorldGenerationShapeModifierStateDefinitionQuery.Offset(
        shape,
        new WorldGenerationShapeModifierStateDefinitionQuery.OffsetDefinition(2, -3)),
      new WorldGenerationShapeModifierStateDefinitionQuery.FlipDefinition(true, true));
  Assert(
    transformed.Contains(
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(-1, 3)),
    "Offset and Flip must operate on explicit relative coordinates.");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition masked =
    WorldGenerationShapeModifierStateDefinitionQuery.RectangleMask(
      expanded,
      new WorldGenerationShapeModifierStateDefinitionQuery.RectangleMaskDefinition(
        -1,
        -1,
        1,
        1));
  Assert(masked.Count == 9, "RectangleMask must retain the inclusive configured bounds.");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition dropped =
    WorldGenerationShapeModifierStateDefinitionQuery.Dither(
      shape,
      new WorldGenerationShapeModifierStateDefinitionQuery.DitherDefinition(0.5),
      0.25);
  Assert(dropped.Count == 0, "Dither must drop the shape when the explicit draw fails.");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition radial =
    WorldGenerationShapeModifierStateDefinitionQuery.RadialDither(
      new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(
        [
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(2, 0),
        ]),
      new WorldGenerationShapeModifierStateDefinitionQuery.RadialDitherDefinition(0, 2),
      new WorldGenerationShapeModifierStateDefinitionQuery.RadialDitherRandomInput(1));
  string radialPoints = string.Join(';', radial.Points);
  Assert(
    radial.Count == 1 && radial.Contains(
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0)),
    $"RadialDither must use the explicit draw against the clamped distance threshold " +
    $"(count={radial.Count}, points={radialPoints}).");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition blotched =
    WorldGenerationShapeModifierStateDefinitionQuery.Blotches(
      new WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition(
        [new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0)]),
      new WorldGenerationShapeModifierStateDefinitionQuery.BlotchesDefinition(
        1,
        1,
        1,
        1,
        0.5),
      new WorldGenerationShapeModifierStateDefinitionQuery.BlotchesRandomInput(
        0.25,
        0,
        0,
        0,
        0));
  Assert(
    blotched.Count == 1 && blotched.Contains(
      new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0)),
    "Blotches must apply the supplied inclusive offsets without ambient random state.");
}

static void TileScanQueryPreservesLegacyCountSemantics()
{
  WorldGenerationTileScanQuery.TileObservation[] observations =
  [
    new(new TilePosition(0, 0), true, 4),
    new(new TilePosition(1, 0), false, 4),
    new(new TilePosition(2, 0), true, 5),
    new(new TilePosition(3, 0), true, 4),
  ];

  WorldGenerationTileScanQuery.Result result =
    WorldGenerationTileScanQuery.Count(observations, [4, 4, 7]);

  Assert(result.TotalMatches == 2, "TileScan must count only active requested tiles.");
  Assert(result.GetCount(4) == 2, "TileScan must preserve the requested tile count.");
  Assert(result.GetCount(7) == 0, "TileScan must initialize requested absent tiles to zero.");
  Assert(result.GetCount(5) == -1, "TileScan must preserve the unknown tile sentinel.");
  Assert(
    result.Counts is System.Collections.Frozen.FrozenDictionary<ushort, int>,
    "TileScan results must be immutable after the query returns.");
}

static void TileScanAndControlSystemPreservesReferenceOrder()
{
  WorldGenerationTileScanQuery.TileObservation observation =
    new(new TilePosition(4, 5), true, 9);

  RecordingCountSink countSink = new();
  WorldGenerationTileScanAndControlActionsCommand countCommand =
    WorldGenerationTileScanAndControlActionsCommand.Count(countSink);
  WorldGenerationTileScanAndControlSystem.Result countResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      countCommand,
      observation);
  Assert(countResult.Accepted && countResult.CountUpdated,
    "Count actions must update the explicit sink and accept the unit action.");
  Assert(countSink.Values.SequenceEqual([1]),
    "Count actions must add exactly one value per visited tile.");

  RecordingContinuation continuation = new(false);
  WorldGenerationTileScanAndControlSystem.Result continuationResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      WorldGenerationTileScanAndControlActionsCommand.Continue(continuation),
      observation);
  Assert(continuationResult.Accepted,
    "ContinueWrapper must retain the UnitApply success when no NextAction exists.");
  Assert(
    continuationResult.CallbackResult == false && continuationResult.CallbackInvoked,
    "Continue actions must expose the wrapped callback result separately.");

  WorldGenerationTileScanAndControlActionsCommand.TileCountAccumulator accumulator =
    new([9]);
  WorldGenerationTileScanAndControlSystem.Result inactiveResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      WorldGenerationTileScanAndControlActionsCommand.TileScanner(accumulator),
      observation with { Active = false });
  Assert(!inactiveResult.TileMatched && accumulator.TotalMatches == 0,
    "TileScanner must ignore inactive observations.");

  WorldGenerationTileScanAndControlSystem.Result activeResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      WorldGenerationTileScanAndControlActionsCommand.TileScanner(accumulator),
      observation);
  Assert(activeResult.TileMatched && accumulator.TotalMatches == 1,
    "TileScanner must record one active requested tile.");

  RecordingBoundsReference bounds = new();
  WorldGenerationTileScanAndControlSystem.Result boundsResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      WorldGenerationTileScanAndControlActionsCommand.UpdateBounds(bounds),
      observation);
  Assert(boundsResult.Accepted && boundsResult.BoundsUpdated,
    "UpdateBounds must use the explicit writable bounds boundary.");
  Assert(bounds.LastPosition == observation.Position,
    "UpdateBounds must receive the visited tile position.");

  WorldGenerationTileScanAndControlSystem.Result exceptionResult =
    WorldGenerationTileScanAndControlSystem.Execute(
      WorldGenerationTileScanAndControlActionsCommand.Count(new ThrowingCountSink()),
      observation,
      new WorldGenerationTileScanAndControlSystem.ExecutionPolicy(
        WorldGenerationTileScanAndControlSystem.ExceptionPolicy.TreatAsFailureAndStop));
  Assert(!exceptionResult.Accepted && exceptionResult.StopRequested,
    "A sink exception must become a stopping failure under the explicit policy.");
  Assert(exceptionResult.FailureReason == "CallbackException:InvalidOperationException",
    "Sink exceptions must expose a stable failure category.");
}

static void FramingAndDebugSystemUsesExplicitPorts()
{
  TilePosition position = new(7, 8);
  RecordingFramingPort framingPort = new();
  WorldGenerationTileFramingAndDebugSystem.Result frameResult =
    WorldGenerationTileFramingAndDebugSystem.Execute(
      WorldGenerationTileFramingAndDebugActionsCommand.SetFrames(
        position,
        frameNeighbors: true),
      framingPort);
  Assert(frameResult.Accepted && frameResult.FramesApplied,
    "SetFrames must commit through the explicit framing port.");
  Assert(frameResult.AffectedRegion == framingPort.LastAffectedRegion,
    "SetFrames must report the exact affected tile region returned by its framing owner.");
  Assert(
    framingPort.LastPosition == position && framingPort.LastFrameNeighbors,
    "SetFrames must preserve target and neighbor framing intent.");

  RecordingDiagnosticSink diagnosticSink = new();
  ColorRgba color = new(1, 2, 3, 4);
  WorldGenerationTileFramingAndDebugSystem.Result debugResult =
    WorldGenerationTileFramingAndDebugSystem.Execute(
      WorldGenerationTileFramingAndDebugActionsCommand.DebugDraw(
        position,
        color,
        diagnosticSink),
      framingPort);
  Assert(debugResult.Accepted && debugResult.DiagnosticPublished,
    "DebugDraw must publish through the explicit diagnostic sink.");
  Assert(
    diagnosticSink.LastPosition == position && diagnosticSink.LastColor == color,
    "DebugDraw must preserve target and diagnostic color.");

  WorldGenerationTileFramingAndDebugSystem.Result missingPortResult =
    WorldGenerationTileFramingAndDebugSystem.Execute(
      WorldGenerationTileFramingAndDebugActionsCommand.SetFrames(position),
      framingPort: null,
      exceptionPolicy:
        WorldGenerationTileFramingAndDebugSystem.ExceptionPolicy.TreatAsFailureAndStop);
  Assert(
    !missingPortResult.Accepted && missingPortResult.StopRequested &&
    missingPortResult.FailureReason == "FramingPortMissing",
    "A missing framing owner must reject rather than write ambient tile state.");
}

ActionExecutionPreservesOrderAndFailurePolicy();
ActionCommitRouterSelectsOneOwnerPort();
ActionPreparationAndProjectionsStaySeparated();
ActionExecutionHonorsCancellationBarrier();
PaintValidationPreservesReferenceFailureRules();
WallMutationPreservesRemoveWallIntent();
StructureReservationIsAtomicAndGenerationScoped();
WorldGenRangePreservesInclusiveScaling();
ReservationSnapshotProjectionPreservesScope();
ShapeTraversalPreservesChainAndOutputOrder();
ConditionSearchPreservesAllAnyAndNotFound();
ShapeModifiersKeepExplicitCoordinatesAndRandomInputs();
TileScanQueryPreservesLegacyCountSemantics();
TileScanAndControlSystemPreservesReferenceOrder();
FramingAndDebugSystemUsesExplicitPorts();
Console.WriteLine("PASS: P20 core action and structure reservation checks");

sealed class RecordingActionCommitPort : IWorldGenerationActionCommitPort
{
  private readonly long _rejectedSequence;
  private readonly Action? _afterCommit;

  public RecordingActionCommitPort(
    long rejectedSequence,
    Action? afterCommit = null)
  {
    _rejectedSequence = rejectedSequence;
    _afterCommit = afterCommit;
  }

  public List<long> Sequences { get; } = [];

  public WorldGenerationActionCommitResult Commit(in WorldGenerationAction action)
  {
    Sequences.Add(action.Sequence);
    WorldGenerationActionCommitResult result = action.Sequence == _rejectedSequence
      ? new WorldGenerationActionCommitResult(false, "intentional verification rejection")
      : new WorldGenerationActionCommitResult(true);
    _afterCommit?.Invoke();
    return result;
  }
}

sealed class ThrowingCancellationCommitPort : IWorldGenerationActionCommitPort
{
  public WorldGenerationActionCommitResult Commit(in WorldGenerationAction action)
  {
    throw new OperationCanceledException();
  }
}

sealed class RoutingRecorder :
  WorldGenerationActionCommitRouter.ITileSetPort,
  WorldGenerationActionCommitRouter.IWallMutationPort,
  WorldGenerationActionCommitRouter.ITilePlacementAndPaintPort,
  WorldGenerationActionCommitRouter.ILiquidAndNeighborPort,
  WorldGenerationActionCommitRouter.ITileScanAndControlPort,
  WorldGenerationActionCommitRouter.ITileFramingAndDebugPort
{
  public List<string> Routes { get; } = [];

  public long LastGenerationId { get; private set; }

  public long LastSequence { get; private set; }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationTileSetActionsCommand command)
  {
    return Record("tile", generationId, sequence);
  }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationWallMutationActionsCommand command)
  {
    return Record("wall", generationId, sequence);
  }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationTilePlacementAndPaintActionsCommand command)
  {
    return Record("placement", generationId, sequence);
  }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationLiquidAndNeighborActionsCommand command)
  {
    return Record("liquid", generationId, sequence);
  }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationTileScanAndControlActionsCommand command)
  {
    return Record("scan", generationId, sequence);
  }

  public WorldGenerationActionCommitResult Commit(
    long generationId,
    long sequence,
    in WorldGenerationTileFramingAndDebugActionsCommand command)
  {
    return Record("framing", generationId, sequence);
  }

  private WorldGenerationActionCommitResult Record(
    string route,
    long generationId,
    long sequence)
  {
    Routes.Add(route);
    LastGenerationId = generationId;
    LastSequence = sequence;
    return new WorldGenerationActionCommitResult(true);
  }
}

sealed class RecordingCountSink :
  WorldGenerationTileScanAndControlActionsCommand.ICountResultSink
{
  public List<int> Values { get; } = [];

  public void Add(int value)
  {
    Values.Add(value);
  }
}

sealed class RecordingContinuation :
  WorldGenerationTileScanAndControlActionsCommand.IContinuation
{
  private readonly bool _result;

  public RecordingContinuation(bool result)
  {
    _result = result;
  }

  public bool Continue(TilePosition position)
  {
    LastPosition = position;
    return _result;
  }

  public TilePosition LastPosition { get; private set; }
}

sealed class RecordingBoundsReference :
  WorldGenerationTileScanAndControlSystem.IWritableBoundsReference
{
  public TilePosition LastPosition { get; private set; }

  public void Update(TilePosition position)
  {
    LastPosition = position;
  }
}

sealed class ThrowingCountSink :
  WorldGenerationTileScanAndControlActionsCommand.ICountResultSink
{
  public void Add(int value)
  {
    throw new InvalidOperationException("verification sink failure");
  }
}

sealed class RecordingFramingPort : WorldGenerationTileFramingAndDebugSystem.IFramingPort
{
  public WorldGenerationTileFramingAndDebugSystem.TileFrameRegion LastAffectedRegion { get; } =
    new(6, 7, 8, 9);

  public TilePosition LastPosition { get; private set; }

  public bool LastFrameNeighbors { get; private set; }

  public WorldGenerationTileFramingAndDebugSystem.TileFrameRegion Frame(
    TilePosition target,
    bool frameNeighbors)
  {
    LastPosition = target;
    LastFrameNeighbors = frameNeighbors;
    return LastAffectedRegion;
  }
}

sealed class RecordingDiagnosticSink :
  WorldGenerationTileFramingAndDebugActionsCommand.IDiagnosticSink
{
  public TilePosition LastPosition { get; private set; }

  public ColorRgba LastColor { get; private set; }

  public void Publish(TilePosition target, ColorRgba color)
  {
    LastPosition = target;
    LastColor = color;
  }
}

sealed class GridTileSnapshotReader : IStructureTileSnapshotReader
{
  private readonly Dictionary<TilePosition, StructureTileSnapshot> _tiles = [];

  public GridTileSnapshotReader(WorldGenerationRectangle worldBounds)
  {
    WorldBounds = worldBounds;
    for (int x = worldBounds.X; x < worldBounds.X + worldBounds.Width; x++)
    {
      for (int y = worldBounds.Y; y < worldBounds.Y + worldBounds.Height; y++)
      {
        _tiles[new TilePosition(x, y)] =
          new StructureTileSnapshot(false, false, 0);
      }
    }
  }

  public WorldGenerationRectangle WorldBounds { get; }

  public void Set(TilePosition position, StructureTileSnapshot snapshot)
  {
    _tiles[position] = snapshot;
  }

  public bool TryRead(
    TilePosition position,
    out StructureTileSnapshot snapshot)
  {
    return _tiles.TryGetValue(position, out snapshot);
  }
}

sealed class RecordingRandomSource : IGenerationRandomSource
{
  private readonly int _value;

  public RecordingRandomSource(int value)
  {
    _value = value;
  }

  public List<(GenerationRandomStream Stream, int Minimum, int Maximum)> Requests { get; } = [];

  public int NextInt(
    GenerationRandomStream stream,
    int minimumInclusive,
    int maximumExclusive)
  {
    Requests.Add((stream, minimumInclusive, maximumExclusive));
    if (_value < minimumInclusive || _value >= maximumExclusive)
    {
      throw new InvalidOperationException("The verification random value is outside the request.");
    }

    return _value;
  }
}
