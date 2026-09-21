using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Biomes;
using Terraria.WorldGeneration.Commands;
using Terraria.WorldGeneration.Dungeon.Bounds;
using Terraria.WorldGeneration.Dungeon.Catalogs;
using Terraria.WorldGeneration.Dungeon.Generation;
using Terraria.WorldGeneration.Dungeon.Halls;
using Terraria.WorldGeneration.Dungeon.Layout;
using Terraria.WorldGeneration.Dungeon.Placement;
using Terraria.WorldGeneration.Dungeon.Queries;
using Terraria.WorldGeneration.Dungeon.Rooms;
using Terraria.WorldGeneration.Dungeon.Styles;
using Terraria.WorldGeneration.Host;
using Terraria.WorldGeneration.Projections;
using Terraria.WorldGeneration.Support;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

var tests = new (string Name, Action Test)[]
{
  ("bounds clamp and derive", TestBoundsClampAndDerivedValues),
  ("bounds query is read-only", TestBoundsQueryDoesNotMutate),
  ("style catalog exposes immutable snapshot", TestStyleCatalogSnapshot),
  ("style selection rejects unknown styles", TestStyleSelectionRejectsUnknownStyle),
  ("dungeon catalogs preserve immutable definitions", TestDungeonCatalogs),
  ("host and menu boundaries isolate external effects", TestHostAndMenuBoundaries),
  ("generation support keeps bounded transient work", TestGenerationSupportWork),
  ("biome structure preparation is pure until command commit", TestBiomeStructurePreparation),
  ("crawler context owns transient iteration", TestCrawlerContextLifecycle),
  ("room and hall lifecycle is ordered", TestRoomAndHallLifecycle),
  ("layout graph records links", TestLayoutGraphRecordsLinks),
  ("control line query is a snapshot", TestControlLineSnapshot),
  ("placement command rejects protected targets", TestPlacementCommand),
  ("trap work state clears attempts", TestTrapWorkState)
};

foreach (var test in tests)
{
  test.Test();
}

Console.WriteLine("P03 world generation verification passed.");

static void TestBoundsClampAndDerivedValues()
{
  var bounds = new DungeonBoundsComponent(worldWidth: 100, worldHeight: 80);
  var mutationSystem = new DungeonBoundsMutationSystem();

  DungeonBoundsMutationResult result = mutationSystem.SetBounds(
    bounds,
    left: -5,
    right: 120,
    top: 10,
    bottom: 65);

  Require(result.Applied, "A valid clamped rectangle should be applied.");
  Require(bounds.Left == 0 && bounds.Right == 100, "Horizontal bounds should be clamped.");
  Require(bounds.Top == 10 && bounds.Bottom == 65, "Vertical bounds should be preserved.");
  Require(bounds.Width == 100 && bounds.Height == 55, "Derived dimensions should be exclusive.");
  Require(bounds.Center == new DungeonTilePoint(50, 37), "Center should be derived from bounds.");
  Require(bounds.Hitbox == new DungeonBoundsRectangle(0, 10, 100, 55), "Hitbox should match bounds.");

  DungeonBoundsMutationResult reset = mutationSystem.Reset(bounds);
  Require(reset.Applied, "Reset should apply to an initialized bounds component.");
  Require(bounds.Width == 0 && bounds.Height == 0, "Reset should clear the active rectangle.");
}

static void TestBoundsQueryDoesNotMutate()
{
  var bounds = new DungeonBoundsComponent(worldWidth: 20, worldHeight: 20);
  var mutationSystem = new DungeonBoundsMutationSystem();
  mutationSystem.SetBounds(bounds, left: 2, right: 12, top: 3, bottom: 15);
  DungeonBoundsSnapshot before = DungeonBoundsQuery.Snapshot(bounds);

  Require(DungeonBoundsQuery.Contains(bounds, new DungeonTilePoint(2, 3)), "The top-left point should be contained.");
  Require(!DungeonBoundsQuery.Contains(bounds, new DungeonTilePoint(12, 3)), "The right edge should be exclusive.");
  Require(DungeonBoundsQuery.Intersects(bounds, new DungeonBoundsRectangle(10, 10, 3, 3)), "Overlapping rectangles should intersect.");
  Require(DungeonBoundsQuery.Snapshot(bounds) == before, "A query must not mutate the bounds state.");
}

static void TestStyleCatalogSnapshot()
{
  DungeonStyleCatalog catalog = DungeonStyleSetCatalog.CreateDefault();
  DungeonStyleMaterialDefinition temple = catalog.Get(DungeonStyleId.Temple);
  DungeonStyleCatalogSnapshot snapshot = catalog.CreateSnapshot();

  Require(temple.Style == DungeonStyleId.Temple, "The default catalog should contain Temple.");
  Require(snapshot.Entries.Count == 15, "The default style catalog should contain 15 styles.");
  Require(snapshot.Entries.All(entry => entry.BrickTileType > 0), "Every default style needs a brick tile.");
  Require(!catalog.TryGet((DungeonStyleId)255, out _), "Unknown styles must not resolve.");
}

static void TestStyleSelectionRejectsUnknownStyle()
{
  DungeonStyleCatalog catalog = DungeonStyleSetCatalog.CreateDefault();
  DungeonStyleSelectionQuery query = new(catalog);

  Require(query.TrySelect(DungeonStyleId.Hallow, out DungeonStyleMaterialDefinition hallow), "Known styles should select.");
  Require(hallow.Style == DungeonStyleId.Hallow, "Selection should return the requested style.");
  Require(!query.TrySelect((DungeonStyleId)255, out _), "Unknown styles should be rejected.");
}

static void TestDungeonCatalogs()
{
  int[] doorItems = [25, 4415];
  var furniture = new DungeonFurnitureDefinition(
    style: DungeonStyleId.Cavern,
    windowPlatformItemTypes: [94, 4416],
    lockedBiomeChestType: -1,
    lockedBiomeChestStyle: -1,
    biomeChestItemType: -1,
    biomeChestLootItemType: -1,
    chestItemTypes: [306, 5886],
    doorItemTypes: doorItems,
    platformItemTypes: [94, 4416],
    chandelierItemTypes: [106, 5885],
    lanternItemTypes: [2037, 5890],
    tableItemTypes: [32, 5894],
    workbenchItemTypes: [36, 5896],
    candleItemTypes: [105, 713, 5883],
    vaseOrStatueItemTypes: [],
    bookcaseItemTypes: [354, 5881],
    chairItemTypes: [34, 5884],
    bedItemTypes: [224, 5880],
    pianoItemTypes: [333, 5891],
    dresserItemTypes: [334, 5888],
    sofaItemTypes: [2397, 5893],
    bathtubItemTypes: [336, 5879],
    lampItemTypes: [342, 5889],
    candelabraItemTypes: [349, 714, 5882],
    clockItemTypes: [359, 5887],
    bannerItemTypes: [337, 339, 338, 340, 5497, 5498],
    biomeRoomType: DungeonRoomType.BiomeStructured,
    subStyles: [DungeonStyleId.Shimmer, DungeonStyleId.Spider]);
  var furnitureCatalog = new DungeonFurnitureCatalog([furniture]);

  doorItems[0] = 9999;
  Require(
    furnitureCatalog.Get(DungeonStyleId.Cavern).DoorItemTypes.SequenceEqual([25, 4415]),
    "Furniture definitions must defensively copy item arrays.");
  Require(
    furnitureCatalog.Get(DungeonStyleId.Cavern).BiomeRoomType == DungeonRoomType.BiomeStructured,
    "Furniture definitions should preserve the room variant type.");

  DungeonObjectStyleCatalog objectStyles = DungeonObjectStyleCatalog.CreateDefault();
  var objectStyleQuery = new DungeonObjectStyleQuery(objectStyles);
  Require(
    objectStyleQuery.TrySelect(DungeonObjectStyleKey.DoorBlueBrick, out int doorStyle)
      && doorStyle == 16,
    "Object style queries should expose the verified blue-brick door style.");
  Require(
    !objectStyleQuery.TrySelect((DungeonObjectStyleKey)255, out _),
    "Unknown object styles must be rejected.");

  DungeonBannerTrapCatalog bannerTrapCatalog = DungeonBannerTrapCatalog.CreateDefault();
  Require(
    bannerTrapCatalog.BannerStyles.SequenceEqual([10, 11, 12, 13, 14, 15]),
    "Banner styles should preserve the verified catalog order.");
  Require(
    bannerTrapCatalog.TrapTypes.SequenceEqual([0]),
    "The dart trap type should be present in the trap catalog.");

  DungeonRoomVariantCatalog variants = DungeonRoomVariantCatalog.CreateDefault();
  Require(
    variants.BaseInnerSize == 32
      && variants.TempleInnerSize == 50
      && variants.WallDepth == 8,
    "Biome room geometry constants should be preserved.");
  Require(
    variants.Variants.SequenceEqual([
      DungeonRoomVariantId.DoubleDiamond,
      DungeonRoomVariantId.Rounded,
      DungeonRoomVariantId.Candy,
      DungeonRoomVariantId.Wiggled]),
    "Structured room variants should preserve their source order.");
  Require(variants.MaxVariants == 4, "The source variant count should be preserved.");
}

static void TestHostAndMenuBoundaries()
{
  var host = new WorldGenerationHostRuntime();
  host.MarkEnginePreloaded();
  host.SetMapEnabled(false);
  host.SetSkipAssemblyLoad(true);
  host.SetRenderCount(7);
  host.SetFavoriteColor(new WorldGenerationColor(1, 2, 3));
  host.SetShimmer(0.25f, 0.5f);
  host.SetAfterPartyOfDoom(true);
  host.BeginGeneration();
  host.SetProgress(0.25f);

  var graphics = new RecordingGraphicsPort();
  var clock = new RecordingClockPort(TimeSpan.FromMilliseconds(42));
  var progress = new RecordingProgressSink();
  var wind = new RecordingAmbientWindPort();
  var chumBucket = new RecordingChumBucketPort();
  var system = new WorldGenerationHostRuntimeSystem(
    graphics,
    clock,
    progress,
    wind,
    chumBucket);

  TimeSpan elapsed = system.Tick(host, "Terrain", "carving surface");
  Require(elapsed == TimeSpan.FromMilliseconds(42), "The host must read time through its clock port.");
  Require(host.GameUpdateCount == 1, "A host tick should advance the update count.");
  Require(graphics.LastSnapshot.MapEnabled == false, "Graphics must receive a presentation snapshot.");
  Require(graphics.LastSnapshot.FavoriteColor == new WorldGenerationColor(1, 2, 3), "Color must cross the graphics port as a value.");
  Require(wind.UpdateCount == 1 && chumBucket.UpdateCount == 1, "External helpers must be called through ports.");
  Require(progress.LastProgress.PassId == "Terrain" && progress.LastProgress.Fraction == 0.25f, "Progress must be projected without changing the host state.");

  AssertThrows<InvalidOperationException>(
    () => host.SetProgress(0.1f),
    "Generation progress must not move backwards.");

  var menu = new WorldGenerationMenuSession(maxMenuItems: 2);
  menu.SetMenuMode(10);
  menu.SetNewWorldName("Test World");
  menu.SetAutoPass(true);
  menu.SetMenuItemScale(0, 1f);
  IReadOnlyList<float> menuScales = menu.MenuItemScales;
  Require(menuScales.SequenceEqual([1f, 0.8f]), "Menu scale state should be readable as a snapshot.");
  AssertThrows<NotSupportedException>(
    () => ((IList<float>)menuScales)[0] = 0f,
    "Menu scale snapshots must not expose mutable storage.");
  Require(menu.MenuMode == 10 && menu.NewWorldName == "Test World" && menu.AutoPass, "Menu state should preserve generation input.");

  host.CompleteGeneration();
  Require(host.Phase == WorldGenerationHostPhase.Completed, "Completion should be an explicit host transition.");
}

static void TestGenerationSupportWork()
{
  var support = new GenerationSupportWorkState();
  support.Begin(revision: 4);
  Require(support.IsActive && support.Revision == 4, "Support work should expose its active revision.");

  PaintingDefinition painting = new(tileType: 123, style: 7);
  Require(painting.TileType == 123 && painting.Style == 7, "Painting values should preserve source fields.");

  var floodFill = new ShapeFloodFillWorkState(maximumActions: 2);
  Require(floodFill.TryConsumeAction(), "A flood fill should accept its first action.");
  Require(floodFill.TryConsumeAction(), "A flood fill should accept its final budgeted action.");
  Require(!floodFill.TryConsumeAction(), "A flood fill should reject actions after its budget.");
  Require(floodFill.RemainingActions == 0, "Flood fill remaining actions should be derived from the budget.");
  floodFill.Reset();
  Require(floodFill.ConsumedActions == 0, "Reset should clear transient flood-fill actions.");

  var track = new TrackGenerationWorkState(historyCapacity: 2, rewriteHistoryCapacity: 1);
  track.AddHistory(new TrackHistoryEntry(10, 11, TrackSlope.Up));
  track.AddHistory(new TrackHistoryEntry(12, 13, TrackSlope.Straight, TrackGenerationMode.Tunnel));
  Require(!track.TryAddHistory(new TrackHistoryEntry(14, 15, TrackSlope.Down)), "Track history should enforce its fixed capacity.");
  track.AddRewriteHistory(new TrackHistoryEntry(20, 21, TrackSlope.Down));
  IReadOnlyList<TrackHistoryEntry> history = track.History;
  Require(history.Count == 2 && history[1].Mode == TrackGenerationMode.Tunnel, "Track history should preserve entries and modes.");
  AssertThrows<NotSupportedException>(
    () => ((IList<TrackHistoryEntry>)history)[0] = history[1],
    "Track history snapshots must not expose mutable storage.");

  GenerationFeedbackPort feedback = GenerationFeedbackPort.WithText;
  var feedbackAdapter = new RoomCheckFeedbackAdapter(feedback);
  feedbackAdapter.BeginSpread(3, 4);
  feedbackAdapter.RoomTooBig(5, 6, 2);
  GenerationFeedbackSnapshot snapshot = feedback.CreateSnapshot();
  Require(snapshot.DisplayText && snapshot.StopOnFail, "Feedback presets should preserve source flags.");
  Require(snapshot.EventCount == 1 && snapshot.LastReason == GenerationFeedbackReason.RoomTooBig, "Feedback should record structured failure reasons.");
  feedbackAdapter.EndSpread();

  support.Complete();
  Require(!support.IsActive, "Completed support work should be inactive.");
  support.Clear();
  Require(support.Revision == 0, "Clearing support work should reset its transient revision.");
}

static void TestBiomeStructurePreparation()
{
  BiomeStructurePlacementDefinition definition =
    BiomeStructurePlacementDefinition.CreateDefault();
  Require(definition.BlacklistedTiles.Contains(225), "The cave-house blacklist should preserve tile 225.");
  Require(definition.BeelistedTiles.Contains(41), "The cave-house beelist should preserve tile 41.");
  Require(!definition.BeelistedTiles.Contains(225), "The beelist should remain narrower than the blacklist.");

  var budget = new CaveHouseBudgetState();
  budget.Configure(sharpenerCount: 1, extractinatorCount: 0);
  var query = new StructurePlacementEligibilityQuery();
  var system = new BiomeStructureGenerationSystem(query);

  StructurePlacementEligibilityDecision rejected = query.Evaluate(
    definition,
    new StructurePlacementProbe(
      tileType: 225,
      isWithinWorldBounds: true,
      isProtected: false,
      isStructureAvailable: true,
      requiresBeelistedTile: false));
  Require(!rejected.Accepted && rejected.Reason == "blacklisted-tile", "Blacklisted tiles must be rejected by a pure query.");

  bool created = system.TryCreateCommand(
    definition,
    budget,
    new StructurePlacementProbe(41, true, false, true, true),
    structureId: "cave-house",
    origin: new DungeonTilePoint(10, 12),
    resource: StructurePlacementResource.Sharpener,
    lootChance: 0.25,
    out StructurePlacementCommand command);
  Require(created, "An eligible beelisted structure should produce a command.");
  Require(command.StructureId == "cave-house" && command.Origin == new DungeonTilePoint(10, 12), "Commands should preserve placement intent.");
  Require(budget.SharpenerCount == 0, "Accepted structure preparation should consume exactly one budget item.");

  Require(
    !system.TryCreateCommand(
      definition,
      budget,
      new StructurePlacementProbe(41, true, false, true, true),
      "second-house",
      new DungeonTilePoint(11, 12),
      StructurePlacementResource.Sharpener,
      0.25,
      out _),
    "A depleted budget must reject a second structure command.");
  Require(budget.SharpenerCount == 0, "Rejected preparation must not underflow the budget.");
}

static void TestCrawlerContextLifecycle()
{
  DungeonStyleCatalog catalog = DungeonStyleSetCatalog.CreateDefault();
  var crawler = new DungeonCrawlerContext();
  DungeonGenerationContextComponent context = crawler.Begin(type: 1, iteration: 0);
  context.ConfigureIteration(
    dungeonEntranceId: 2,
    new DungeonBoundsRectangle(2, 2, 8, 8),
    new DungeonBoundsRectangle(0, 0, 20, 20));
  crawler.LegacyRules.BeginGeneration(DungeonStyleId.Temple, catalog, 3);
  crawler.Collections.AddFeature(7);

  Require(ReferenceEquals(crawler.RequireCurrent(), context), "The crawler should expose its sole active context.");
  crawler.End();
  Require(crawler.CurrentDungeonData is null, "Ending an iteration should release its context.");
  Require(crawler.Collections.FeatureIds.Count == 0, "Ending an iteration should clear transient collections.");
}

static void TestRoomAndHallLifecycle()
{
  DungeonStyleMaterialDefinition style = DungeonStyleSetCatalog.CreateDefault().Get(DungeonStyleId.Cavern);
  var room = new DungeonRoomLifecycleComponent();
  AssertThrows<InvalidOperationException>(
    room.MarkGenerated,
    "A room must not generate before calculation.");
  room.MarkCalculated();
  room.MarkGenerated();
  Require(room.Processed, "A generated room should be processed.");

  var hall = new DungeonHallLifecycleComponent();
  AssertThrows<InvalidOperationException>(
    hall.MarkGenerated,
    "A hall must not generate before calculation.");
  hall.MarkCalculated();
  hall.MarkGenerated();
  Require(hall.Processed, "A generated hall should be processed.");
  _ = new DungeonRoomDefinition(1, 0, 1, style);
  _ = new DungeonHallDefinition(1, 0, 1, style);
}

static void TestLayoutGraphRecordsLinks()
{
  DungeonStyleMaterialDefinition style = DungeonStyleSetCatalog.CreateDefault().Get(DungeonStyleId.Cavern);
  var graph = new DungeonLayoutGraphWorkState();
  var system = new DungeonLayoutSystem();
  system.AddRoom(graph, 1, new DungeonRoomDefinition(1, 0, 1, style), 0f);
  system.AddRoom(graph, 2, new DungeonRoomDefinition(2, 0, 2, style), 1f);
  system.ConnectRooms(
    graph,
    lineId: 10,
    sourceEntryId: 1,
    targetEntryId: 2,
    sourcePoint: new DungeonTilePoint(2, 2),
    targetPoint: new DungeonTilePoint(8, 8));

  Require(graph.Halls.Count == 1, "The layout graph should contain the new hall line.");
  Require(graph.Rooms[1].ForwardLinks.SequenceEqual([2]), "Source room should record its forward link.");
  Require(graph.Rooms[2].BackLinks.SequenceEqual([1]), "Target room should record its back link.");
}

static void TestControlLineSnapshot()
{
  var system = new DungeonControlLineSystem();
  DungeonControlLineComponent line = system.Create(
    index: 4,
    start: new DungeonTilePoint(2, 4),
    end: new DungeonTilePoint(12, 14),
    style: DungeonStyleId.Jungle,
    progressionStage: 3,
    curveLine: true);
  DungeonControlLineSnapshot snapshot = DungeonControlLineGeometryQuery.Snapshot(line);

  Require(snapshot.Index == 4, "Control line snapshot should preserve identity.");
  Require(snapshot.Center == new DungeonTilePoint(7, 9), "Control line center should be derived.");
  Require(snapshot.CurveLine, "Control line snapshot should preserve curve state.");
}

static void TestPlacementCommand()
{
  var system = new DungeonPlacementCommandSystem();
  var request = new DungeonPlatformPlacementRequest(new DungeonTilePoint(4, 4))
  {
    PlaceBooksChance = 1f,
    CanPlaceHereCallback = snapshot => !snapshot.Occupied
  };
  DungeonPlacementSnapshot protectedSnapshot = new(
    request.Position,
    OccupancyRevision: 1,
    Occupied: false,
    InProtectedBounds: true);
  DungeonPlacementDecision rejected = system.ValidatePlatform(request, protectedSnapshot);
  Require(!rejected.Accepted, "Protected placement should be rejected by the pure query.");
  Require(request.IsAShelf, "A platform with book chance should be classified as a shelf.");
}

static void TestTrapWorkState()
{
  var state = new DungeonTrapPlacementWorkState();
  state.Configure(numberOfDartTraps: 2, numberOfBoulderTraps: 1, stepsBetweenBoulderTraps: 5);
  state.AddDartTrap(new DartTrapPlacementAttempt(1, 0, 3, 4, new DungeonTilePoint(3, 4), 0.5f));
  state.AddBoulder(new BoulderPlacementAttempt(new DungeonTilePoint(5, 6), 1, 2, 3));
  state.SetExplosive(new ExplosivePlacementAttempt(new DungeonTilePoint(7, 8)));
  Require(state.DartTraps.Count == 1 && state.Boulders.Count == 1, "Trap attempts should be retained for the pass.");
  state.ClearAttempts();
  Require(state.DartTraps.Count == 0 && state.Boulders.Count == 0 && state.Explosive is null, "Failed or completed attempts should be clearable.");
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

static void Require(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

sealed class RecordingGraphicsPort : IGraphicsGenerationPort
{
  public WorldGenerationGraphicsSnapshot LastSnapshot { get; private set; }

  public int PresentCount { get; private set; }

  public void Present(WorldGenerationGraphicsSnapshot snapshot)
  {
    LastSnapshot = snapshot;
    PresentCount++;
  }
}

sealed class RecordingClockPort : IGenerationClockPort
{
  private readonly TimeSpan _elapsed;

  public RecordingClockPort(TimeSpan elapsed)
  {
    _elapsed = elapsed;
  }

  public int ReadCount { get; private set; }

  public TimeSpan ReadElapsedTime()
  {
    ReadCount++;
    return _elapsed;
  }
}

sealed class RecordingProgressSink : IProgressSink
{
  public WorldGenerationProgressProjection LastProgress { get; private set; }

  public int PublishCount { get; private set; }

  public void Publish(WorldGenerationProgressProjection progress)
  {
    LastProgress = progress;
    PublishCount++;
  }
}

sealed class RecordingAmbientWindPort : IAmbientWindGenerationPort
{
  public int UpdateCount { get; private set; }

  public void Update()
  {
    UpdateCount++;
  }
}

sealed class RecordingChumBucketPort : IChumBucketProjectilePort
{
  public int UpdateCount { get; private set; }

  public void Update()
  {
    UpdateCount++;
  }
}
