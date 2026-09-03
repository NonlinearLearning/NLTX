using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldModel;

if (args.Any(argument =>
      StringComparer.Ordinal.Equals(argument, "--wall-framing-only")))
{
  RunPyramidWallFramingFocusedVerification();
  Environment.Exit(0);
}

if (args.Any(argument =>
      StringComparer.Ordinal.Equals(argument, "--wall-frame-evaluation-only")))
{
  RunPyramidWallFrameEvaluationFocusedVerification();
  Environment.Exit(0);
}

if (args.Any(argument =>
      StringComparer.Ordinal.Equals(argument, "--wall-neighbor-only")))
{
  RunPyramidWallFrameNeighborFocusedVerification();
  Environment.Exit(0);
}

if (args.Any(argument =>
      StringComparer.Ordinal.Equals(argument, "--tunnel-opening-only")))
{
  RunPyramidTunnelOpeningFocusedVerification();
  Environment.Exit(0);
}

if (args.Any(argument =>
      StringComparer.Ordinal.Equals(argument, "--buried-chest-only")))
{
  RunPyramidBuriedChestFocusedVerification();
  Environment.Exit(0);
}

if (!LegacyPyramidStructureRequest.TryCreateDefault(
      originX: 120,
      originY: 240,
      noTunnel: false,
      out LegacyPyramidStructureRequest defaultRequest))
{
  throw new InvalidOperationException("The default Pyramid request was rejected.");
}

if (defaultRequest.OriginX != 120 ||
    defaultRequest.OriginY != 240 ||
    defaultRequest.PyramidMinDepth != 75 ||
    defaultRequest.PyramidMaxDepth != 125 ||
    defaultRequest.NoTunnel ||
    defaultRequest.TileType != 151 ||
    defaultRequest.WallType != 34)
{
  throw new InvalidOperationException(
    "The default Pyramid request drifted from the source contract.");
}

if (!LegacyPyramidStructureRequest.TryCreate(
      originX: 120,
      originY: 190,
      pyramidMinDepth: 75,
      pyramidMaxDepth: 100,
      noTunnel: true,
      out LegacyPyramidStructureRequest dualDungeonRequest) ||
    !dualDungeonRequest.NoTunnel ||
    dualDungeonRequest.PyramidMaxDepth != 100)
{
  throw new InvalidOperationException("The dual-dungeon Pyramid request was not preserved.");
}

if (LegacyPyramidStructureRequest.TryCreate(
      originX: -1,
      originY: 190,
      pyramidMinDepth: 75,
      pyramidMaxDepth: 125,
      noTunnel: false,
      out _) ||
    LegacyPyramidStructureRequest.TryCreate(
      originX: 120,
      originY: -1,
      pyramidMinDepth: 75,
      pyramidMaxDepth: 125,
      noTunnel: false,
      out _) ||
    LegacyPyramidStructureRequest.TryCreate(
      originX: 120,
      originY: 190,
      pyramidMinDepth: 0,
      pyramidMaxDepth: 125,
      noTunnel: false,
      out _) ||
    LegacyPyramidStructureRequest.TryCreate(
      originX: 120,
      originY: 190,
      pyramidMinDepth: 126,
      pyramidMaxDepth: 125,
      noTunnel: false,
      out _))
{
  throw new InvalidOperationException("Invalid Pyramid request inputs were accepted.");
}

LegacyPyramidStructureRequest first = defaultRequest;
if (!first.Equals(defaultRequest) || first.GetHashCode() != defaultRequest.GetHashCode())
{
  throw new InvalidOperationException("Pyramid request value semantics are not deterministic.");
}

WorldMetadata metadata = new("pyramid-footprint", new WorldSeed(1456), 200, 300);
WorldGrid sourceWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
LegacyPyramidStructureRequest footprintRequest =
  LegacyPyramidStructureRequest.TryCreate(
    originX: 100,
    originY: 100,
    pyramidMinDepth: 10,
    pyramidMaxDepth: 14,
    noTunnel: false,
    out LegacyPyramidStructureRequest createdRequest)
    ? createdRequest
    : throw new InvalidOperationException("The footprint Pyramid request was rejected.");
LegacyPassRandomState expectedRandom = new(1456);
int expectedTopOffset = expectedRandom.Next(0, 7);
int expectedTunnelWidth = expectedRandom.Next(9, 13);
int expectedDepth = expectedRandom.Next(
  footprintRequest.PyramidMinDepth,
  footprintRequest.PyramidMaxDepth);
int expectedTopY = footprintRequest.OriginY - expectedTopOffset;
int expectedBottomYExclusive = footprintRequest.OriginY + expectedDepth;
int expectedRowCount = expectedBottomYExclusive - expectedTopY;
HashSet<(int X, int Y)> expectedPillarCoordinates = new();
List<(int X, int Y)> expectedPillarSequence = new();
for (int row = 0; row < expectedRowCount; row++)
{
  int halfWidth = row + 1;
  for (int x = footprintRequest.OriginX - halfWidth;
    x < footprintRequest.OriginX + halfWidth - 1;
       x++)
  {
    _ = expectedPillarCoordinates.Add((x, expectedTopY + row));
    expectedPillarSequence.Add((x, expectedTopY + row));
  }
}

WorldTile preservedPillar = new(
  IsActive: false,
  Type: 7,
  LiquidAmount: 123,
  LiquidType: 2,
  FrameX: 17,
  FrameY: -9,
  WallType: 8,
  HasWire: true,
  IsHalfBrick: true,
  Slope: 4,
  IsInactive: true,
  TileColor: 6,
  WallColor: 7,
  IsInvisibleBlock: true,
  IsInvisibleWall: true,
  IsFullbrightBlock: true,
  IsFullbrightWall: true);
if (!sourceWorld.TrySetTile(
      footprintRequest.OriginX - 1,
      expectedTopY,
      preservedPillar))
{
  throw new InvalidOperationException("The Pyramid preservation fixture could not be created.");
}

WorldGridSnapshot sourceSnapshot = sourceWorld.CreateSnapshot(metadata);

List<(int X, int Y)> expectedWallCoordinates = new();
int finalHalfWidth = expectedRowCount + 1;
for (int x = footprintRequest.OriginX - finalHalfWidth - 5;
     x <= footprintRequest.OriginX + finalHalfWidth + 5;
     x++)
{
  for (int y = footprintRequest.OriginY - 1;
       y <= expectedBottomYExclusive + 1;
       y++)
  {
    bool hasSolidPyramidNeighborhood = true;
    for (int neighborX = x - 1; neighborX <= x + 1; neighborX++)
    {
      for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
      {
        if (!expectedPillarCoordinates.Contains((neighborX, neighborY)))
        {
          hasSolidPyramidNeighborhood = false;
        }
      }
    }

    if (hasSolidPyramidNeighborhood)
    {
      expectedWallCoordinates.Add((x, y));
    }
  }
}

LegacyPassRandomState footprintRandom = new(1456);
WorldGenerationStateComponent footprintState = new(1);
List<TileChangeCommand> footprintCommands = new();
if (!LegacyPyramidFootprintMutation.TryAppendCommands(
      sourceSnapshot,
      footprintRequest,
      footprintRandom,
      ref footprintState,
      footprintCommands,
      out LegacyPyramidFootprintMutationResult footprintResult,
      out string? footprintFailure))
{
  throw new InvalidOperationException(
    $"The Pyramid footprint mutation was rejected: {footprintFailure}");
}

if (footprintFailure is not null ||
    footprintRandom.SampleCount != 3 ||
    footprintResult.TopY != expectedTopY ||
    footprintResult.BottomYExclusive != expectedBottomYExclusive ||
    footprintResult.TunnelWidth != expectedTunnelWidth ||
    footprintResult.PillarTileCount != expectedPillarCoordinates.Count ||
    footprintResult.WallTileCount != expectedWallCoordinates.Count ||
    footprintState.Stage != WorldGenerationStage.Structure ||
    footprintState.NextSequence != footprintCommands.Count ||
    footprintCommands.Count != expectedPillarCoordinates.Count * 2 + expectedWallCoordinates.Count)
{
  throw new InvalidOperationException("The Pyramid footprint source loop contract drifted.");
}

List<(int X, int Y)> actualWallCoordinates = new();
List<(int X, int Y)> actualPillarSequence = new();
HashSet<(int X, int Y)> actualPillarCoordinates = new();
int actualPillarShapeCount = 0;
foreach (TileChangeCommand command in footprintCommands)
{
  if (command.Source != LegacyPyramidFootprintMutation.Source)
  {
    throw new InvalidOperationException("Pyramid footprint commands lost source attribution.");
  }

  if (command.Kind == TileChangeKind.SetWall)
  {
    if (command.WallType != LegacyPyramidStructureRequest.PyramidWallType)
    {
      throw new InvalidOperationException("Pyramid wall command used the wrong wall type.");
    }

    actualWallCoordinates.Add((command.X, command.Y));
  }
  else if (command.Kind == TileChangeKind.UpdateTileType)
  {
    if (command.TileType != LegacyPyramidStructureRequest.PyramidTileType ||
        command.IsActive != true)
    {
      throw new InvalidOperationException("Pyramid pillar type command drifted.");
    }

    _ = actualPillarCoordinates.Add((command.X, command.Y));
    actualPillarSequence.Add((command.X, command.Y));
  }
  else if (command.Kind != TileChangeKind.UpdateTileShape ||
           command.IsHalfBrick != false || command.Slope != 0)
  {
    throw new InvalidOperationException("Pyramid pillar shape command drifted.");
  }
  else
  {
    actualPillarShapeCount++;
  }
}

if (actualPillarCoordinates.Count != expectedPillarCoordinates.Count ||
    actualPillarSequence.SequenceEqual(expectedPillarSequence) == false ||
    actualPillarShapeCount != expectedPillarSequence.Count ||
    !actualWallCoordinates.SequenceEqual(expectedWallCoordinates))
{
  throw new InvalidOperationException("Pyramid footprint coordinates drifted from source order.");
}

WorldGrid committedWorld = WorldGrid.FromSnapshot(sourceSnapshot);
if (!new TileChangeCommitSystem().TryCommit(
      committedWorld,
      footprintCommands,
      out TileChangeCommitResult footprintCommit) ||
    footprintCommit.AppliedCount != footprintCommands.Count)
{
  throw new InvalidOperationException("Pyramid footprint commands could not be committed.");
}

WorldTile committedPillar = committedWorld.GetTile(
  footprintRequest.OriginX - 1,
  expectedTopY);
if (!committedPillar.IsActive ||
    committedPillar.Type != LegacyPyramidStructureRequest.PyramidTileType ||
    committedPillar.IsHalfBrick ||
    committedPillar.Slope != 0 ||
    committedPillar.LiquidAmount != preservedPillar.LiquidAmount ||
    committedPillar.LiquidType != preservedPillar.LiquidType ||
    committedPillar.FrameX != preservedPillar.FrameX ||
    committedPillar.FrameY != preservedPillar.FrameY ||
    committedPillar.WallType != preservedPillar.WallType ||
    !committedPillar.HasWire ||
    !committedPillar.IsInactive ||
    committedPillar.TileColor != preservedPillar.TileColor ||
    committedPillar.WallColor != preservedPillar.WallColor ||
    !committedPillar.IsInvisibleBlock ||
    !committedPillar.IsInvisibleWall ||
    !committedPillar.IsFullbrightBlock ||
    !committedPillar.IsFullbrightWall)
{
  throw new InvalidOperationException("Pyramid pillar tile state was not normalized.");
}

WorldGrid invalidWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
WorldGridSnapshot invalidSnapshot = invalidWorld.CreateSnapshot(metadata);
LegacyPassRandomState invalidRandom = new(1456);
WorldGenerationStateComponent invalidState = new(2);
List<TileChangeCommand> invalidCommands = new();
if (!LegacyPyramidStructureRequest.TryCreate(
      0,
      100,
      10,
      14,
      false,
      out LegacyPyramidStructureRequest invalidBoundsRequest))
{
  throw new InvalidOperationException("The invalid-bounds Pyramid request was not created.");
}
if (LegacyPyramidFootprintMutation.TryAppendCommands(
      invalidSnapshot,
      invalidBoundsRequest,
      invalidRandom,
      ref invalidState,
      invalidCommands,
      out _,
      out _) ||
    invalidCommands.Count != 0 ||
    invalidRandom.SampleCount != 0 ||
    invalidState.NextSequence != 0)
{
  throw new InvalidOperationException("Invalid Pyramid footprint bounds were not fail-closed.");
}

if (!LegacyPyramidStructureRequest.TryCreate(
      25,
      100,
      10,
      14,
      false,
      out LegacyPyramidStructureRequest horizontalEnvelopeRequest))
{
  throw new InvalidOperationException("The horizontal-envelope Pyramid request was not created.");
}

LegacyPassRandomState horizontalEnvelopeRandom = new(1456);
WorldGenerationStateComponent horizontalEnvelopeState = new(4);
List<TileChangeCommand> horizontalEnvelopeCommands = new();
if (LegacyPyramidFootprintMutation.TryAppendCommands(
      sourceSnapshot,
      horizontalEnvelopeRequest,
      horizontalEnvelopeRandom,
      ref horizontalEnvelopeState,
      horizontalEnvelopeCommands,
      out _,
      out _) ||
    horizontalEnvelopeCommands.Count != 0 ||
    horizontalEnvelopeRandom.SampleCount != 0 ||
    horizontalEnvelopeState.NextSequence != 0)
{
  throw new InvalidOperationException("The Pyramid horizontal envelope was not fail-closed.");
}

LegacyPyramidStructureRequest malformedRequest = default;
LegacyPassRandomState malformedRandom = new(1456);
WorldGenerationStateComponent malformedState = new(5);
List<TileChangeCommand> malformedCommands = new();
if (LegacyPyramidFootprintMutation.TryAppendCommands(
      sourceSnapshot,
      malformedRequest,
      malformedRandom,
      ref malformedState,
      malformedCommands,
      out _,
      out _) ||
    malformedCommands.Count != 0 ||
    malformedRandom.SampleCount != 0 ||
    malformedState.NextSequence != 0)
{
  throw new InvalidOperationException("Malformed Pyramid depths were not fail-closed.");
}

WorldGenerationStateComponent exhaustedState = new(3, long.MaxValue - 1);
LegacyPassRandomState exhaustedRandom = new(1456);
List<TileChangeCommand> exhaustedCommands = new();
if (LegacyPyramidFootprintMutation.TryAppendCommands(
      sourceSnapshot,
      footprintRequest,
      exhaustedRandom,
      ref exhaustedState,
      exhaustedCommands,
      out _,
      out _) ||
    exhaustedCommands.Count != 0 ||
    exhaustedRandom.SampleCount != 0 ||
    exhaustedState.NextSequence != long.MaxValue - 1)
{
  throw new InvalidOperationException("Exhausted Pyramid sequence space was not fail-closed.");
}

Console.WriteLine(
  "PASS: Pyramid request and source-backed pillar/wall mutation preserve typed contracts");
Console.WriteLine(
  "SUMMARY: Pyramid framing, tunnel/features, global RNG/checkpoint parity, publication, and " +
  "aggregate WLD parity remain deferred");

static void RunPyramidWallFramingFocusedVerification()
{
  const int width = 200;
  const int height = 300;
  const int seed = 1456;
  const int centerX = 100;
  const int centerY = 100;
  WorldMetadata metadata = new(
    "pyramid-wall-framing-focused",
    new WorldSeed(seed),
    width,
    height);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  WorldTile beforeCenter = snapshot.GetTile(centerX, centerY);
  IReadOnlyList<WallFrameCoordinate> centers = new[]
  {
    new WallFrameCoordinate(centerX, centerY),
    new WallFrameCoordinate(centerX + 1, centerY)
  };
  LegacyPassRandomState random = new(seed);
  WorldGenerationStateComponent state = new(seed);
  if (!LegacyPyramidWallFrameRequestQuery.TryCreateRequests(
        snapshot,
        centers,
        out IReadOnlyList<LegacyPyramidWallFrameRequest> requests,
        out string? failureReason))
  {
    throw new InvalidOperationException(
      $"Pyramid wall-framing request expansion was rejected: {failureReason}");
  }

  int expectedRequestCount = checked(
    centers.Count * LegacyPyramidWallFrameRequestQuery.RequestsPerCenter);
  if (requests.Count != expectedRequestCount || failureReason is not null)
  {
    throw new InvalidOperationException(
      "Pyramid wall-framing request count or failure state drifted.");
  }

  int requestIndex = 0;
  for (int centerIndex = 0; centerIndex < centers.Count; centerIndex++)
  {
    WallFrameCoordinate center = centers[centerIndex];
    for (int offsetX = -1; offsetX <= 1; offsetX++)
    {
      for (int offsetY = -1; offsetY <= 1; offsetY++)
      {
        LegacyPyramidWallFrameRequest request = requests[requestIndex++];
        WallFrameCoordinate expectedTarget = new(
          center.X + offsetX,
          center.Y + offsetY);
        if (request.Center != center ||
            request.Target != expectedTarget ||
            !request.ResetFrame ||
            request.Source != "worldgen.Pyramid.wall-frame" ||
            request.SourceLine != 28425)
        {
          throw new InvalidOperationException(
            "Pyramid wall-framing requests did not preserve source x-major order or metadata.");
        }
      }
    }
  }

  if (random.SampleCount != 0 ||
      state.Stage != WorldGenerationStage.Created ||
      state.NextSequence != 0 ||
      snapshot.GetTile(centerX, centerY) != beforeCenter)
  {
    throw new InvalidOperationException(
      "Pyramid wall-framing request expansion consumed state or mutated its snapshot input.");
  }

  IReadOnlyList<WallFrameCoordinate> invalidCenters = new[]
  {
    new WallFrameCoordinate(0, 0)
  };
  if (LegacyPyramidWallFrameRequestQuery.TryCreateRequests(
        snapshot,
        invalidCenters,
        out IReadOnlyList<LegacyPyramidWallFrameRequest> invalidRequests,
        out string? invalidFailure) ||
      invalidRequests.Count != 0 ||
      string.IsNullOrWhiteSpace(invalidFailure))
  {
    throw new InvalidOperationException(
      "Out-of-bounds Pyramid wall-framing requests were not rejected atomically.");
  }

  IReadOnlyList<WallFrameCoordinate> edgeAdjacentCenters = new[]
  {
    new WallFrameCoordinate(1, centerY),
    new WallFrameCoordinate(centerX, 1)
  };
  if (LegacyPyramidWallFrameRequestQuery.TryCreateRequests(
        snapshot,
        edgeAdjacentCenters,
        out IReadOnlyList<LegacyPyramidWallFrameRequest> edgeAdjacentRequests,
        out string? edgeAdjacentFailure) ||
      edgeAdjacentRequests.Count != 0 ||
      string.IsNullOrWhiteSpace(edgeAdjacentFailure))
  {
    throw new InvalidOperationException(
      "A Pyramid wall-framing envelope adjacent to the border was not rejected atomically.");
  }

  if (!LegacyPyramidWallFrameRequestQuery.TryCreateRequests(
        snapshot,
        Array.Empty<WallFrameCoordinate>(),
        out IReadOnlyList<LegacyPyramidWallFrameRequest> emptyRequests,
        out string? emptyFailure) ||
      emptyRequests.Count != 0 ||
      emptyFailure is not null)
  {
    throw new InvalidOperationException(
      "An empty Pyramid wall-framing center list was not a deterministic no-op.");
  }

  Console.WriteLine(
    $"PASS: Pyramid wall-framing request expansion preserves {centers.Count} centers and " +
    $"{requests.Count} x-major requests");
  Console.WriteLine(
    "SUMMARY: Framing.WallFrame value semantics, frame mutation, tunnel/features, exact global " +
    "RNG parity, publication, and aggregate WLD parity remain deferred");
}

static void RunPyramidWallFrameEvaluationFocusedVerification()
{
  const int width = 200;
  const int height = 300;
  const int seed = 1456;
  const int centerX = 100;
  const int centerY = 100;
  WorldMetadata metadata = new(
    "pyramid-wall-frame-evaluation-focused",
    new WorldSeed(seed),
    width,
    height);

  WorldGrid ordinaryWorld = new(width, height, initializeLegacyEmptyFrames: true);
  _ = ordinaryWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(
      IsActive: false,
      Type: 0,
      FrameX: 17,
      FrameY: -9,
      WallType: 34,
      WallColor: 7,
      IsInvisibleWall: true,
      IsFullbrightWall: true,
      WallFrameX: 41,
      WallFrameY: 43,
      WallFrameNumber: 3));
  WorldGridSnapshot ordinarySnapshot = ordinaryWorld.CreateSnapshot(metadata);
  LegacyPassRandomState ordinaryRandom = new(seed);
  LegacyPassRandomState expectedOrdinaryRandom = new(seed);
  int expectedOrdinaryFrameNumber = expectedOrdinaryRandom.Next(0, 3);
  LegacyWallFrameEvaluationResult ordinary = LegacyWallFrameEvaluationQuery.Evaluate(
    ordinarySnapshot,
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    ordinaryRandom);
  if (!ordinary.IsApplicable || ordinary.NeighborMask != 0 || ordinary.RandomDrawCount != 1 ||
      ordinaryRandom.SampleCount != 1 || ordinary.Projected.WallType != 34 ||
      ordinary.Projected.WallFrameNumber != expectedOrdinaryFrameNumber ||
      ordinary.Projected.FrameX != 17 || ordinary.Projected.FrameY != -9 ||
      ordinary.Projected.WallFrameX != (ordinary.Projected.WallFrameNumber == 0 ? 324 :
        ordinary.Projected.WallFrameNumber == 1 ? 360 :
        ordinary.Projected.WallFrameNumber == 2 ? 396 : 216) ||
      ordinary.Projected.WallFrameY != (ordinary.Projected.WallFrameNumber == 3 ? 216 : 108) ||
      !ordinary.Projected.IsInvisibleWall || !ordinary.Projected.IsFullbrightWall ||
      ordinary.Projected.WallColor != 7)
  {
    throw new InvalidOperationException(
      "Ordinary Pyramid wall-frame evaluation drifted from the source lookup and reset branch.");
  }

  WorldGrid maskedFrameNumberWorld = WorldGrid.FromSnapshot(ordinarySnapshot);
  _ = maskedFrameNumberWorld.TrySetTile(
    centerX,
    centerY,
    ordinarySnapshot.GetTile(centerX, centerY) with { WallFrameNumber = byte.MaxValue });
  LegacyWallFrameEvaluationResult maskedFrameNumber = LegacyWallFrameEvaluationQuery.Evaluate(
    maskedFrameNumberWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: false,
    showInvisibleWalls: false,
    new LegacyPassRandomState(seed));
  if (maskedFrameNumber.Projected.WallFrameNumber != 3 ||
      maskedFrameNumber.Projected.FrameX != 17 ||
      maskedFrameNumber.Projected.FrameY != -9 ||
      maskedFrameNumber.Projected.WallFrameX != 216 ||
      maskedFrameNumber.Projected.WallFrameY != 216)
  {
    throw new InvalidOperationException(
      "Non-reset wall-frame numbers did not preserve the legacy two-bit normalization.");
  }

  WorldGrid fullMaskWorld = WorldGrid.FromSnapshot(ordinarySnapshot);
  _ = fullMaskWorld.TrySetTile(
    centerX,
    centerY - 1,
    new WorldTile(IsActive: false, Type: 0, WallType: 34));
  _ = fullMaskWorld.TrySetTile(
    centerX - 1,
    centerY,
    new WorldTile(IsActive: false, Type: 0, WallType: 34));
  _ = fullMaskWorld.TrySetTile(
    centerX + 1,
    centerY,
    new WorldTile(IsActive: false, Type: 0, WallType: 34));
  _ = fullMaskWorld.TrySetTile(
    centerX,
    centerY + 1,
    new WorldTile(IsActive: false, Type: 0, WallType: 34));
  LegacyWallFrameEvaluationResult fullMask = LegacyWallFrameEvaluationQuery.Evaluate(
    fullMaskWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: false,
    showInvisibleWalls: false,
    new LegacyPassRandomState(seed));
  if (fullMask.NeighborMask != 15 || fullMask.FrameLookupIndex != 16 ||
      fullMask.Projected.FrameX != 17 || fullMask.Projected.FrameY != -9 ||
      fullMask.Projected.WallFrameX != 252 || fullMask.Projected.WallFrameY != 180 ||
      fullMask.RandomDrawCount != 0)
  {
    throw new InvalidOperationException(
      "Pyramid full wall-neighbor mask or center offset drifted.");
  }

  WorldGrid truncatingWorld = WorldGrid.FromSnapshot(ordinarySnapshot);
  _ = truncatingWorld.TrySetTile(
    centerX,
    centerY - 1,
    new WorldTile(IsActive: true, Type: 54, IsInvisibleWall: true));
  LegacyWallFrameEvaluationResult hiddenTruncating = LegacyWallFrameEvaluationQuery.Evaluate(
    truncatingWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: false,
    showInvisibleWalls: false,
    new LegacyPassRandomState(seed));
  LegacyWallFrameEvaluationResult visibleTruncating = LegacyWallFrameEvaluationQuery.Evaluate(
    truncatingWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: false,
    showInvisibleWalls: true,
    new LegacyPassRandomState(seed));
  if (hiddenTruncating.NeighborMask != 0 || visibleTruncating.NeighborMask != 1)
  {
    throw new InvalidOperationException(
      "Pyramid wall-frame invisible/truncating neighbor semantics drifted.");
  }

  WorldGrid largeWallWorld = new(width, height, initializeLegacyEmptyFrames: true);
  _ = largeWallWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(IsActive: false, Type: 0, WallType: 179));
  LegacyPassRandomState largeWallRandom = new(seed);
  LegacyWallFrameEvaluationResult phlebas = LegacyWallFrameEvaluationQuery.Evaluate(
    largeWallWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    largeWallRandom);
  if (phlebas.Projected.WallFrameNumber != 3 || phlebas.Projected.WallFrameX != 216 ||
      phlebas.Projected.WallFrameY != 216 || phlebas.RandomDrawCount != 0 ||
      largeWallRandom.SampleCount != 0)
  {
    throw new InvalidOperationException("Phlebas wall-frame lookup semantics drifted.");
  }

  _ = largeWallWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(IsActive: false, Type: 0, WallType: 185));
  LegacyWallFrameEvaluationResult lazure = LegacyWallFrameEvaluationQuery.Evaluate(
    largeWallWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    new LegacyPassRandomState(seed));
  if (lazure.Projected.WallFrameNumber != 0 || lazure.Projected.WallFrameX != 324 ||
      lazure.Projected.WallFrameY != 108 || lazure.RandomDrawCount != 0)
  {
    throw new InvalidOperationException("Lazure wall-frame lookup semantics drifted.");
  }

  _ = largeWallWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(IsActive: false, Type: 0, WallType: 21));
  LegacyPassRandomState wall21Random = new(seed);
  LegacyPassRandomState expectedWall21Random = new(seed);
  int expectedWall21FrameNumber = expectedWall21Random.Next(0, 3);
  if (expectedWall21Random.Next(2) == 0)
  {
    expectedWall21FrameNumber = 2;
  }
  LegacyWallFrameEvaluationResult wall21 = LegacyWallFrameEvaluationQuery.Evaluate(
    largeWallWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    wall21Random);
  if (wall21.Projected.WallFrameNumber != expectedWall21FrameNumber ||
      wall21.RandomDrawCount != 2 || wall21Random.SampleCount != 2)
  {
    throw new InvalidOperationException("Wall-21 reset-frame random semantics drifted.");
  }

  WorldGrid clearedWorld = new(width, height, initializeLegacyEmptyFrames: true);
  _ = clearedWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(
      IsActive: false,
      Type: 0,
      FrameX: 321,
      FrameY: 432,
      WallType: 0,
      WallColor: 9,
      IsInvisibleWall: true,
      IsFullbrightWall: true,
      WallFrameX: 72,
      WallFrameY: 108,
      WallFrameNumber: 2));
  LegacyPassRandomState clearRandom = new(seed);
  LegacyWallFrameEvaluationResult cleared = LegacyWallFrameEvaluationQuery.Evaluate(
    clearedWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    clearRandom);
  if (cleared.Projected.WallType != 0 || cleared.Projected.WallColor != 0 ||
      cleared.Projected.IsInvisibleWall || cleared.Projected.IsFullbrightWall ||
      cleared.Projected.FrameX != 321 || cleared.Projected.FrameY != 432 ||
      cleared.Projected.WallFrameX != 72 || cleared.Projected.WallFrameY != 108 ||
      cleared.Projected.WallFrameNumber != 2 || cleared.RandomDrawCount != 0 ||
      clearRandom.SampleCount != 0)
  {
    throw new InvalidOperationException("Zero-wall paint/coating clearing semantics drifted.");
  }

  _ = clearedWorld.TrySetTile(
    centerX,
    centerY,
    new WorldTile(
      IsActive: false,
      Type: 0,
      FrameX: 321,
      FrameY: 432,
      WallType: LegacyLargeFrameWallRegistry.WallTypeCount,
      WallColor: 10,
      IsInvisibleWall: true,
      IsFullbrightWall: true,
      WallFrameX: 72,
      WallFrameY: 108,
      WallFrameNumber: 1));
  LegacyPassRandomState invalidWallRandom = new(seed);
  LegacyWallFrameEvaluationResult invalidWall = LegacyWallFrameEvaluationQuery.Evaluate(
    clearedWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(centerX, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    invalidWallRandom);
  if (invalidWall.Projected.WallType != 0 || invalidWall.Projected.WallColor != 0 ||
      invalidWall.Projected.IsInvisibleWall || invalidWall.Projected.IsFullbrightWall ||
      invalidWall.Projected.FrameX != 321 || invalidWall.Projected.FrameY != 432 ||
      invalidWall.Projected.WallFrameX != 72 || invalidWall.Projected.WallFrameY != 108 ||
      invalidWall.Projected.WallFrameNumber != 1 ||
      invalidWall.RandomDrawCount != 0 || invalidWallRandom.SampleCount != 0)
  {
    throw new InvalidOperationException("Invalid-wall normalization semantics drifted.");
  }

  LegacyPassRandomState edgeRandom = new(seed);
  LegacyWallFrameEvaluationResult edge = LegacyWallFrameEvaluationQuery.Evaluate(
    clearedWorld.CreateSnapshot(metadata),
    new WallFrameCoordinate(0, centerY),
    resetFrame: true,
    showInvisibleWalls: false,
    edgeRandom);
  if (edge.IsApplicable || edge.RandomDrawCount != 0 || edgeRandom.SampleCount != 0)
  {
    throw new InvalidOperationException("Pyramid wall-frame edge guards were not no-ops.");
  }

  WorldGrid commandWorld = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = centerX - 1; x <= centerX + 2; x++)
  {
    for (int y = centerY - 1; y <= centerY + 1; y++)
    {
      _ = commandWorld.TrySetTile(
        x,
        y,
        new WorldTile(
          IsActive: false,
          Type: 0,
          FrameX: 17,
          FrameY: -9,
          WallType: 34,
          WallFrameX: 41,
          WallFrameY: 43));
    }
  }
  WorldGridSnapshot commandSnapshot = commandWorld.CreateSnapshot(metadata);
  IReadOnlyList<WallFrameCoordinate> commandCenters = new[]
  {
    new WallFrameCoordinate(centerX, centerY),
    new WallFrameCoordinate(centerX + 1, centerY)
  };
  if (!LegacyPyramidWallFrameRequestQuery.TryCreateRequests(
        commandSnapshot,
        commandCenters,
        out IReadOnlyList<LegacyPyramidWallFrameRequest> requests,
        out string? requestFailure))
  {
    throw new InvalidOperationException(
      $"The Pyramid wall-frame requests were rejected: {requestFailure}");
  }

  LegacyPassRandomState emptyProjectionRandom = new(seed);
  WorldGenerationStateComponent emptyProjectionState = new(seed);
  List<LegacyWallFrameCommand> emptyProjectionCommands = new();
  if (!LegacyPyramidWallFrameCommandProjection.TryAppendCommands(
        commandSnapshot,
        Array.Empty<LegacyPyramidWallFrameRequest>(),
        showInvisibleWalls: false,
        emptyProjectionRandom,
        ref emptyProjectionState,
        emptyProjectionCommands,
        out string? emptyProjectionFailure) ||
      emptyProjectionFailure is not null ||
      emptyProjectionCommands.Count != 0 ||
      emptyProjectionState.Stage != WorldGenerationStage.Created ||
      emptyProjectionState.NextSequence != 0 ||
      emptyProjectionRandom.SampleCount != 0)
  {
    throw new InvalidOperationException(
      "An empty Pyramid wall-frame command batch was not a deterministic no-op.");
  }

  LegacyPyramidWallFrameRequest malformedCenterRequest = requests[0] with
  {
    Center = new WallFrameCoordinate(centerX + 20, centerY + 20)
  };
  LegacyPassRandomState malformedCenterRandom = new(seed);
  WorldGenerationStateComponent malformedCenterState = new(seed);
  List<LegacyWallFrameCommand> malformedCenterCommands = new();
  if (LegacyPyramidWallFrameCommandProjection.TryAppendCommands(
        commandSnapshot,
        new[] { malformedCenterRequest },
        showInvisibleWalls: false,
        malformedCenterRandom,
        ref malformedCenterState,
        malformedCenterCommands,
        out string? malformedCenterFailure) ||
      malformedCenterCommands.Count != 0 ||
      malformedCenterState.Stage != WorldGenerationStage.Created ||
      malformedCenterState.NextSequence != 0 ||
      malformedCenterRandom.SampleCount != 0 ||
      string.IsNullOrWhiteSpace(malformedCenterFailure))
  {
    throw new InvalidOperationException(
      "A Pyramid wall-frame request with a mismatched center was not rejected atomically.");
  }

  LegacyPyramidWallFrameRequest edgeTargetRequest = requests[0] with
  {
    Center = new WallFrameCoordinate(1, centerY),
    Target = new WallFrameCoordinate(0, centerY)
  };
  LegacyPassRandomState edgeTargetRandom = new(seed);
  WorldGenerationStateComponent edgeTargetState = new(seed);
  List<LegacyWallFrameCommand> edgeTargetCommands = new();
  if (LegacyPyramidWallFrameCommandProjection.TryAppendCommands(
        commandSnapshot,
        new[] { edgeTargetRequest },
        showInvisibleWalls: false,
        edgeTargetRandom,
        ref edgeTargetState,
        edgeTargetCommands,
        out string? edgeTargetFailure) ||
      edgeTargetCommands.Count != 0 ||
      edgeTargetState.Stage != WorldGenerationStage.Created ||
      edgeTargetState.NextSequence != 0 ||
      edgeTargetRandom.SampleCount != 0 ||
      string.IsNullOrWhiteSpace(edgeTargetFailure))
  {
    throw new InvalidOperationException(
      "A Pyramid wall-frame edge target was not rejected atomically.");
  }

  WorldGenerationStateComponent committedState = new(seed);
  if (!committedState.TryAdvance(WorldGenerationStage.Committed))
  {
    throw new InvalidOperationException("The committed-state fixture could not be created.");
  }

  LegacyPassRandomState committedRandom = new(seed);
  List<LegacyWallFrameCommand> committedCommands = new();
  if (LegacyPyramidWallFrameCommandProjection.TryAppendCommands(
        commandSnapshot,
        requests,
        showInvisibleWalls: false,
        committedRandom,
        ref committedState,
        committedCommands,
        out string? committedFailure) ||
      committedCommands.Count != 0 ||
      committedState.Stage != WorldGenerationStage.Committed ||
      committedState.NextSequence != 0 ||
      committedRandom.SampleCount != 0 ||
      string.IsNullOrWhiteSpace(committedFailure))
  {
    throw new InvalidOperationException(
      "A committed world-generation state accepted Pyramid wall-frame commands.");
  }
  LegacyPassRandomState commandRandom = new(seed);
  WorldGenerationStateComponent commandState = new(seed);
  List<LegacyWallFrameCommand> commands = new();
  if (!LegacyPyramidWallFrameCommandProjection.TryAppendCommands(
        commandSnapshot,
        requests,
        showInvisibleWalls: false,
        commandRandom,
        ref commandState,
        commands,
        out string? commandFailure) ||
      commandFailure is not null || commands.Count != requests.Count ||
      commandRandom.SampleCount != requests.Count ||
      commandState.Stage != WorldGenerationStage.Framing ||
      commandState.NextSequence != commands.Count)
  {
    throw new InvalidOperationException(
      $"The Pyramid wall-frame command projection failed: {commandFailure}");
  }
  for (int index = 0; index < commands.Count; index++)
  {
    LegacyWallFrameCommand command = commands[index];
    LegacyPyramidWallFrameRequest request = requests[index];
    if (command.Sequence != index ||
        command.X != request.Target.X ||
        command.Y != request.Target.Y ||
        command.Source != LegacyPyramidWallFrameRequestQuery.Source ||
        command.SourceLine != LegacyPyramidWallFrameRequestQuery.SourceLine ||
        command.Result.WallType != 34)
    {
      throw new InvalidOperationException(
        "Pyramid wall-frame commands lost ordered request metadata.");
    }
  }

  WorldGrid committedWorld = WorldGrid.FromSnapshot(commandSnapshot);
  WorldTile committedCenterBefore = committedWorld.GetTile(centerX, centerY);
  LegacyWallFrameCommand expectedCenterCommand = commands
    .Where(command => command.X == centerX && command.Y == centerY)
    .OrderBy(command => command.Sequence)
    .Last();
  if (!new LegacyWallFrameCommandCommitSystem().TryCommit(
        committedWorld,
        commands,
        out LegacyWallFrameCommitResult commitResult) ||
      !commitResult.Succeeded || commitResult.AppliedCount != commands.Count ||
      committedWorld.GetTile(centerX, centerY) != expectedCenterCommand.Result ||
      committedWorld.GetTile(centerX, centerY).FrameX != committedCenterBefore.FrameX ||
      committedWorld.GetTile(centerX, centerY).FrameY != committedCenterBefore.FrameY)
  {
    throw new InvalidOperationException(
      $"Pyramid wall-frame commands did not commit atomically: {commitResult.FailureReason}");
  }

  WorldGrid malformedLookupWorld = WorldGrid.FromSnapshot(commandSnapshot);
  WorldTile malformedLookupBefore = malformedLookupWorld.GetTile(centerX, centerY);
  LegacyWallFrameCommand malformedLookupCommand = commands[0] with
  {
    NeighborMask = commands[0].NeighborMask == 0 ? 1 : 0
  };
  if (new LegacyWallFrameCommandCommitSystem().TryCommit(
        malformedLookupWorld,
        new[] { malformedLookupCommand },
        out _) ||
      malformedLookupWorld.GetTile(centerX, centerY) != malformedLookupBefore)
  {
    throw new InvalidOperationException(
      "Wall-frame command commit accepted a mask/index mismatch.");
  }

  WorldGrid staleWorld = WorldGrid.FromSnapshot(commandSnapshot);
  WorldTile staleBefore = staleWorld.GetTile(centerX, centerY);
  long staleVersion = staleWorld.GetSectionVersion(
    staleWorld.GetSectionCoordinates(centerX, centerY));
  LegacyWallFrameCommand staleCommand = commands[0] with
  {
    ExpectedSectionVersion = staleVersion + 1
  };
  if (new LegacyWallFrameCommandCommitSystem().TryCommit(
        staleWorld,
        new[] { staleCommand },
        out _) ||
      staleWorld.GetTile(centerX, centerY) != staleBefore ||
      staleWorld.GetSectionVersion(
        staleWorld.GetSectionCoordinates(centerX, centerY)) != staleVersion)
  {
    throw new InvalidOperationException(
      "Wall-frame section-version rejection was not atomic.");
  }

  WorldGrid terminalSequenceWorld = WorldGrid.FromSnapshot(commandSnapshot);
  LegacyWallFrameCommand terminalCommand = commands[0] with
  {
    Sequence = long.MaxValue - 1,
    ExpectedSectionVersion = terminalSequenceWorld.GetSectionVersion(
      terminalSequenceWorld.GetSectionCoordinates(centerX, centerY))
  };
  if (new LegacyWallFrameCommandCommitSystem().TryCommit(
        terminalSequenceWorld,
        new[] { terminalCommand },
        out _) ||
      terminalSequenceWorld.GetTile(centerX, centerY) != staleBefore)
  {
    throw new InvalidOperationException(
      "Wall-frame terminal sequence rejection was not atomic.");
  }

  Console.WriteLine(
    "PASS: Pyramid Framing.WallFrame value semantics and typed command commit preserve source " +
    "guards, masks, lookup tables, RNG, and ordered mutation");
  Console.WriteLine(
    "SUMMARY: Pyramid tunnel/features, client visibility state, global RNG/WLD parity, and " +
    "legacy deletion remain deferred");
}

static void RunPyramidWallFrameNeighborFocusedVerification()
{
  const int width = 200;
  const int height = 300;
  const int seed = 1456;
  const int centerX = 100;
  const int centerY = 100;
  WorldMetadata metadata = new(
    "pyramid-wall-frame-neighbor-focused",
    new WorldSeed(seed),
    width,
    height);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldTile center = new(
    IsActive: false,
    Type: 0,
    FrameX: 123,
    FrameY: 234,
    WallType: 34,
    WallColor: 7);
  WorldTile above = new(IsActive: false, Type: 0, WallType: 35);
  WorldTile left = new(IsActive: true, Type: 54, WallType: 0);
  WorldTile right = new(IsActive: true, Type: 1, WallType: 36, IsInvisibleWall: true);
  WorldTile below = new(IsActive: false, Type: 0, WallType: 37);
  if (!world.TrySetTile(centerX, centerY, center) ||
      !world.TrySetTile(centerX, centerY - 1, above) ||
      !world.TrySetTile(centerX - 1, centerY, left) ||
      !world.TrySetTile(centerX + 1, centerY, right) ||
      !world.TrySetTile(centerX, centerY + 1, below))
  {
    throw new InvalidOperationException("The WallFrame neighbor fixture could not be created.");
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  WorldTile beforeCenter = snapshot.GetTile(centerX, centerY);
  WorldSectionCoordinates section = world.GetSectionCoordinates(centerX, centerY);
  long beforeSectionVersion = snapshot.GetSectionVersion(section);
  LegacyPassRandomState random = new(seed);
  WorldGenerationStateComponent state = new(seed);
  if (!LegacyWallFrameNeighborQuery.TryEvaluate(
        snapshot,
        centerX,
        centerY,
        showInvisibleWalls: false,
        out LegacyWallFrameNeighborResult hiddenResult,
        out string? hiddenFailure))
  {
    throw new InvalidOperationException(
      $"WallFrame neighbor classification was rejected: {hiddenFailure}");
  }

  LegacyWallFrameNeighborMask expectedHiddenMask =
    LegacyWallFrameNeighborMask.Above |
    LegacyWallFrameNeighborMask.Left |
    LegacyWallFrameNeighborMask.Below;
  if (hiddenFailure is not null ||
      hiddenResult.X != centerX ||
      hiddenResult.Y != centerY ||
      hiddenResult.OriginalCenterWallType != 34 ||
      hiddenResult.EffectiveCenterWallType != 34 ||
      hiddenResult.NeighborMask != expectedHiddenMask ||
      !hiddenResult.ShouldFrame ||
      hiddenResult.ShowInvisibleWalls ||
      hiddenResult.WasCenterWallTypeNormalized)
  {
    throw new InvalidOperationException(
      "WallFrame neighbor mask did not preserve source bit order or visibility filtering.");
  }

  if (!LegacyWallFrameNeighborQuery.TryEvaluate(
        snapshot,
        centerX,
        centerY,
        showInvisibleWalls: true,
        out LegacyWallFrameNeighborResult visibleResult,
        out string? visibleFailure) ||
      visibleFailure is not null ||
      visibleResult.NeighborMask !=
        (expectedHiddenMask | LegacyWallFrameNeighborMask.Right) ||
      !visibleResult.ShowInvisibleWalls)
  {
    throw new InvalidOperationException(
      "WallFrame neighbor visibility did not retain an invisible qualifying neighbor.");
  }

  IReadOnlySet<ushort> expectedTruncatingTiles = new HashSet<ushort> { 54, 328, 459, 748 };
  if (!LegacyTruncatingWallTileRegistry.RegisterDefaults().SetEquals(expectedTruncatingTiles) ||
      !LegacyTruncatingWallTileRegistry.IsTruncatingTile(54) ||
      !LegacyTruncatingWallTileRegistry.IsTruncatingTile(328) ||
      !LegacyTruncatingWallTileRegistry.IsTruncatingTile(459) ||
      !LegacyTruncatingWallTileRegistry.IsTruncatingTile(748) ||
      LegacyTruncatingWallTileRegistry.IsTruncatingTile(53))
  {
    throw new InvalidOperationException(
      "The TruncatesWalls registry did not preserve the exact legacy set.");
  }

  if (random.SampleCount != 0 ||
      state.Stage != WorldGenerationStage.Created ||
      state.NextSequence != 0 ||
      snapshot.GetTile(centerX, centerY) != beforeCenter ||
      snapshot.GetSectionVersion(section) != beforeSectionVersion)
  {
    throw new InvalidOperationException(
      "WallFrame neighbor classification consumed state or mutated its snapshot input.");
  }

  WorldGrid emptyCenterWorld = WorldGrid.FromSnapshot(snapshot);
  if (!emptyCenterWorld.TrySetTile(
        centerX,
        centerY,
        center with { WallType = 0, FrameX = 345, FrameY = 456 }))
  {
    throw new InvalidOperationException("The empty-center WallFrame fixture could not be created.");
  }

  WorldGridSnapshot emptyCenterSnapshot = emptyCenterWorld.CreateSnapshot(metadata);
  if (!LegacyWallFrameNeighborQuery.TryEvaluate(
        emptyCenterSnapshot,
        centerX,
        centerY,
        showInvisibleWalls: true,
        out LegacyWallFrameNeighborResult emptyResult,
        out string? emptyFailure) ||
      emptyFailure is not null ||
      emptyResult.EffectiveCenterWallType != 0 ||
      emptyResult.NeighborMask != LegacyWallFrameNeighborMask.None ||
      emptyResult.ShouldFrame ||
      emptyResult.WasCenterWallTypeNormalized)
  {
    throw new InvalidOperationException(
      "A zero center wall did not preserve the source no-frame branch.");
  }

  WorldGrid invalidCenterWorld = WorldGrid.FromSnapshot(snapshot);
  if (!invalidCenterWorld.TrySetTile(
        centerX,
        centerY,
        center with { WallType = LegacyLargeFrameWallRegistry.WallTypeCount }))
  {
    throw new InvalidOperationException(
      "The invalid-center WallFrame fixture could not be created.");
  }

  WorldGridSnapshot invalidCenterSnapshot = invalidCenterWorld.CreateSnapshot(metadata);
  if (!LegacyWallFrameNeighborQuery.TryEvaluate(
        invalidCenterSnapshot,
        centerX,
        centerY,
        showInvisibleWalls: true,
        out LegacyWallFrameNeighborResult invalidResult,
        out string? invalidFailure) ||
      invalidFailure is not null ||
      invalidResult.OriginalCenterWallType != LegacyLargeFrameWallRegistry.WallTypeCount ||
      invalidResult.EffectiveCenterWallType != 0 ||
      invalidResult.NeighborMask != LegacyWallFrameNeighborMask.None ||
      invalidResult.ShouldFrame ||
      !invalidResult.WasCenterWallTypeNormalized)
  {
    throw new InvalidOperationException(
      "An invalid center wall type did not normalize to the source zero-wall branch.");
  }

  if (LegacyWallFrameNeighborQuery.TryEvaluate(
        snapshot,
        0,
        centerY,
        showInvisibleWalls: false,
        out LegacyWallFrameNeighborResult boundaryResult,
        out string? boundaryFailure) ||
      boundaryResult != default ||
      string.IsNullOrWhiteSpace(boundaryFailure))
  {
    throw new InvalidOperationException(
      "A border WallFrame coordinate was not rejected atomically.");
  }

  Console.WriteLine(
    "PASS: Pyramid WallFrame neighbor classification preserves source mask, visibility, and " +
    "zero-wall branches");
  Console.WriteLine(
    "SUMMARY: wall-frame lookup/mutation, wall-21 RNG, tunnel/features, exact global RNG parity, " +
    "publication, and aggregate WLD parity remain deferred");
}

static void RunPyramidTunnelOpeningFocusedVerification()
{
  const int width = 200;
  const int height = 300;
  const int seed = 1456;
  const int originX = 100;
  const int originY = 100;
  const int tunnelWidth = 10;
  WorldMetadata metadata = new(
    "pyramid-tunnel-opening-focused",
    new WorldSeed(seed),
    width,
    height);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  if (!LegacyPyramidStructureRequest.TryCreate(
        originX,
        originY,
        pyramidMinDepth: 10,
        pyramidMaxDepth: 14,
        noTunnel: true,
        out LegacyPyramidStructureRequest request))
  {
    throw new InvalidOperationException("The tunnel-opening Pyramid request was rejected.");
  }

  LegacyPassRandomState expectedRandom = new(seed);
  int expectedDirection = expectedRandom.Next(2) == 0 ? -1 : 1;
  int expectedTunnelHeight = expectedRandom.Next(5, 8);
  int expectedDelay = expectedRandom.Next(20, 30);
  int startX = originX - tunnelWidth * expectedDirection;
  int startY = originY + tunnelWidth;
  int firstRow = startY;
  if (!world.TrySetTile(
        startX,
        firstRow - 1,
        new WorldTile(
          IsActive: true,
          Type: 53,
          LiquidAmount: 12,
          LiquidType: 1,
          FrameX: 27,
          FrameY: -4,
          WallType: 9,
          HasWire: true)) ||
      !world.TrySetTile(
        startX,
        firstRow,
        new WorldTile(
          IsActive: true,
          Type: 151,
          LiquidAmount: 33,
          LiquidType: 2,
          FrameX: 31,
          FrameY: 32,
          WallType: 7,
          HasWire2: true,
          IsHalfBrick: true,
          Slope: 3,
          TileColor: 4,
          WallColor: 5)))
  {
    throw new InvalidOperationException("The tunnel-opening fixture could not be created.");
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  WorldTile beforePillar = snapshot.GetTile(startX, firstRow);
  WorldTile beforeAbove = snapshot.GetTile(startX, firstRow - 1);
  LegacyPassRandomState random = new(seed);
  WorldGenerationStateComponent state = new(seed);
  List<TileChangeCommand> commands = new();
  if (!LegacyPyramidTunnelOpening.TryAppendCommands(
        snapshot,
        request,
        tunnelWidth,
        random,
        ref state,
        commands,
        out LegacyPyramidTunnelOpeningResult result,
        out string? failureReason))
  {
    throw new InvalidOperationException(
      $"Pyramid tunnel-opening projection was rejected: {failureReason}");
  }

  if (failureReason is not null ||
      result.Direction != expectedDirection ||
      result.StartX != startX ||
      result.StartY != startY ||
      result.TunnelHeight != expectedTunnelHeight ||
      result.InitialDelay != expectedDelay ||
      !result.NoTunnelMode ||
      random.SampleCount != 3 ||
      state.Stage != WorldGenerationStage.Structure ||
      state.NextSequence != commands.Count ||
      commands.Count == 0 ||
      snapshot.GetTile(startX, firstRow) != beforePillar ||
      snapshot.GetTile(startX, firstRow - 1) != beforeAbove)
  {
    throw new InvalidOperationException(
      "Pyramid tunnel-opening projection did not preserve source draws or snapshot purity.");
  }

  bool hasSourceWall = false;
  bool hasPillarClear = false;
  bool hasSandRewrite = false;
  for (int index = 0; index < commands.Count; index++)
  {
    TileChangeCommand command = commands[index];
    if (command.Source != LegacyPyramidTunnelOpening.Source ||
        command.Sequence != index)
    {
      throw new InvalidOperationException(
        "Tunnel-opening commands lost source attribution or sequence order.");
    }

    if (command.Kind == TileChangeKind.SetWall && command.WallType == 34)
    {
      hasSourceWall = true;
    }

    if (command.Kind == TileChangeKind.UpdateTileType &&
        command.TileType == 151 && command.IsActive == false)
    {
      hasPillarClear = true;
    }

    if (command.Kind == TileChangeKind.UpdateTileType &&
        command.TileType == 53 && command.IsActive == true)
    {
      hasSandRewrite = true;
    }
  }

  if (!hasSourceWall || !hasPillarClear || !hasSandRewrite)
  {
    throw new InvalidOperationException(
      "Tunnel-opening projection omitted a source wall, pillar clear, or sand rewrite.");
  }

  WorldGrid committedWorld = WorldGrid.FromSnapshot(snapshot);
  if (!new TileChangeCommitSystem().TryCommit(
        committedWorld,
        commands,
        out TileChangeCommitResult commitResult) ||
      !commitResult.Succeeded ||
      commitResult.AppliedCount != commands.Count)
  {
    throw new InvalidOperationException(
      $"Tunnel-opening commands did not commit atomically: {commitResult.FailureReason}");
  }

  WorldTile committedPillar = committedWorld.GetTile(startX, firstRow);
  WorldTile committedWallBelow = committedWorld.GetTile(startX, firstRow + 1);
  WorldTile committedWallSide = committedWorld.GetTile(startX + expectedDirection, firstRow);
  if (!committedPillar.IsActive ||
      committedPillar.Type != 53 ||
      committedPillar.LiquidAmount != beforePillar.LiquidAmount ||
      committedPillar.FrameX != beforePillar.FrameX ||
      committedPillar.FrameY != beforePillar.FrameY ||
      !committedPillar.HasWire2 ||
      committedWallBelow.WallType != 34 ||
      committedWallSide.WallType != 34)
  {
    throw new InvalidOperationException(
      "Tunnel-opening commit did not preserve source untouched fields or wall writes.");
  }

  WorldGrid invalidWorld = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot invalidSnapshot = invalidWorld.CreateSnapshot(metadata);
  LegacyPassRandomState invalidRandom = new(seed);
  WorldGenerationStateComponent invalidState = new(seed);
  List<TileChangeCommand> invalidCommands = new();
  if (!LegacyPyramidStructureRequest.TryCreate(
        originX: 1,
        originY,
        pyramidMinDepth: 10,
        pyramidMaxDepth: 14,
        noTunnel: true,
        out LegacyPyramidStructureRequest invalidRequest))
  {
    throw new InvalidOperationException("The invalid tunnel-opening request could not be created.");
  }

  if (LegacyPyramidTunnelOpening.TryAppendCommands(
        invalidSnapshot,
        invalidRequest,
        tunnelWidth,
        invalidRandom,
        ref invalidState,
        invalidCommands,
        out _,
        out string? invalidFailure) ||
      invalidCommands.Count != 0 ||
      invalidRandom.SampleCount != 0 ||
      invalidState.Stage != WorldGenerationStage.Created ||
      string.IsNullOrWhiteSpace(invalidFailure))
  {
    throw new InvalidOperationException(
      "An out-of-envelope tunnel-opening request was not rejected atomically.");
  }

  Console.WriteLine(
    $"PASS: Pyramid noTunnel opening preserves direction, {result.CommandsVisited} columns, " +
    $"{commands.Count} typed commands, and source random accounting");
  Console.WriteLine(
    "SUMMARY: Pyramid feature intents, final extended tunnel, client visibility, exact global " +
    "RNG/WLD parity, publication, legacy deletion, and 44 ServerRelevant rows remain deferred");
}

static void RunPyramidBuriedChestFocusedVerification()
{
  const int width = 200;
  const int height = 300;
  const int seed = 1456;
  const int openingY = 130;
  WorldMetadata metadata = new(
    "pyramid-buried-chest-focused",
    new WorldSeed(seed),
    width,
    height);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  LegacyPassRandomState random = new(seed);
  WorldGenerationStateComponent state = new(seed);
  if (!LegacyPyramidBuriedChestIntentPolicy.TryCreateIntent(
        snapshot,
        tunnelStartX: 90,
        tunnelEndX: 110,
        openingY,
        tenthAnniversaryWorld: false,
        random,
        ref state,
        out LegacyPyramidBuriedChestIntent intent,
        out string? failureReason))
  {
    throw new InvalidOperationException(
      $"Pyramid buried-chest intent was rejected: {failureReason}");
  }

  if (failureReason is not null ||
      intent.Sequence != 0 ||
      intent.TileX != 100 ||
      intent.TileY != openingY ||
      intent.MainItemInChest != 857 ||
      intent.NotNearOtherChests ||
      intent.ChestStyle != 1 ||
      intent.TrySlope ||
      intent.ChestTileType != 0 ||
      random.SampleCount != 1 ||
      state.Stage != WorldGenerationStage.Structure ||
      state.NextSequence != 1 ||
      LegacyPyramidBuriedChestIntentPolicy.SourceLine != 28555)
  {
    throw new InvalidOperationException(
      "Pyramid buried-chest intent did not preserve the source call and random accounting.");
  }

  List<TileChangeCommand> chestCommands = new();
  WorldGenerationStateComponent placementState = state;
  if (!LegacyPyramidBuriedChestCommandProjection.TryAppendCommands(
        snapshot,
        intent,
        ref placementState,
        chestCommands,
        out LegacyPyramidBuriedChestCommandProjectionResult placementResult,
        out string? placementFailure) ||
      placementFailure is not null ||
      placementResult.OriginX != 99 ||
      placementResult.OriginY != openingY - 1 ||
      placementResult.ChestTileType != 21 ||
      placementResult.ChestStyle != 1 ||
      placementResult.MainItemInChest != 857 ||
      placementResult.TileCommandCount != 4 ||
      chestCommands.Count != 4 ||
      placementState.NextSequence != 5)
  {
    throw new InvalidOperationException(
      $"Pyramid buried-chest command projection failed: {placementFailure}");
  }

  (int X, int Y, short FrameX, short FrameY)[] expectedChestTiles =
  [
    (99, openingY - 1, 36, 0),
    (99, openingY, 36, 18),
    (100, openingY - 1, 54, 0),
    (100, openingY, 54, 18)
  ];
  for (int index = 0; index < chestCommands.Count; index++)
  {
    TileChangeCommand command = chestCommands[index];
    (int x, int y, short frameX, short frameY) = expectedChestTiles[index];
    if (command.Sequence != index + 1 ||
        command.X != x ||
        command.Y != y ||
        command.Kind != TileChangeKind.Place ||
        command.TileType != 21 ||
        command.FrameX != frameX ||
        command.FrameY != frameY ||
        command.Source != LegacyPyramidBuriedChestCommandProjection.Source ||
        command.SourceLine != LegacyPyramidBuriedChestCommandProjection.SourceLine ||
        command.ExpectedSectionVersion != snapshot.GetSectionVersion(
          snapshot.Metadata.IsInside(x, y)
            ? new WorldSectionCoordinates(x / WorldGrid.SectionWidth, y / WorldGrid.SectionHeight)
            : default))
    {
      throw new InvalidOperationException(
        "Pyramid buried-chest commands lost source order, frame layout, or section guard.");
    }
  }

  WorldGrid committedWorld = WorldGrid.FromSnapshot(snapshot);
  if (!new TileChangeCommitSystem().TryCommit(
        committedWorld,
        chestCommands,
        out TileChangeCommitResult commitResult) ||
      !commitResult.Succeeded ||
      commitResult.AppliedCount != chestCommands.Count)
  {
    throw new InvalidOperationException(
      $"Pyramid buried-chest tile commands did not commit: {commitResult.FailureReason}");
  }

  for (int index = 0; index < expectedChestTiles.Length; index++)
  {
    (int x, int y, short frameX, short frameY) = expectedChestTiles[index];
    WorldTile tile = committedWorld.GetTile(x, y);
    if (!tile.IsActive ||
        tile.Type != 21 ||
        tile.FrameX != frameX ||
        tile.FrameY != frameY ||
        tile.IsHalfBrick ||
        tile.Slope != 0)
    {
      throw new InvalidOperationException(
        "Pyramid buried-chest tile commit did not preserve the source 2x2 footprint.");
    }
  }

  WorldGenerationStateComponent invalidState = state;
  List<TileChangeCommand> invalidCommands = new();
  if (LegacyPyramidBuriedChestCommandProjection.TryAppendCommands(
        snapshot,
        intent with { TileX = 0 },
        ref invalidState,
        invalidCommands,
        out _,
        out string? invalidFailure) ||
      invalidCommands.Count != 0 ||
      invalidState.NextSequence != state.NextSequence ||
      string.IsNullOrWhiteSpace(invalidFailure))
  {
    throw new InvalidOperationException(
      "An out-of-envelope Pyramid buried-chest command was not rejected atomically.");
  }

  LegacyPassRandomState anniversaryRandom = new(1);
  WorldGenerationStateComponent anniversaryState = new(seed, nextSequence: 7);
  if (!LegacyPyramidBuriedChestIntentPolicy.TryCreateIntent(
        snapshot,
        tunnelStartX: 110,
        tunnelEndX: 90,
        openingY,
        tenthAnniversaryWorld: true,
        anniversaryRandom,
        ref anniversaryState,
        out LegacyPyramidBuriedChestIntent anniversaryIntent,
        out string? anniversaryFailure) ||
      anniversaryFailure is not null ||
      anniversaryIntent.Sequence != 7 ||
      anniversaryIntent.TileX != 100 ||
      anniversaryIntent.MainItemInChest != 857 ||
      anniversaryRandom.SampleCount != 2 ||
      anniversaryState.NextSequence != 8)
  {
    throw new InvalidOperationException(
      "Pyramid buried-chest intent did not preserve reversed bounds or anniversary selection.");
  }

  LegacyPassRandomState invalidRandom = new(seed);
  WorldGenerationStateComponent invalidIntentState = new(seed);
  if (LegacyPyramidBuriedChestIntentPolicy.TryCreateIntent(
        snapshot,
        tunnelStartX: -1,
        tunnelEndX: 110,
        openingY,
        tenthAnniversaryWorld: false,
        invalidRandom,
        ref invalidIntentState,
        out _,
        out string? invalidIntentFailure) ||
      invalidRandom.SampleCount != 0 ||
      invalidIntentState.Stage != WorldGenerationStage.Created ||
      invalidIntentState.NextSequence != 0 ||
      string.IsNullOrWhiteSpace(invalidIntentFailure))
  {
    throw new InvalidOperationException(
      "An invalid Pyramid buried-chest intent was not rejected atomically.");
  }

  Console.WriteLine(
    $"PASS: Pyramid buried-chest intent preserves midpoint, source item selection, " +
    $"tile command projection/commit, and {random.SampleCount} random sample");
  Console.WriteLine(
    "SUMMARY: buried-chest entity/item-fill registration, pile/plant/pot features, final " +
    "extended tunnel, aggregate RNG/WLD parity, publication, legacy deletion, and 44 " +
    "ServerRelevant rows remain deferred");
}
