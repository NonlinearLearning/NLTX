using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Actions;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldSession.Components;
using Terraria.Content;
using SavedOreTierState = Terraria.WorldSession.Components.OreTierState;

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

WorldTileMergeCullStateQuery.TileMergeNeighborhood snapshot =
  new(
    CenterInvisible: false,
    TopInvisible: true,
    BottomInvisible: null,
    LeftInvisible: false,
    RightInvisible: true,
    TopLeftInvisible: null,
    TopRightInvisible: false,
    BottomLeftInvisible: true,
    BottomRightInvisible: null,
    FramingRevision: 7);

WorldTileMergeCullStateQuery.CullResult result =
  WorldTileMergeCullStateQuery.Calculate(
    snapshot,
    new WorldTileMergeCullStateQuery.CullRules(7));

Require(result.CullTop, "A different top visibility should be culled.");
Require(!result.CullLeft, "An equal left visibility should not be culled.");
Require(result.CullRight, "A different right visibility should be culled.");
Require(!result.CullBottom, "A missing neighbor should follow the no-cull edge policy.");
Require(result.FramingRevision == 7, "The result must retain the snapshot revision.");

bool staleRejected = false;
try
{
  WorldTileMergeCullStateQuery.Calculate(
    snapshot,
    new WorldTileMergeCullStateQuery.CullRules(8));
}
catch (ArgumentException)
{
  staleRejected = true;
}

Require(staleRejected, "A stale framing revision must be rejected.");

WorldGenerationTileSetActionsCommand setTile =
  WorldGenerationTileSetActionsCommand.SetTile(
    new(12, 8),
    tileType: 42);
Require(
  setTile.Kind == WorldGenerationTileSetActionsCommand.OperationKind.SetTile &&
  setTile.TileType == 42 &&
  !setTile.FrameSelf &&
  setTile.FrameNeighbors &&
  setTile.ClearBeforeSet,
  "SetTile must retain the Version4 constructor defaults.");
setTile.Validate();

WorldGenerationTileSetActionsCommand clearTile =
  WorldGenerationTileSetActionsCommand.ClearTile(new(12, 8));
Require(
  clearTile.Kind == WorldGenerationTileSetActionsCommand.OperationKind.ClearTile &&
  !clearTile.FrameNeighbors,
  "ClearTile must default to local framing only.");

WorldGenerationTileSetActionsCommand invalidSlope =
  WorldGenerationTileSetActionsCommand.SetSlope(new(12, 8), -1);
bool invalidSlopeRejected = false;
try
{
  invalidSlope.Validate();
}
catch (ArgumentException)
{
  invalidSlopeRejected = true;
}

Require(invalidSlopeRejected, "Negative slopes must be rejected before commit.");

WorldGenerationWallMutationActionsCommand wallSet =
  WorldGenerationWallMutationActionsCommand.SetWall(
    new(12, 8),
    wallType: 17);
Require(
  wallSet.Kind == WorldGenerationWallMutationActionsCommand.OperationKind.SetWall &&
  wallSet.WallType == 17 &&
  !wallSet.FrameSelf &&
  wallSet.FrameNeighbors &&
  wallSet.ClearTile,
  "SetWall must retain its source framing and clear-tile defaults.");
wallSet.Validate();

WorldGenerationWallMutationActionsCommand clearWall =
  WorldGenerationWallMutationActionsCommand.ClearWall(new(12, 8));
Require(
  clearWall.Kind == WorldGenerationWallMutationActionsCommand.OperationKind.ClearWall &&
  !clearWall.FrameNeighbors &&
  !clearWall.ClearTile,
  "ClearWall must not request an implicit tile clear.");

WorldGenerationWallMutationActionsCommand placeWall =
  WorldGenerationWallMutationActionsCommand.PlaceWall(new(12, 8), 18);
Require(
  placeWall.Kind == WorldGenerationWallMutationActionsCommand.OperationKind.PlaceWall &&
  placeWall.PlaceNeighbors,
  "PlaceWall must default to neighbor handling.");
placeWall.Validate();

WorldGenerationTilePlacementAndPaintActionsCommand tilePaint =
  WorldGenerationTilePlacementAndPaintActionsCommand.SetTilePaint(
    new(12, 8),
    paintId: 3);
Require(
  tilePaint.Kind == WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.SetTilePaint &&
  tilePaint.PaintId == 3,
  "Tile paint must preserve its operation and paint identifier.");
tilePaint.Validate();

WorldGenerationTilePlacementAndPaintActionsCommand placeTile =
  WorldGenerationTilePlacementAndPaintActionsCommand.PlaceTile(
    new(12, 8),
    tileType: 42);
Require(
  placeTile.Kind == WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.PlaceTile &&
  placeTile.TileType == 42 &&
  placeTile.Style == 0,
  "PlaceTile must preserve its source default style.");
placeTile.Validate();

WorldGenerationTilePlacementAndPaintActionsCommand combinedPaint =
  WorldGenerationTilePlacementAndPaintActionsCommand.SetTileAndWallPaint(
    new(12, 8),
    paintId: 4);
Require(
  combinedPaint.Kind == WorldGenerationTilePlacementAndPaintActionsCommand.OperationKind.SetTileAndWallPaint &&
  combinedPaint.PaintId == 4,
  "Combined paint must remain a distinct operation kind.");

WorldGenerationLiquidAndNeighborActionsCommand setLiquid =
  WorldGenerationLiquidAndNeighborActionsCommand.SetLiquid(
    new(12, 8),
    liquidType: 2);
Require(
  setLiquid.Kind == WorldGenerationLiquidAndNeighborActionsCommand.OperationKind.SetLiquid &&
  setLiquid.Target == new TilePosition(12, 8) &&
  setLiquid.LiquidType == 2 &&
  setLiquid.LiquidLevel == byte.MaxValue &&
  !setLiquid.ApplyToNeighbors,
  "SetLiquid must preserve its target, type, full-level default, and explicit neighbor policy.");
setLiquid.Validate();

WorldGenerationLiquidAndNeighborActionsCommand smooth =
  WorldGenerationLiquidAndNeighborActionsCommand.Smooth(new(12, 8));
Require(
  smooth.Kind == WorldGenerationLiquidAndNeighborActionsCommand.OperationKind.Smooth &&
  smooth.Target == new TilePosition(12, 8) &&
  !smooth.ApplyToNeighbors,
  "Smooth must preserve its target and default to local smoothing.");
smooth.Validate();

ushort[] scanFilter = { 4, 7, 4 };
WorldGenerationTileScanAndControlActionsCommand.TileCountAccumulator tileCounts =
  new(scanFilter);
scanFilter[0] = 99;
Require(
  tileCounts.TileIds[0] == 4,
  "Tile scanner filters must be copied at the pass boundary.");
tileCounts.Record(4);
tileCounts.Record(4);
tileCounts.Record(7);
tileCounts.Record(8);
WorldGenerationTileScanAndControlActionsCommand.TileCountSnapshot tileCountSnapshot =
  tileCounts.CreateSnapshot();
Require(
  tileCountSnapshot.Counts[4] == 2 &&
  tileCountSnapshot.Counts[7] == 1 &&
  !tileCountSnapshot.Counts.ContainsKey(8) &&
  tileCountSnapshot.TotalMatches == 3,
  "Tile scanner accumulation must count only the copied filter values.");
tileCounts.Reset();
Require(
  tileCounts.CreateSnapshot().TotalMatches == 0,
  "Tile scanner accumulation must be resettable between passes.");

TestScanControlAdapters();

WorldGenerationTileFramingAndDebugActionsCommand frameCommand =
  WorldGenerationTileFramingAndDebugActionsCommand.SetFrames(new(12, 8));
Require(
  frameCommand.Kind ==
    WorldGenerationTileFramingAndDebugActionsCommand.OperationKind.SetFrames &&
  frameCommand.Target == new TilePosition(12, 8) &&
  !frameCommand.FrameNeighbors &&
  frameCommand.DiagnosticColor == default,
  "SetFrames must retain its target, default neighbor policy, and empty diagnostic value.");
frameCommand.Validate();

TestDiagnosticSink diagnosticSink = new();
ColorRgba diagnosticColor = new(10, 20, 30, 40);
WorldGenerationTileFramingAndDebugActionsCommand debugCommand =
  WorldGenerationTileFramingAndDebugActionsCommand.DebugDraw(
    new(12, 8),
    diagnosticColor,
    diagnosticSink);
Require(
  debugCommand.Kind ==
    WorldGenerationTileFramingAndDebugActionsCommand.OperationKind.DebugDraw &&
  debugCommand.Target == new TilePosition(12, 8) &&
  debugCommand.DiagnosticColor == diagnosticColor &&
  ReferenceEquals(debugCommand.DiagnosticSink, diagnosticSink) &&
  !debugCommand.FrameNeighbors,
  "DebugDraw must preserve its color and explicit projection sink without framing intent.");
debugCommand.Validate();

TestConditionsAndSearches();
TestShapeDataDefinitionQuery();
TestShapeModifierStateDefinitionQuery();
TestTileWallConditionStateQuery();

WorldSavedOreTierStateComponent savedOreTiers =
  new(new SavedOreTierState(7, 6, 9, 8, 221, 222, 223));

SavedOreTierState savedSnapshot = WorldSavedOreTierQuery.Snapshot(savedOreTiers);
Require(
  savedSnapshot == new SavedOreTierState(7, 6, 9, 8, 221, 222, 223),
  "The saved-tier query must expose the complete seven-value snapshot.");

WorldSavedOreTierResetSystem.Reset(savedOreTiers);
Require(
  savedOreTiers.Value == SavedOreTierState.Uninitialized,
  "Saved-tier reset must restore all seven values to -1.");

WorldSavedOreTierRepairSystem.Apply(
  savedOreTiers,
  new WorldSavedOreTierTileCounts(
    CopperVanillaCount: 4,
    CopperAlternateCount: 4,
    IronVanillaCount: 5,
    IronAlternateCount: 3,
    SilverVanillaCount: 2,
    SilverAlternateCount: 3,
    GoldVanillaCount: 8,
    GoldAlternateCount: 8));

Require(
  savedOreTiers.Value == new SavedOreTierState(166, 6, 168, 169, -1, -1, -1),
  "Saved-tier repair must repair all low tiers, use alternate IDs on ties, and preserve high tiers.");

WorldSavedOreTierCommitSystem commitSystem = new(savedOreTiers);
commitSystem.CommitGeneration(new WorldSavedOreTierGenerationCommit(7, 6, 9, 8));
Require(
  savedOreTiers.Value == new SavedOreTierState(7, 6, 9, 8, -1, -1, -1),
  "Generation commit must replace only the first four saved tiers.");

commitSystem.CommitAltar(new WorldSavedOreTierAltarCommit(221, 222, 223));
Require(
  savedOreTiers.Value == new SavedOreTierState(7, 6, 9, 8, 221, 222, 223),
  "Altar commit must replace only the three high saved tiers.");

bool invalidGenerationRejected = false;
try
{
  commitSystem.CommitGeneration(new WorldSavedOreTierGenerationCommit(-1, 6, 9, 8));
}
catch (ArgumentOutOfRangeException)
{
  invalidGenerationRejected = true;
}

Require(invalidGenerationRejected, "A negative generation tier must be rejected.");
Require(
  savedOreTiers.Value == new SavedOreTierState(7, 6, 9, 8, 221, 222, 223),
  "A rejected generation commit must not partially update the saved-tier owner.");

WorldFileSavedOreTierAdapter worldFileAdapter = new();
SavedOreTierState legacyUninitialized = worldFileAdapter.Read(
  new WorldSavedOreTierFileInput(
    VersionNumber: 23,
    AltarCount: 0,
    Cobalt: null,
    Mythril: null,
    Adamantite: null,
    Copper: null,
    Iron: null,
    Silver: null,
    Gold: null));
Require(
  legacyUninitialized == SavedOreTierState.Uninitialized,
  "Version 23 with no broken altars must preserve the uninitialized saved-tier sentinel.");

SavedOreTierState legacyDefaults = worldFileAdapter.Read(
  new WorldSavedOreTierFileInput(
    VersionNumber: 23,
    AltarCount: 1,
    Cobalt: null,
    Mythril: null,
    Adamantite: null,
    Copper: null,
    Iron: null,
    Silver: null,
    Gold: null));
Require(
  legacyDefaults == new SavedOreTierState(-1, -1, -1, -1, 107, 108, 111),
  "Version 23 with an altar must retain the declaration high-tier defaults.");

SavedOreTierState version54 = worldFileAdapter.Read(
  new WorldSavedOreTierFileInput(
    VersionNumber: 54,
    AltarCount: 0,
    Cobalt: 221,
    Mythril: 222,
    Adamantite: 223,
    Copper: null,
    Iron: null,
    Silver: null,
    Gold: null));
Require(
  version54 == new SavedOreTierState(-1, -1, -1, -1, 221, 222, 223),
  "Version 54 must directly read high tiers while leaving the pre-216 low tiers uninitialized.");

SavedOreTierState version216 = worldFileAdapter.Read(
  new WorldSavedOreTierFileInput(
    VersionNumber: 216,
    AltarCount: 3,
    Cobalt: 221,
    Mythril: 222,
    Adamantite: 223,
    Copper: 7,
    Iron: 6,
    Silver: 9,
    Gold: 8));
Require(
  version216 == new SavedOreTierState(7, 6, 9, 8, 221, 222, 223),
  "Version 216 must directly read all seven saved tiers.");

WorldSavedOreTierFileSaveValues saveValues = worldFileAdapter.PrepareSave(
  new SavedOreTierState(7, 6, 9, 8, 221, 222, 223));
Require(
  saveValues == new WorldSavedOreTierFileSaveValues(221, 222, 223, 7, 6, 9, 8),
  "World-file save values must preserve the legacy high-tier-first order.");

bool missingDirectFieldRejected = false;
try
{
  worldFileAdapter.Read(
    new WorldSavedOreTierFileInput(
      VersionNumber: 216,
      AltarCount: 0,
      Cobalt: 221,
      Mythril: 222,
      Adamantite: 223,
      Copper: null,
      Iron: 6,
      Silver: 9,
      Gold: 8));
}
catch (ArgumentException)
{
  missingDirectFieldRejected = true;
}

Require(
  missingDirectFieldRejected,
  "A missing directly versioned world-file field must be rejected before state publication.");

NetMessageSavedOreTierAdapter networkAdapter = new();
WorldSavedOreTierNetworkFields networkFields = networkAdapter.Encode(
  new SavedOreTierState(7, 6, 9, 8, 221, 222, 223));
Require(
  networkFields == new WorldSavedOreTierNetworkFields(7, 6, 9, 8, 221, 222, 223),
  "Saved-tier network projection must preserve the seven outbound field positions.");

WorldSavedOreTierNetworkFields wrappedNetworkFields = networkAdapter.Encode(
  new SavedOreTierState(65535, 65534, 65533, 65532, 65531, 65530, 65529));
Require(
  wrappedNetworkFields == new WorldSavedOreTierNetworkFields(-1, -2, -3, -4, -5, -6, -7),
  "Saved-tier network projection must preserve Version4 unchecked signed-short conversion.");

if (args.Contains("--c14-only", StringComparer.Ordinal))
{
  Console.WriteLine("C14 focused verifier passed.");
  return;
}

TestJungleRegionStructureState();
TestJungleRegionStructureCommit();
TestPyramidPlacementCommit();
TestJungleChestAndLootCommit();
TestPyramidPlacementState();
TestJungleChestAndLootState();
TestUndergroundDesertStructureCommit();
TestSurfaceTunnelHistory();
TestSurfaceOrePatchHistory();
TestMushroomBiomeAnchorState();
TestFallenLogFlowerHandoff();

Console.WriteLine("WorldTileMergeCullStateQuery focused verifier passed.");
Console.WriteLine("WorldSavedOreTier C14 focused verifier passed.");
Console.WriteLine("WorldFileSavedOreTierAdapter C14 focused verifier passed.");
Console.WriteLine("NetMessageSavedOreTierAdapter C14 focused verifier passed.");
Console.WriteLine("WorldGenJungleStructureState C07 focused verifier passed.");
Console.WriteLine("SurfaceTunnelHistoryState C09 focused verifier passed.");
Console.WriteLine("SurfaceOrePatchHistoryState C09 focused verifier passed.");
Console.WriteLine("WorldGenMushroomBiomeAndLogState C10 focused verifier passed.");

static void TestJungleRegionStructureState()
{
  JungleRegionStructureComponent component = new(
    generationId: 17,
    extraBastStatueCount: 2,
    extraBastStatueCountMax: 4,
    jungleOriginX: 120,
    jungleMinX: 90,
    jungleMaxX: 150,
    jungleHut: 119,
    mudWall: true);

  JungleRegionStructureSnapshot snapshot = component.CreateSnapshot();
  Require(snapshot.GenerationId == 17, "jungle region generation");
  Require(snapshot.ExtraBastStatueCount == 2, "jungle region statue count");
  Require(snapshot.ExtraBastStatueCountMax == 4, "jungle region statue maximum");
  Require(snapshot.JungleOriginX == 120, "jungle region origin");
  Require(snapshot.JungleMinX == 90, "jungle region minimum");
  Require(snapshot.JungleMaxX == 150, "jungle region maximum");
  Require(snapshot.JungleHut == 119, "jungle region hut definition");
  Require(snapshot.MudWall, "jungle region mud-wall mode");
}

static void TestConditionsAndSearches()
{
  Dictionary<TilePosition, WorldGenerationConditionsAndSearchesQuery.TileSnapshot> tiles =
    new()
    {
      [new TilePosition(1, 1)] = new(
        Exists: true,
        IsActive: true,
        TileType: 42,
        IsSolid: true,
        LiquidAmount: 0,
        LiquidType: 0,
        IsTileCut: false),
      [new TilePosition(2, 1)] = new(
        Exists: true,
        IsActive: true,
        TileType: 43,
        IsSolid: false,
        LiquidAmount: 32,
        LiquidType: 1,
        IsTileCut: false),
      [new TilePosition(3, 1)] = new(
        Exists: true,
        IsActive: false,
        TileType: 0,
        IsSolid: false,
        LiquidAmount: 0,
        LiquidType: 0,
        IsTileCut: false),
      [new TilePosition(11, 11)] = new(
        Exists: true,
        IsActive: true,
        TileType: 44,
        IsSolid: true,
        LiquidAmount: 0,
        LiquidType: 0,
        IsTileCut: false),
    };

  WorldGenerationConditionsAndSearchesQuery.TileWorldSnapshot snapshot =
    new(new(30, 30), tiles);
  tiles[new TilePosition(1, 1)] = default;

  WorldGenerationConditionsAndSearchesQuery.Condition tileCondition =
    WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(7, 42);
  Require(
    WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(1, 1),
      tileCondition),
    "IsTile must use the copied type list and active tile fact.");
  Require(
    !WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(3, 1),
      tileCondition),
    "IsTile must reject an inactive tile.");

  Require(
    WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(0, 0),
      WorldGenerationConditionsAndSearchesQuery.Conditions.BoolCheck(true)),
    "BoolCheck must return its immutable boolean input.");
  Require(
    !WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(0, 0),
      WorldGenerationConditionsAndSearchesQuery.Conditions.Continue()),
    "Continue must remain a false qualification condition.");
  Require(
    WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(1, 1),
      WorldGenerationConditionsAndSearchesQuery.Conditions.InWorld(1)) &&
    !WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(0, 1),
      WorldGenerationConditionsAndSearchesQuery.Conditions.InWorld(1)),
    "InWorld must use inclusive lower and exclusive upper bounds with fluff.");
  Require(
    WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(11, 11),
      WorldGenerationConditionsAndSearchesQuery.Conditions.IsSolid()),
    "IsSolid must use the explicit active and solid facts.");
  Require(
    WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(2, 1),
      WorldGenerationConditionsAndSearchesQuery.Conditions.HasLava()),
    "HasLava must recognize a positive lava liquid fact.");
  Require(
    !WorldGenerationConditionsAndSearchesQuery.EvaluateCondition(
      snapshot,
      new TilePosition(0, 0),
      WorldGenerationConditionsAndSearchesQuery.Conditions.NotNull()),
    "NotNull must reject an absent tile snapshot.");

  WorldGenerationConditionsAndSearchesQuery.SearchDefinition rightSearch =
    new WorldGenerationConditionsAndSearchesQuery.Searches.Right(
      maxDistance: 4,
      WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(43));
  WorldGenerationConditionsAndSearchesQuery.SearchResult found =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(1, 1),
      rightSearch);
  Require(
    found.Found && found.Position == new TilePosition(2, 1) &&
    found.TerminationReason ==
      WorldGenerationConditionsAndSearchesQuery.SearchTerminationReason.Found,
    "Right search must include the origin and return the first matching tile.");

  WorldGenerationConditionsAndSearchesQuery.SearchResult rectangle =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(0, 0),
      new WorldGenerationConditionsAndSearchesQuery.Searches.Rectangle(
        width: 4,
        height: 2,
        WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(43)));
  Require(
    rectangle.Found && rectangle.Position == new TilePosition(2, 1),
    "Rectangle search must scan x-major then y-major within its dimensions.");

  WorldGenerationConditionsAndSearchesQuery.SearchResult notFound =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(1, 1),
      new WorldGenerationConditionsAndSearchesQuery.Searches.Left(
        maxDistance: 0,
        WorldGenerationConditionsAndSearchesQuery.Conditions.IsTile(42)));
  Require(
    !notFound.Found &&
    notFound.Position == WorldGenerationConditionsAndSearchesQuery.NotFound &&
    notFound.Position == WorldGenerationConditionsAndSearchesQuery.NOT_FOUND,
    "Zero-distance search must return the immutable not-found sentinel.");

  WorldGenerationConditionsAndSearchesQuery.SearchResult edge =
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(0, 1),
      new WorldGenerationConditionsAndSearchesQuery.Searches.Left(
        maxDistance: 3,
        WorldGenerationConditionsAndSearchesQuery.Conditions.BoolCheck(true)));
  Require(
    edge.Found && edge.Position == new TilePosition(0, 1),
    "Directional search must evaluate the in-bounds origin before its edge.");

  bool negativeRejected = false;
  try
  {
    _ = new WorldGenerationConditionsAndSearchesQuery.Searches.Up(-1);
  }
  catch (ArgumentOutOfRangeException)
  {
    negativeRejected = true;
  }

  Require(negativeRejected, "Negative search distance must be rejected.");

  Require(
    WorldGenerationConditionsAndSearchesQuery.Search(
      snapshot,
      new TilePosition(1, 1),
      new WorldGenerationConditionsAndSearchesQuery.Searches.Rectangle(
        width: 0,
        height: 2)).Position ==
      WorldGenerationConditionsAndSearchesQuery.NotFound,
    "A zero-width rectangle must have no candidates.");
}

static void TestShapeDataDefinitionQuery()
{
  List<WorldGenerationShapeDataDefinitionQuery.ShapePoint> sourcePoints =
    new()
    {
      new(0, 0),
      new(1, 0),
      new(0, 1),
      new(0, 0),
    };

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition shape =
    new(sourcePoints);
  sourcePoints[0] = new(9, 9);

  Require(shape.Count == 3, "ShapeData must collapse duplicate points.");
  Require(
    shape.Contains(new(0, 0)) && !shape.Contains(new(9, 9)),
    "ShapeData must defensively copy its input points.");
  Require(
    shape.Points[0] == new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0) &&
    shape.Points[1] == new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 1) &&
    shape.Points[2] == new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
    "ShapeData points must have deterministic value ordering.");

  WorldGenerationShapeDataDefinitionQuery.ShapeExecutionPolicy policy =
    new(QuitOnFail: true);
  WorldGenerationShapeDataDefinitionQuery.ShapeOutlineResult cardinalOuter =
    WorldGenerationShapeDataDefinitionQuery.OuterOutline(
      shape,
      useDiagonals: false,
      useInterior: false,
      policy);
  Require(
    cardinalOuter.ExecutionPolicy == policy &&
    cardinalOuter.Points.Contains(new(-1, 0)) &&
    cardinalOuter.Points.Contains(new(0, -1)) &&
    cardinalOuter.Points.Contains(new(2, 0)) &&
    !cardinalOuter.Points.Contains(new(0, 0)),
    "OuterOutline must emit only missing cardinal neighbors when interior is disabled.");

  WorldGenerationShapeDataDefinitionQuery.ShapeOutlineResult diagonalOuter =
    WorldGenerationShapeDataDefinitionQuery.OuterOutline(
      shape,
      useDiagonals: true,
      useInterior: true,
      policy);
  Require(
    diagonalOuter.Points.Length > cardinalOuter.Points.Length &&
    diagonalOuter.Points.Contains(new(0, 0)) &&
    diagonalOuter.Points.Contains(new(1, 1)),
    "OuterOutline must include interior points and diagonal neighbors when requested.");

  WorldGenerationShapeDataDefinitionQuery.ShapeOutlineResult inner =
    WorldGenerationShapeDataDefinitionQuery.InnerOutline(
      shape,
      useDiagonals: false,
      policy);
  Require(
    inner.Points.Length == 3 &&
    inner.Points.Contains(new(0, 0)) &&
    inner.Points.Contains(new(1, 0)) &&
    inner.Points.Contains(new(0, 1)),
    "InnerOutline must retain points with at least one missing neighbor.");

  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition fullBlock =
    new(
      new[]
      {
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(2, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 1),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 1),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(2, 1),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 2),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 2),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(2, 2),
      });
  WorldGenerationShapeDataDefinitionQuery.ShapeOutlineResult fullInner =
    WorldGenerationShapeDataDefinitionQuery.InnerOutline(
      fullBlock,
      useDiagonals: false,
      executionPolicy: default);
  Require(
    !fullInner.Points.Contains(new(1, 1)),
    "InnerOutline must exclude a point surrounded by all required neighbors.");

  Require(
    WorldGenerationShapeDataDefinitionQuery.Count(shape) == shape.Count &&
    WorldGenerationShapeDataDefinitionQuery.OuterOutline(
      shape,
      useDiagonals: true,
      useInterior: true,
      policy) == diagonalOuter,
    "ShapeData count and repeated outline queries must be deterministic.");
}

static void TestShapeModifierStateDefinitionQuery()
{
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition source =
    new(
      new[]
      {
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 1),
      });
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition sourceSnapshot =
    new(source.Points);

    WorldGenerationShapeModifierStateDefinitionQuery.ShapeScaleDefinition scaleDefinition =
      new(2);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition scaled =
    WorldGenerationShapeModifierStateDefinitionQuery.Scale(source, scaleDefinition);
  Require(scaled.Count == 12, "ShapeScale must expand each point into a scaled square.");

  WorldGenerationShapeModifierStateDefinitionQuery.ExpandDefinition expandDefinition =
    new(1, 0);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition expanded =
    WorldGenerationShapeModifierStateDefinitionQuery.Expand(
      new(
        new[]
        {
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        }),
      expandDefinition);
  Require(
    expanded.Count == 3 &&
    expanded.Contains(new(-1, 0)) &&
    expanded.Contains(new(1, 0)),
    "Expand must include the inclusive horizontal radius.");

  WorldGenerationShapeModifierStateDefinitionQuery.OffsetDefinition offsetDefinition =
    new(2, -1);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition offset =
    WorldGenerationShapeModifierStateDefinitionQuery.Offset(source, offsetDefinition);
  Require(
    offset.Contains(new(2, -1)) && offset.Contains(new(3, -1)),
    "Offset must translate every point without mutating the source.");

  WorldGenerationShapeModifierStateDefinitionQuery.FlipDefinition flipDefinition =
    new(FlipX: true, FlipY: false);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition flipped =
    WorldGenerationShapeModifierStateDefinitionQuery.Flip(source, flipDefinition);
  Require(
    flipped.Contains(new(-1, 0)) && flipped.Contains(new(0, 1)),
    "Flip must reflect points around the local origin.");

  WorldGenerationShapeModifierStateDefinitionQuery.RectangleMaskDefinition maskDefinition =
    new(XMin: 0, YMin: 0, XMax: 0, YMax: 1);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition masked =
    WorldGenerationShapeModifierStateDefinitionQuery.RectangleMask(source, maskDefinition);
  Require(masked.Count == 2, "RectangleMask must use inclusive local bounds.");

  WorldGenerationShapeModifierStateDefinitionQuery.CheckerboardDefinition checkerboard =
    new(2);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition checkedShape =
    WorldGenerationShapeModifierStateDefinitionQuery.Checkerboard(
      new(
        new[]
        {
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(2, 2),
        }),
      checkerboard);
  Require(
    checkedShape.Count == 2 &&
    checkedShape.Contains(new(0, 0)) &&
    checkedShape.Contains(new(2, 2)),
    "Checkerboard must retain points on the configured grid.");

  WorldGenerationShapeModifierStateDefinitionQuery.ShapeMembershipDefinition membership =
    new(
      new(
        new[]
        {
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        }));
  Require(
    WorldGenerationShapeModifierStateDefinitionQuery.InShape(source, membership).Count == 1 &&
    WorldGenerationShapeModifierStateDefinitionQuery.NotInShape(source, membership).Count == 2,
    "Shape membership modifiers must partition points without mutation.");

  WorldGenerationShapeModifierStateDefinitionQuery.DitherDefinition dither =
    new(0.5);
  Require(
    WorldGenerationShapeModifierStateDefinitionQuery.Dither(source, dither, 0.5).Count ==
      source.Count &&
    WorldGenerationShapeModifierStateDefinitionQuery.Dither(source, dither, 0.49).Count == 0,
    "Dither must use the supplied random decision and source threshold boundary.");

  WorldGenerationShapeModifierStateDefinitionQuery.RadialDitherDefinition radial =
    new(0, 2);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition radialSource =
    new(
      new[]
      {
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        new WorldGenerationShapeDataDefinitionQuery.ShapePoint(1, 0),
      });
  Require(
    WorldGenerationShapeModifierStateDefinitionQuery.RadialDither(
      radialSource,
      radial,
      new(0.6)).Count == 2 &&
    WorldGenerationShapeModifierStateDefinitionQuery.RadialDither(
      radialSource,
      radial,
      new(0.4)).Count == 1,
    "RadialDither must use an explicit deterministic decision.");

  WorldGenerationShapeModifierStateDefinitionQuery.BlotchesDefinition blotches =
    new(2, 2, 2, 2, 0.5);
  WorldGenerationShapeModifierStateDefinitionQuery.BlotchesRandomInput blotchRandom =
    new(0.1, -1, 1, -1, 1);
  WorldGenerationShapeDataDefinitionQuery.ShapeDataDefinition blotched =
    WorldGenerationShapeModifierStateDefinitionQuery.Blotches(
      new(
        new[]
        {
          new WorldGenerationShapeDataDefinitionQuery.ShapePoint(0, 0),
        }),
      blotches,
      blotchRandom);
  Require(blotched.Count == 9, "Blotches must apply the supplied offset window once.");

  Require(
    source.Count == sourceSnapshot.Count &&
    source.Contains(new(0, 0)) &&
    source.Contains(new(1, 0)) &&
    source.Contains(new(0, 1)),
    "Shape modifiers must not mutate their input shape.");
}

static void TestTileWallConditionStateQuery()
{
  WorldGenerationTileWallConditionStateQuery.TileCellSnapshot center =
    new(
      Exists: true,
      IsActive: true,
      TileType: 5,
      WallType: 7,
      LiquidType: 2,
      LiquidLevel: 100);
  Dictionary<TilePosition, WorldGenerationTileWallConditionStateQuery.TileCellSnapshot>
    neighbors = new()
    {
      [new TilePosition(5, 4)] = center with { TileType = 10 },
      [new TilePosition(6, 5)] = center with { IsActive = false, TileType = 11 },
      [new TilePosition(4, 5)] = center with { TileType = 12 },
      [new TilePosition(5, 6)] = center with { TileType = 13 },
      [new TilePosition(4, 4)] = center with { TileType = 14 },
    };
  WorldGenerationTileWallConditionStateQuery.TileNeighborhoodSnapshot snapshot =
    new(
      new WorldGenerationConditionsAndSearchesQuery.TileWorldBounds(20, 20),
      new TilePosition(5, 5),
      center,
      neighbors);

  ushort[] tileTypes = { 5, 5 };
  Require(
    WorldGenerationTileWallConditionStateQuery.OnlyTiles(snapshot, tileTypes),
    "OnlyTiles must match the active center tile.");
  tileTypes[0] = 99;
  Require(
    WorldGenerationTileWallConditionStateQuery.OnlyTiles(snapshot, new ushort[] { 5 }),
    "Tile type filters must be copied at the condition boundary.");
  Require(
    WorldGenerationTileWallConditionStateQuery.OnlyWalls(snapshot, new ushort[] { 7 }) &&
    !WorldGenerationTileWallConditionStateQuery.OnlyWalls(snapshot, new ushort[] { 8 }),
    "OnlyWalls must match the center wall type.");
  Require(
    WorldGenerationTileWallConditionStateQuery.IsTouching(
      snapshot,
      useDiagonals: false,
      tileIds: new ushort[] { 10 }),
    "IsTouching must use the source cardinal direction order.");
  Require(
    !WorldGenerationTileWallConditionStateQuery.IsTouching(
      snapshot,
      useDiagonals: false,
      tileIds: new ushort[] { 14 }) &&
    WorldGenerationTileWallConditionStateQuery.IsTouching(
      snapshot,
      useDiagonals: true,
      tileIds: new ushort[] { 14 }),
    "IsTouching must expose diagonal selection explicitly.");
  Require(
    WorldGenerationTileWallConditionStateQuery.NotTouching(
      snapshot,
      useDiagonals: false,
      tileIds: new ushort[] { 99 }) &&
    !WorldGenerationTileWallConditionStateQuery.NotTouching(
      snapshot,
      useDiagonals: false,
      tileIds: new ushort[] { 10 }),
    "NotTouching must reject a matching active neighbor.");
  Require(
    WorldGenerationTileWallConditionStateQuery.IsTouchingAir(
      snapshot,
      useDiagonals: false),
    "IsTouchingAir must recognize an inactive cardinal neighbor.");
  Require(
    !WorldGenerationTileWallConditionStateQuery.SkipTiles(snapshot, new ushort[] { 5 }) &&
    WorldGenerationTileWallConditionStateQuery.SkipTiles(snapshot, new ushort[] { 6 }) &&
    !WorldGenerationTileWallConditionStateQuery.SkipWalls(snapshot, new ushort[] { 7 }),
    "Skip tile and wall filters must reject only matching center facts.");
  Require(
    WorldGenerationTileWallConditionStateQuery.HasLiquid(
      snapshot,
      liquidLevel: 100,
      liquidType: 2) &&
    WorldGenerationTileWallConditionStateQuery.HasLiquid(
      snapshot,
      liquidLevel: -1,
      liquidType: -1) &&
    !WorldGenerationTileWallConditionStateQuery.NoLiquid(snapshot, liquidType: 2),
    "Liquid filters must preserve exact and wildcard source semantics.");
  Require(
    WorldGenerationTileWallConditionStateQuery.IsAboveHeight(
      snapshot,
      height: 5,
      inclusive: true) &&
    !WorldGenerationTileWallConditionStateQuery.IsAboveHeight(
      snapshot,
      height: 5,
      inclusive: false) &&
    WorldGenerationTileWallConditionStateQuery.IsBelowHeight(
      snapshot,
      height: 5,
      inclusive: true) &&
    !WorldGenerationTileWallConditionStateQuery.IsBelowHeight(
      snapshot,
      height: 5,
      inclusive: false),
    "Height filters must preserve source inclusive equality behavior.");

  WorldGenerationTileWallConditionStateQuery.ConditionSet conditions =
    WorldGenerationTileWallConditionStateQuery.Compose(
      WorldGenerationTileWallConditionStateQuery.OnlyTilesCondition(5),
      WorldGenerationTileWallConditionStateQuery.OnlyWallsCondition(7));
  WorldGenerationTileWallConditionStateQuery.ConditionResult conditionResult =
    WorldGenerationTileWallConditionStateQuery.Evaluate(snapshot, conditions);
  Require(
    conditionResult.Satisfied &&
    conditionResult.FailureReason ==
      WorldGenerationTileWallConditionStateQuery.ConditionFailureReason.None,
    "Compound conditions must evaluate in caller order over one snapshot.");

  WorldGenerationTileWallConditionStateQuery.TileNeighborhoodSnapshot edgeSnapshot =
    new(
      new WorldGenerationConditionsAndSearchesQuery.TileWorldBounds(1, 1),
      new TilePosition(0, 0),
      center,
      new Dictionary<TilePosition,
        WorldGenerationTileWallConditionStateQuery.TileCellSnapshot>());
  Require(
    WorldGenerationTileWallConditionStateQuery.IsTouchingAir(
      edgeSnapshot,
      useDiagonals: false) &&
    WorldGenerationTileWallConditionStateQuery.NotTouching(
      edgeSnapshot,
      useDiagonals: false,
      tileIds: new ushort[] { 5 }),
    "Out-of-world neighbors must be explicit air and not matching contacts.");
}

static void TestScanControlAdapters()
{
  TestContinuation continuation = new();
  WorldGenerationTileScanAndControlActionsCommand continuationCommand =
    WorldGenerationTileScanAndControlActionsCommand.Continue(continuation);
  Require(
    continuationCommand.Kind ==
      WorldGenerationTileScanAndControlActionsCommand.OperationKind.Continue &&
    ReferenceEquals(continuationCommand.Continuation, continuation),
    "Continuation commands must retain the explicit pass-local adapter.");
  continuationCommand.Validate();

  TestCountSink countSink = new();
  WorldGenerationTileScanAndControlActionsCommand countCommand =
    WorldGenerationTileScanAndControlActionsCommand.Count(countSink);
  WorldGenerationTileScanAndControlActionsCommand scannerCommand =
    WorldGenerationTileScanAndControlActionsCommand.Scanner(countSink);
  Require(
    countCommand.Kind == WorldGenerationTileScanAndControlActionsCommand.OperationKind.Count &&
    scannerCommand.Kind ==
      WorldGenerationTileScanAndControlActionsCommand.OperationKind.Scanner &&
    ReferenceEquals(countCommand.CountSink, countSink) &&
    ReferenceEquals(scannerCommand.CountSink, countSink),
    "Count and scanner commands must retain the explicit count sink.");
  countCommand.Validate();
  scannerCommand.Validate();

  WorldGenerationTileScanAndControlActionsCommand.TileCountAccumulator accumulator =
    new(new ushort[] { 11 });
  WorldGenerationTileScanAndControlActionsCommand tileScannerCommand =
    WorldGenerationTileScanAndControlActionsCommand.TileScanner(accumulator);
  Require(
    tileScannerCommand.Kind ==
      WorldGenerationTileScanAndControlActionsCommand.OperationKind.TileScanner &&
    ReferenceEquals(tileScannerCommand.TileAccumulator, accumulator),
    "Tile scanner commands must retain the pass-local accumulator.");
  tileScannerCommand.Validate();

  TestPerUnitAction perUnitAction = new();
  WorldGenerationTileScanAndControlActionsCommand customCommand =
    WorldGenerationTileScanAndControlActionsCommand.Custom(perUnitAction);
  Require(
    customCommand.Kind == WorldGenerationTileScanAndControlActionsCommand.OperationKind.Custom &&
    ReferenceEquals(customCommand.PerUnitAction, perUnitAction),
    "Custom commands must retain the explicit per-unit adapter.");
  customCommand.Validate();

  TestBoundsReference bounds = new();
  WorldGenerationTileScanAndControlActionsCommand boundsCommand =
    WorldGenerationTileScanAndControlActionsCommand.UpdateBounds(bounds);
  Require(
    boundsCommand.Kind ==
      WorldGenerationTileScanAndControlActionsCommand.OperationKind.UpdateBounds &&
    ReferenceEquals(boundsCommand.Bounds, bounds),
    "Bounds commands must retain an opaque integration value.");
  boundsCommand.Validate();
}

static void TestJungleRegionStructureCommit()
{
  JungleRegionStructureComponent component = new(
    generationId: 17,
    extraBastStatueCount: 1,
    extraBastStatueCountMax: 2,
    jungleOriginX: 10,
    jungleMinX: 5,
    jungleMaxX: 15,
    jungleHut: 7,
    mudWall: false);

  JungleRegionStructureSnapshot committed = new(
    GenerationId: 17,
    ExtraBastStatueCount: 3,
    ExtraBastStatueCountMax: 6,
    JungleOriginX: 120,
    JungleMinX: 90,
    JungleMaxX: 150,
    JungleHut: 119,
    MudWall: true);

  JungleRegionStructureSystem.Commit(component, committed);
  Require(
    component.CreateSnapshot() == committed,
    "jungle-region structure commit must replace the complete snapshot");

  JungleRegionStructureSnapshot beforeMismatch = component.CreateSnapshot();
  JungleRegionStructureSnapshot mismatched = committed with { GenerationId = 18 };
  bool mismatchRejected = false;
  try
  {
    JungleRegionStructureSystem.Commit(component, mismatched);
  }
  catch (ArgumentException)
  {
    mismatchRejected = true;
  }

  Require(mismatchRejected, "a cross-generation jungle-region commit must be rejected");
  Require(
    component.CreateSnapshot() == beforeMismatch,
    "a rejected jungle-region commit must not mutate the owner");
}

static void TestUndergroundDesertStructureCommit()
{
  UndergroundDesertStructureComponent component = new(generationId: 17);
  UndergroundDesertStructureSnapshot initial = component.CreateSnapshot();

  UndergroundDesertStructureSnapshot committed = new(
    GenerationId: 17,
    UndergroundDesertLocation: new UndergroundDesertRectangle(10, 20, 30, 40),
    UndergroundDesertHiveLocation: new UndergroundDesertRectangle(15, 25, 10, 20),
    DesertHiveHigh: 300,
    DesertHiveLow: 100,
    DesertHiveLeft: 12,
    DesertHiveRight: 48);

  UndergroundDesertStructureSystem.Commit(component, committed);
  Require(
    component.CreateSnapshot() == committed,
    "underground-desert structure commit must replace the complete layout snapshot");

  UndergroundDesertStructureSnapshot beforeMismatch = component.CreateSnapshot();
  UndergroundDesertStructureSnapshot mismatched = committed with { GenerationId = 18 };
  bool mismatchRejected = false;
  try
  {
    UndergroundDesertStructureSystem.Commit(component, mismatched);
  }
  catch (ArgumentException)
  {
    mismatchRejected = true;
  }

  Require(mismatchRejected, "a cross-generation desert layout commit must be rejected");
  Require(
    component.CreateSnapshot() == beforeMismatch,
    "a rejected desert layout commit must not mutate the owner");
  Require(
    initial.UndergroundDesertLocation.IsEmpty && initial.UndergroundDesertHiveLocation.IsEmpty,
    "the initial desert layout must use empty rectangle sentinels");
}

static void TestPyramidPlacementCommit()
{
  PyramidPlacementStateComponent component =
    new(generationId: 17, capacity: 3);
  component.TryAppend(1, 2);

  PyramidPlacementSnapshot committed = new(
    GenerationId: 17,
    Capacity: 3,
    Count: 2,
    XPositions: new[] { 101, 102 },
    YPositions: new[] { 201, 202 });

  PyramidPlacementCommitSystem.Commit(component, committed);
  PyramidPlacementSnapshot result = component.CreateSnapshot();
  Require(result.GenerationId == 17, "pyramid commit generation");
  Require(result.Capacity == 3, "pyramid commit capacity");
  Require(result.Count == 2, "pyramid commit count");
  Require(result.XPositions[0] == 101 && result.XPositions[1] == 102,
    "pyramid commit x coordinates");
  Require(result.YPositions[0] == 201 && result.YPositions[1] == 202,
    "pyramid commit y coordinates");

  PyramidPlacementSnapshot beforeMismatch = component.CreateSnapshot();
  bool mismatchRejected = false;
  try
  {
    PyramidPlacementCommitSystem.Commit(
      component,
      committed with { GenerationId = 18 });
  }
  catch (ArgumentException)
  {
    mismatchRejected = true;
  }

  Require(mismatchRejected, "a cross-generation pyramid commit must be rejected");
  PyramidPlacementSnapshot afterMismatch = component.CreateSnapshot();
  Require(afterMismatch.Count == beforeMismatch.Count &&
    afterMismatch.XPositions[0] == beforeMismatch.XPositions[0] &&
    afterMismatch.YPositions[0] == beforeMismatch.YPositions[0],
    "a rejected pyramid commit must not mutate the owner");

  bool invalidCapacityRejected = false;
  try
  {
    PyramidPlacementCommitSystem.Commit(
      component,
      committed with { Capacity = committed.Capacity - 1 });
  }
  catch (ArgumentException)
  {
    invalidCapacityRejected = true;
  }

  Require(invalidCapacityRejected, "a changed pyramid capacity must be rejected");
  Require(
    HasSamePyramidPlacementState(component.CreateSnapshot(), beforeMismatch),
    "an invalid pyramid capacity must not mutate the owner");
}

static bool HasSamePyramidPlacementState(
  PyramidPlacementSnapshot actual,
  PyramidPlacementSnapshot expected)
{
  if (actual.GenerationId != expected.GenerationId ||
      actual.Capacity != expected.Capacity ||
      actual.Count != expected.Count ||
      actual.XPositions.Count != expected.XPositions.Count ||
      actual.YPositions.Count != expected.YPositions.Count)
  {
    return false;
  }

  for (int index = 0; index < actual.Count; index++)
  {
    if (actual.XPositions[index] != expected.XPositions[index] ||
        actual.YPositions[index] != expected.YPositions[index])
    {
      return false;
    }
  }

  return true;
}

static void TestJungleChestAndLootCommit()
{
  JungleChestAndLootGenerationStateComponent component =
    new(generationId: 17);

  JungleChestAndLootGenerationSnapshot committed = new(
    GenerationId: 17,
    JungleItemCount: 4,
    GennedLivingMahoganyWands: true,
    Capacity: JungleChestAndLootGenerationStateComponent.Capacity,
    Count: 2,
    ChestXPositions: new[] { 11, 12 },
    ChestYPositions: new[] { 21, 22 });

  JungleChestPlacementSystem.Commit(component, committed);
  JungleChestAndLootGenerationSnapshot result = component.CreateSnapshot();
  Require(result.GenerationId == 17, "jungle chest commit generation");
  Require(result.JungleItemCount == 4, "jungle chest commit item cursor");
  Require(result.GennedLivingMahoganyWands, "jungle chest commit wand result");
  Require(result.Count == 2, "jungle chest commit count");
  Require(result.ChestXPositions[0] == 11 && result.ChestXPositions[1] == 12,
    "jungle chest commit x coordinates");
  Require(result.ChestYPositions[0] == 21 && result.ChestYPositions[1] == 22,
    "jungle chest commit y coordinates");

  JungleChestAndLootGenerationSnapshot beforeMismatch = component.CreateSnapshot();
  bool mismatchRejected = false;
  try
  {
    JungleChestPlacementSystem.Commit(
      component,
      committed with { GenerationId = 18 });
  }
  catch (ArgumentException)
  {
    mismatchRejected = true;
  }

  Require(mismatchRejected, "a cross-generation jungle chest commit must be rejected");
  JungleChestAndLootGenerationSnapshot afterMismatch = component.CreateSnapshot();
  Require(afterMismatch.Count == beforeMismatch.Count &&
    afterMismatch.JungleItemCount == beforeMismatch.JungleItemCount &&
    afterMismatch.ChestXPositions[0] == beforeMismatch.ChestXPositions[0] &&
    afterMismatch.ChestYPositions[0] == beforeMismatch.ChestYPositions[0],
    "a rejected jungle chest commit must not mutate the owner");

  bool invalidCapacityRejected = false;
  try
  {
    JungleChestPlacementSystem.Commit(
      component,
      committed with { Capacity = committed.Capacity - 1 });
  }
  catch (ArgumentException)
  {
    invalidCapacityRejected = true;
  }

  Require(invalidCapacityRejected, "a changed jungle chest capacity must be rejected");
  Require(
    HasSameJungleChestAndLootState(component.CreateSnapshot(), beforeMismatch),
    "an invalid jungle chest capacity must not mutate the owner");

  bool invalidCountRejected = false;
  try
  {
    JungleChestPlacementSystem.Commit(
      component,
      committed with { Count = committed.Capacity + 1 });
  }
  catch (ArgumentOutOfRangeException)
  {
    invalidCountRejected = true;
  }

  Require(invalidCountRejected, "a jungle chest count above capacity must be rejected");
  Require(
    HasSameJungleChestAndLootState(component.CreateSnapshot(), beforeMismatch),
    "an invalid jungle chest count must not mutate the owner");
}

static bool HasSameJungleChestAndLootState(
  JungleChestAndLootGenerationSnapshot actual,
  JungleChestAndLootGenerationSnapshot expected)
{
  if (actual.GenerationId != expected.GenerationId ||
      actual.JungleItemCount != expected.JungleItemCount ||
      actual.GennedLivingMahoganyWands != expected.GennedLivingMahoganyWands ||
      actual.Capacity != expected.Capacity ||
      actual.Count != expected.Count ||
      actual.ChestXPositions.Count != expected.ChestXPositions.Count ||
      actual.ChestYPositions.Count != expected.ChestYPositions.Count)
  {
    return false;
  }

  for (int index = 0; index < actual.Count; index++)
  {
    if (actual.ChestXPositions[index] != expected.ChestXPositions[index] ||
        actual.ChestYPositions[index] != expected.ChestYPositions[index])
    {
      return false;
    }
  }

  return true;
}

static void TestPyramidPlacementState()
{
  PyramidPlacementStateComponent component =
    new(generationId: 17, capacity: 2);

  Require(component.TryAppend(101, 201), "first pyramid coordinate");
  Require(component.TryAppend(102, 202), "second pyramid coordinate");
  Require(!component.TryAppend(103, 203), "pyramid capacity rejection");

  PyramidPlacementSnapshot snapshot = component.CreateSnapshot();
  Require(snapshot.GenerationId == 17, "pyramid generation");
  Require(snapshot.Capacity == 2, "pyramid dynamic capacity");
  Require(snapshot.Count == 2, "pyramid count");
  Require(snapshot.XPositions[0] == 101, "pyramid first x coordinate");
  Require(snapshot.YPositions[0] == 201, "pyramid first y coordinate");
  Require(snapshot.XPositions[1] == 102, "pyramid second x coordinate");
  Require(snapshot.YPositions[1] == 202, "pyramid second y coordinate");
  Require(
    ((IList<int>)snapshot.XPositions).IsReadOnly,
    "pyramid snapshot x coordinates are read-only");
  Require(
    ((IList<int>)snapshot.YPositions).IsReadOnly,
    "pyramid snapshot y coordinates are read-only");

  component.Clear();
  Require(component.Count == 0, "pyramid reset count");
  Require(snapshot.Count == 2, "pyramid snapshot is isolated from reset");
}

static void TestJungleChestAndLootState()
{
  JungleChestAndLootGenerationStateComponent component =
    new(generationId: 17);
  component.ReplaceLootState(jungleItemCount: 3, gennedLivingMahoganyWands: true);

  for (int index = 0; index < JungleChestAndLootGenerationStateComponent.Capacity; index++)
  {
    Require(
      component.TryAppendChest(index, index + 100),
      "jungle chest coordinate within capacity");
  }

  Require(
    !component.TryAppendChest(100, 200),
    "jungle chest capacity rejection");

  JungleChestAndLootGenerationSnapshot snapshot = component.CreateSnapshot();
  Require(snapshot.GenerationId == 17, "jungle chest generation");
  Require(snapshot.JungleItemCount == 3, "jungle item cursor");
  Require(snapshot.GennedLivingMahoganyWands, "living mahogany wand result");
  Require(snapshot.Capacity == JungleChestAndLootGenerationStateComponent.Capacity,
    "jungle chest fixed capacity");
  Require(snapshot.Count == JungleChestAndLootGenerationStateComponent.Capacity,
    "jungle chest count");
  Require(snapshot.ChestXPositions[0] == 0, "jungle chest first x coordinate");
  Require(snapshot.ChestYPositions[0] == 100, "jungle chest first y coordinate");
  Require(snapshot.ChestXPositions[99] == 99, "jungle chest last x coordinate");
  Require(snapshot.ChestYPositions[99] == 199, "jungle chest last y coordinate");
  Require(
    ((IList<int>)snapshot.ChestXPositions).IsReadOnly,
    "jungle chest snapshot x coordinates are read-only");
  Require(
    ((IList<int>)snapshot.ChestYPositions).IsReadOnly,
    "jungle chest snapshot y coordinates are read-only");

  component.ClearChests();
  Require(component.Count == 0, "jungle chest reset count");
  Require(component.JungleItemCount == 3, "jungle chest reset preserves item cursor");
  Require(component.GennedLivingMahoganyWands, "jungle chest reset preserves wand result");
  Require(snapshot.Count == JungleChestAndLootGenerationStateComponent.Capacity,
    "jungle chest snapshot is isolated from reset");
}

static void TestSurfaceTunnelHistory()
{
  SurfaceTunnelHistoryComponent component =
    new(generationId: 17);

  Require(
    SurfaceTunnelHistoryComponent.Capacity == 50 &&
    SurfaceTunnelHistoryComponent.EffectiveEntryLimit == 49,
    "surface tunnel capacity definitions");

  for (int index = 0; index < SurfaceTunnelHistoryComponent.EffectiveEntryLimit; index++)
  {
    Require(
      SurfaceTunnelHistorySystem.TryAppend(component, 1000 + index),
      "surface tunnel center within effective capacity");
  }

  Require(
    !SurfaceTunnelHistorySystem.TryAppend(component, 2000),
    "surface tunnel effective capacity rejection");

  SurfaceTunnelHistorySnapshot snapshot = SurfaceTunnelHistoryQuery.Snapshot(component);
  Require(snapshot.GenerationId == 17, "surface tunnel generation");
  Require(snapshot.Capacity == 50, "surface tunnel declared capacity");
  Require(snapshot.EffectiveEntryLimit == 49, "surface tunnel effective limit");
  Require(snapshot.Count == 49, "surface tunnel count");
  Require(snapshot.CenterXPositions[0] == 1000, "surface tunnel first center");
  Require(snapshot.CenterXPositions[48] == 1048, "surface tunnel last center");
  Require(
    ((IList<int>)snapshot.CenterXPositions).IsReadOnly,
    "surface tunnel snapshot is read-only");

  SurfaceTunnelHistorySystem.Clear(component);
  Require(component.Count == 0, "surface tunnel reset count");
  Require(snapshot.Count == 49, "surface tunnel snapshot is isolated from reset");
}

static void TestSurfaceOrePatchHistory()
{
  SurfaceOrePatchHistoryComponent component =
    new(generationId: 17);

  Require(
    SurfaceOrePatchHistoryComponent.Capacity == 50 &&
    SurfaceOrePatchHistoryComponent.EffectiveEntryLimit == 49,
    "surface ore-patch capacity definitions");

  for (int index = 0; index < SurfaceOrePatchHistoryComponent.EffectiveEntryLimit; index++)
  {
    Require(
      SurfaceOrePatchHistorySystem.TryAppend(component, 3000 + index),
      "surface ore-patch position within effective capacity");
  }

  Require(
    !SurfaceOrePatchHistorySystem.TryAppend(component, 4000),
    "surface ore-patch effective capacity rejection");

  SurfaceOrePatchHistorySnapshot snapshot = component.CreateSnapshot();
  Require(snapshot.GenerationId == 17, "surface ore-patch generation");
  Require(snapshot.Capacity == 50, "surface ore-patch declared capacity");
  Require(snapshot.EffectiveEntryLimit == 49, "surface ore-patch effective limit");
  Require(snapshot.Count == 49, "surface ore-patch count");
  Require(snapshot.PatchXPositions[0] == 3000, "surface ore-patch first position");
  Require(snapshot.PatchXPositions[48] == 3048, "surface ore-patch last position");
  Require(
    ((IList<int>)snapshot.PatchXPositions).IsReadOnly,
    "surface ore-patch snapshot is read-only");

  SurfaceOrePatchHistorySystem.Clear(component);
  Require(component.Count == 0, "surface ore-patch reset count");
  Require(snapshot.Count == 49, "surface ore-patch snapshot is isolated from reset");
}

static void TestMushroomBiomeAnchorState()
{
  MushroomBiomeAnchorStateComponent component =
    new(generationId: 17);

  Require(
    !MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
      component,
      new TilePosition(100, 200),
      patchesCommitted: false),
    "mushroom anchor must not append before patch commit");
  Require(component.Count == 0, "mushroom anchor rejected patch keeps count");

  Require(
    MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
      component,
      new TilePosition(100, 200),
      patchesCommitted: true),
    "mushroom anchor appends after patch commit");

  MushroomBiomeAnchorStateSnapshot snapshot =
    MushroomBiomeAnchorQuery.Snapshot(component);
  Require(snapshot.GenerationId == 17, "mushroom anchor generation");
  Require(
    snapshot.Capacity == MushroomBiomeCapacityDefinition.Capacity,
    "mushroom anchor capacity");
  Require(snapshot.Count == 1, "mushroom anchor count");
  Require(
    snapshot.Positions[0] == new TilePosition(100, 200),
    "mushroom anchor insertion order");
  Require(
    ((IList<TilePosition>)snapshot.Positions).IsReadOnly,
    "mushroom anchor snapshot is read-only");
  Require(
    MushroomBiomeAnchorQuery.HasAnchorWithinDistance(
      snapshot,
      new TilePosition(103, 203),
      distance: 5),
    "mushroom anchor strict interior distance");
  Require(
    !MushroomBiomeAnchorQuery.HasAnchorWithinDistance(
      snapshot,
      new TilePosition(103, 204),
      distance: 5),
    "mushroom anchor distance boundary is excluded");

  for (int index = 1; index < MushroomBiomeCapacityDefinition.Capacity; index++)
  {
    Require(
      MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
        component,
        new TilePosition(100 + index, 200 + index),
        patchesCommitted: true),
      "mushroom anchor within capacity");
  }

  Require(
    !MushroomBiomeGenerationSystem.TryAppendAfterSuccessfulPatchCommit(
      component,
      new TilePosition(999, 999),
      patchesCommitted: true),
    "mushroom anchor capacity rejection");
  MushroomBiomeGenerationSystem.Clear(component);
  Require(component.Count == 0, "mushroom anchor reset count");
  Require(snapshot.Count == 1, "mushroom anchor snapshot is isolated from reset");
}

static void TestFallenLogFlowerHandoff()
{
  FallenLogFlowerHandoffComponent component =
    new(generationId: 17);
  FallenLogFlowerHandoffSnapshot initial =
    FallenLogFlowerHandoffQuery.Snapshot(component);
  Require(initial.GenerationId == 17, "fallen-log handoff generation");
  Require(initial.LogX == -1 && initial.LogY == -1 && !initial.HasPendingLog,
    "fallen-log handoff reset sentinel");

  Require(
    !FallenLogFlowerHandoffSystem.TryPublish(
      component,
      new TilePosition(10, 20),
      placementSucceeded: false,
      randomSelectionAccepted: true),
    "failed fallen-log placement must not publish");
  Require(
    !FallenLogFlowerHandoffQuery.Snapshot(component).HasPendingLog,
    "failed fallen-log placement preserves empty handoff");

  Require(
    FallenLogFlowerHandoffSystem.TryPublish(
      component,
      new TilePosition(10, 20),
      placementSucceeded: true,
      randomSelectionAccepted: true),
    "successful fallen-log placement publishes");
  Require(
    FallenLogFlowerHandoffSystem.TryPublish(
      component,
      new TilePosition(30, 40),
      placementSucceeded: true,
      randomSelectionAccepted: true),
    "later fallen-log placement overwrites the pending pair");

  FallenLogFlowerHandoffSnapshot published =
    FallenLogFlowerHandoffQuery.Snapshot(component);
  Require(published.LogX == 30 && published.LogY == 40 && published.HasPendingLog,
    "fallen-log handoff retains the latest selected pair");

  Require(
    FallenLogFlowerHandoffSystem.TryConsume(component, out TilePosition consumed),
    "fallen-log handoff consumes once");
  Require(consumed == new TilePosition(30, 40), "fallen-log handoff consumes latest pair");

  FallenLogFlowerHandoffSnapshot consumedSnapshot =
    FallenLogFlowerHandoffQuery.Snapshot(component);
  Require(
    consumedSnapshot.LogX == -1 && consumedSnapshot.LogY == 40 &&
    !consumedSnapshot.HasPendingLog,
    "fallen-log consume clears logX and preserves stale logY");
  Require(
    !FallenLogFlowerHandoffSystem.TryConsume(component, out _),
    "fallen-log handoff is one-shot");

  FallenLogFlowerHandoffSystem.TryPublish(
    component,
    new TilePosition(50, 60),
    placementSucceeded: true,
    randomSelectionAccepted: true);
  FallenLogFlowerHandoffSystem.Reset(component);
  FallenLogFlowerHandoffSnapshot reset =
    FallenLogFlowerHandoffQuery.Snapshot(component);
  Require(reset.LogX == -1 && reset.LogY == -1 && !reset.HasPendingLog,
    "fallen-log handoff reset clears both coordinates");
}

sealed class TestContinuation :
  WorldGenerationTileScanAndControlActionsCommand.IContinuation
{
  public bool Continue(TilePosition position)
  {
    return position.X >= 0;
  }
}

sealed class TestCountSink :
  WorldGenerationTileScanAndControlActionsCommand.ICountResultSink
{
  public int Total { get; private set; }

  public void Add(int value)
  {
    Total += value;
  }
}

sealed class TestPerUnitAction :
  WorldGenerationTileScanAndControlActionsCommand.IPerUnitAction
{
  public bool Apply(TilePosition position)
  {
    return position.X >= 0;
  }
}

sealed class TestBoundsReference :
  WorldGenerationTileScanAndControlActionsCommand.IBoundsReference
{
}

sealed class TestDiagnosticSink :
  WorldGenerationTileFramingAndDebugActionsCommand.IDiagnosticSink
{
  public void Publish(
    TilePosition target,
    ColorRgba color)
  {
  }
}
