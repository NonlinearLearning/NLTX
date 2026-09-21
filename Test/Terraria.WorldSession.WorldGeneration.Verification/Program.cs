using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Passes;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static class Program
{
  private static int Main()
  {
    try
    {
      TestConfigurationIsImmutable();
      TestOreSelectionOwnerCommitsOneSnapshot();
      TestStructureReservationsAreBoundedAndCopied();
      TestLayerMetricsStoresAllMembersAndCopiesArrays();
      TestLayerMetricsRejectsUnpairedSnowColumns();
      TestLayerMetricsSnapshotIsReadOnly();
      TestLayerMetricsQueryReturnsCommittedSnapshot();
      TestLayerMetricsCalculatorCommitsVersion4Metrics();
      TestLayerMetricsCalculatorHonorsInitialBeachPadding();
      TestWorldSpawnAndLandmassStoresAllMembersAndCopiesList();
      TestWorldSpawnAndLandmassRejectsStaleCommit();
      TestSurfaceMaterialDefinitionPreservesVersion4Defaults();
      TestWorldGenerationLiquidBoundaryCommitsBothLines();
      TestBeachBoundaryStoresAllMembersAndRejectsStaleCommit();
      TestOceanBiomeStateCommitsAllConstraintsAndTreasureState();
      TestUndergroundDesertStateCommitsLayoutAndLarvaState();
      TestPyramidPlacementCommitBoundary();
      TestJungleChestAndLootCommitBoundary();
      TestDungeonAndFloatingIslandStateBoundaries();
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestConfigurationIsImmutable()
  {
    WorldGenerationConfigurationDefinition definition =
      WorldGenerationConfigurationDefinition.Create(
        worldWidth: 4200,
        worldHeight: 1200,
        seedText: "p17",
        rulesVersion: 1);

    WorldGenerationConfigurationSnapshot snapshot = definition.CreateSnapshot();
    Assert(snapshot.WorldWidth == 4200, "configuration width");
    Assert(snapshot.WorldHeight == 1200, "configuration height");
    Assert(snapshot.SeedText == "p17", "configuration seed");
  }

  private static void TestOreSelectionOwnerCommitsOneSnapshot()
  {
    WorldGenerationOreSelectionComponent component =
      new(generationId: 1);
    WorldGenerationOreSelectionSnapshot selection = new(
      1,
      7,
      6,
      9,
      8,
      20,
      22,
      21,
      19);

    OreTierSelectionSystem.Commit(component, selection);

    Assert(component.CreateSnapshot() == selection, "ore selection snapshot");
  }

  private static void TestStructureReservationsAreBoundedAndCopied()
  {
    InMemoryStructureReservationAdapter adapter = new();
    StructureReservationIntent intent = new(
      GenerationId: 1,
      ReservationId: "desert",
      Bounds: new WorldGenerationRectangle(10, 20, 30, 40));

    ReservationResult first = adapter.Reserve(intent);
    ReservationResult second = adapter.Reserve(
      intent with { ReservationId = "overlap" });

    Assert(first.Accepted, "first reservation");
    Assert(!second.Accepted, "overlapping reservation rejection");
    Assert(adapter.CreateSnapshot().Count == 1, "reservation snapshot count");
  }

  private static void TestLayerMetricsStoresAllMembersAndCopiesArrays()
  {
    int[] minimums = [10, 20, 30];
    int[] maximums = [11, 21, 31];
    WorldLayerMetricsComponent component = new(
      generationId: 2,
      lowestCloud: -1,
      worldSurfaceLow: 100.25d,
      worldSurface: 120.5d,
      worldSurfaceHigh: 140.75d,
      rockLayerLow: 300.25d,
      rockLayer: 320.5d,
      rockLayerHigh: 340.75d,
      snowTop: 400,
      snowBottom: 500,
      snowOriginLeft: 600,
      snowOriginRight: 700,
      snowMinX: minimums,
      snowMaxX: maximums);

    minimums[0] = -100;
    maximums[0] = -101;
    WorldLayerMetricsSnapshot snapshot = component.CreateSnapshot();

    Assert(snapshot.GenerationId == 2, "layer metrics generation");
    Assert(snapshot.LowestCloud == -1, "layer metrics lowest cloud");
    Assert(snapshot.WorldSurfaceLow == 100.25d, "layer metrics surface low");
    Assert(snapshot.WorldSurface == 120.5d, "layer metrics surface");
    Assert(snapshot.WorldSurfaceHigh == 140.75d, "layer metrics surface high");
    Assert(snapshot.RockLayerLow == 300.25d, "layer metrics rock low");
    Assert(snapshot.RockLayer == 320.5d, "layer metrics rock");
    Assert(snapshot.RockLayerHigh == 340.75d, "layer metrics rock high");
    Assert(snapshot.SnowTop == 400, "layer metrics snow top");
    Assert(snapshot.SnowBottom == 500, "layer metrics snow bottom");
    Assert(snapshot.SnowOriginLeft == 600, "layer metrics snow origin left");
    Assert(snapshot.SnowOriginRight == 700, "layer metrics snow origin right");
    Assert(snapshot.SnowMinX.Count == 3, "layer metrics snow minimum count");
    Assert(snapshot.SnowMaxX.Count == 3, "layer metrics snow maximum count");
    Assert(snapshot.SnowMinX[0] == 10, "layer metrics snow minimum copy");
    Assert(snapshot.SnowMaxX[0] == 11, "layer metrics snow maximum copy");
  }

  private static void TestLayerMetricsQueryReturnsCommittedSnapshot()
  {
    WorldLayerMetricsComponent component = new(generationId: 3);
    WorldLayerMetricsSnapshot expected = new(
      3,
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
      new[] { 10, 20 },
      new[] { 11, 21 });

    WorldLayerMetricsSystem.Commit(component, expected);
    WorldLayerMetricsSnapshot actual = WorldLayerMetricsQuery.Snapshot(component);

    Assert(actual.GenerationId == expected.GenerationId, "layer metrics query generation");
    Assert(actual.WorldSurface == expected.WorldSurface, "layer metrics query surface");
    Assert(actual.SnowMinX[1] == 20, "layer metrics query minimum");
    Assert(actual.SnowMaxX[1] == 21, "layer metrics query maximum");
  }

  private static void TestLayerMetricsRejectsUnpairedSnowColumns()
  {
    bool constructorRejected = false;
    try
    {
      _ = new WorldLayerMetricsComponent(
        generationId: 5,
        snowMinX: new[] { 10 },
        snowMaxX: Array.Empty<int>());
    }
    catch (ArgumentException)
    {
      constructorRejected = true;
    }

    Assert(constructorRejected, "layer metrics constructor paired arrays");

    WorldLayerMetricsComponent component = new(
      generationId: 5,
      snowMinX: new[] { 10 },
      snowMaxX: new[] { 11 });
    WorldLayerMetricsSnapshot before = component.CreateSnapshot();
    bool commitRejected = false;
    try
    {
      WorldLayerMetricsSystem.Commit(
        component,
        new WorldLayerMetricsSnapshot(
          5,
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
          new[] { 20 },
          Array.Empty<int>()));
    }
    catch (ArgumentException)
    {
      commitRejected = true;
    }

    Assert(commitRejected, "layer metrics commit paired arrays");
    WorldLayerMetricsSnapshot after = component.CreateSnapshot();
    Assert(after.GenerationId == before.GenerationId, "layer metrics rejected generation");
    Assert(after.LowestCloud == before.LowestCloud, "layer metrics rejected lowest cloud");
    Assert(after.WorldSurface == before.WorldSurface, "layer metrics rejected surface");
    Assert(after.RockLayer == before.RockLayer, "layer metrics rejected rock layer");
    Assert(after.SnowTop == before.SnowTop, "layer metrics rejected snow top");
    Assert(after.SnowBottom == before.SnowBottom, "layer metrics rejected snow bottom");
    Assert(after.SnowMinX.Count == before.SnowMinX.Count, "layer metrics rejected minimum count");
    Assert(after.SnowMaxX.Count == before.SnowMaxX.Count, "layer metrics rejected maximum count");
    Assert(after.SnowMinX[0] == before.SnowMinX[0], "layer metrics rejected minimum value");
    Assert(after.SnowMaxX[0] == before.SnowMaxX[0], "layer metrics rejected maximum value");
  }

  private static void TestLayerMetricsSnapshotIsReadOnly()
  {
    WorldLayerMetricsComponent component = new(
      generationId: 6,
      snowMinX: new[] { 10 },
      snowMaxX: new[] { 11 });
    WorldLayerMetricsSnapshot snapshot = component.CreateSnapshot();
    IList<int> minimums = (IList<int>)snapshot.SnowMinX;

    Assert(minimums.IsReadOnly, "layer metrics snapshot read-only minimums");
    bool mutationRejected = false;
    try
    {
      minimums[0] = 99;
    }
    catch (NotSupportedException)
    {
      mutationRejected = true;
    }

    Assert(mutationRejected, "layer metrics snapshot mutation rejection");
    Assert(component.SnowMinX[0] == 10, "layer metrics snapshot cannot mutate component");
  }

  private static void TestLayerMetricsCalculatorCommitsVersion4Metrics()
  {
    WorldGenerationConfigurationDefinition configuration =
      WorldGenerationConfigurationDefinition.Create(
        worldWidth: 4200,
        worldHeight: 1200,
        seedText: "layer-metrics",
        rulesVersion: 1);
    WorldLayerMetricsCalculationInput input = new(
      generationId: 11,
      configuration,
      leftBeachEnd: 0,
      rightBeachStart: 4200,
      flatBeachPadding: 0,
      isRemixWorld: false,
      isDrunkWorld: false,
      isGoodWorld: false,
      isNoSurfaceWorld: false,
      isSurfaceInSpace: false);
    DeterministicGenerationRandomSource random =
      new();
    WorldLayerMetricsComponent component = new(generationId: 11);

    WorldLayerMetricsSnapshot actual =
      WorldLayerMetricsSystem.CalculateAndCommit(component, input, random);

    Assert(actual.GenerationId == 11, "layer metrics calculator generation");
    Assert(actual.WorldSurfaceLow == 180d, "layer metrics calculator surface low");
    Assert(actual.WorldSurface == 228d, "layer metrics calculator surface");
    Assert(actual.WorldSurfaceHigh == 228d, "layer metrics calculator surface high");
    Assert(actual.RockLayerLow == 420d, "layer metrics calculator rock low");
    Assert(actual.RockLayer == 420d, "layer metrics calculator rock");
    Assert(actual.RockLayerHigh == 420d, "layer metrics calculator rock high");
    Assert(actual.LowestCloud == -1, "layer metrics calculator cloud sentinel");
    Assert(actual.SnowMinX.Count == 1200, "layer metrics calculator snow minimum capacity");
    Assert(actual.SnowMaxX.Count == 1200, "layer metrics calculator snow maximum capacity");
    Assert(actual.SnowMinX[1199] == 0, "layer metrics calculator snow minimum default");
    Assert(actual.SnowMaxX[1199] == 0, "layer metrics calculator snow maximum default");
    WorldLayerMetricsSnapshot committed = component.CreateSnapshot();
    Assert(committed.GenerationId == actual.GenerationId, "layer metrics calculator commit generation");
    Assert(committed.WorldSurface == actual.WorldSurface, "layer metrics calculator commit surface");
    Assert(committed.RockLayer == actual.RockLayer, "layer metrics calculator commit rock");
    Assert(committed.SnowMinX.Count == actual.SnowMinX.Count, "layer metrics calculator commit snow");
    Assert(random.CallCount > 0, "layer metrics calculator random input");
  }

  private static void TestLayerMetricsCalculatorHonorsInitialBeachPadding()
  {
    WorldGenerationConfigurationDefinition configuration =
      WorldGenerationConfigurationDefinition.Create(
        worldWidth: 20,
        worldHeight: 100,
        seedText: "layer-metrics-padding",
        rulesVersion: 1);
    WorldLayerMetricsCalculationInput input = new(
      generationId: 12,
      configuration,
      leftBeachEnd: 5,
      rightBeachStart: 15,
      flatBeachPadding: 2,
      isRemixWorld: false,
      isDrunkWorld: false,
      isGoodWorld: false,
      isNoSurfaceWorld: false,
      isSurfaceInSpace: false);
    InitialBeachPaddingRandomSource random = new();
    WorldLayerMetricsComponent component = new(generationId: 12);

    _ = WorldLayerMetricsSystem.CalculateAndCommit(component, input, random);

    Assert(
      random.FirstFeatureSelectionCall == 17,
      "layer metrics calculator must preserve the initial beach padding before feature selection");
  }

  private static void TestWorldSpawnAndLandmassStoresAllMembersAndCopiesList()
  {
    List<WorldGenerationLandmassValue> landmass =
      [new WorldGenerationLandmassValue(3, 12.5d, 40.5d, 8, 2)];
    WorldSpawnAndLandmassComponent component = new(
      generationId: 7,
      worldSpawnHasBeenRandomized: true,
      landmassData: landmass,
      remixSurfaceLayerLow: 100,
      remixSurfaceLayerHigh: 200,
      remixMushroomLayerLow: 300,
      remixMushroomLayerHigh: 400,
      boulderPetsPlaced: 5);

    landmass[0] = new WorldGenerationLandmassValue(99, 1d, 2d, 3, 4);
    WorldSpawnAndLandmassSnapshot snapshot = component.CreateSnapshot();

    Assert(snapshot.GenerationId == 7, "spawn and landmass generation");
    Assert(snapshot.WorldSpawnHasBeenRandomized, "spawn randomized");
    Assert(snapshot.RemixSurfaceLayerLow == 100, "remix surface low");
    Assert(snapshot.RemixSurfaceLayerHigh == 200, "remix surface high");
    Assert(snapshot.RemixMushroomLayerLow == 300, "remix mushroom low");
    Assert(snapshot.RemixMushroomLayerHigh == 400, "remix mushroom high");
    Assert(snapshot.BoulderPetsPlaced == 5, "boulder pets placed");
    Assert(snapshot.LandmassData.Count == 1, "landmass count");
    Assert(snapshot.LandmassData[0].TypeId == 3, "landmass input copy");
    Assert(snapshot.LandmassData[0].TopY == 32.5d, "landmass top calculation");

    IList<WorldGenerationLandmassValue> snapshotLandmass =
      (IList<WorldGenerationLandmassValue>)snapshot.LandmassData;
    Assert(snapshotLandmass.IsReadOnly, "landmass snapshot read-only");
  }

  private static void TestWorldSpawnAndLandmassRejectsStaleCommit()
  {
    WorldSpawnAndLandmassComponent component = new(generationId: 8);
    WorldSpawnAndLandmassSnapshot before = component.CreateSnapshot();
    bool rejected = false;
    try
    {
      WorldSpawnAndLandmassSystem.Commit(
        component,
        new WorldSpawnAndLandmassSnapshot(
          9,
          true,
          new[] { new WorldGenerationLandmassValue(1, 1d, 1d, 1, 1) },
          1,
          2,
          3,
          4,
          5));
    }
    catch (ArgumentException)
    {
      rejected = true;
    }

    Assert(rejected, "spawn and landmass stale commit");
    WorldSpawnAndLandmassSnapshot after = component.CreateSnapshot();
    Assert(after.GenerationId == before.GenerationId, "spawn and landmass stale generation");
    Assert(!after.WorldSpawnHasBeenRandomized, "spawn and landmass stale state");
    Assert(after.LandmassData.Count == 0, "spawn and landmass stale list");
  }

  private static void TestSurfaceMaterialDefinitionPreservesVersion4Defaults()
  {
    SurfaceMaterialDefinition definition = SurfaceMaterialDefinition.Version4;

    Assert(definition.CrimsonStoneWall == 83, "surface material crimson wall");
    Assert(definition.CrimsonStone == 203, "surface material crimson stone");
    Assert(definition.EbonStoneWall == 3, "surface material ebon wall");
    Assert(definition.EbonStone == 25, "surface material ebon stone");
    Assert(definition.MossTile == 179, "surface material moss tile");
    Assert(definition.MossWall == 54, "surface material moss wall");
  }

  private static void TestWorldGenerationLiquidBoundaryCommitsBothLines()
  {
    WorldGenerationLiquidBoundaryComponent component = new(generationId: 10);
    WorldGenerationLiquidBoundarySnapshot expected = new(
      10,
      LavaLine: 900,
      WaterLine: 450);
    WorldGenerationLiquidBoundarySystem.Commit(component, expected);
    WorldGenerationLiquidBoundarySnapshot actual = component.CreateSnapshot();

    Assert(actual.GenerationId == 10, "liquid boundary generation");
    Assert(actual.LavaLine == 900, "liquid boundary lava line");
    Assert(actual.WaterLine == 450, "liquid boundary water line");

    bool rejected = false;
    try
    {
      WorldGenerationLiquidBoundarySystem.Commit(
        component,
        new WorldGenerationLiquidBoundarySnapshot(11, 1, 2));
    }
    catch (ArgumentException)
    {
      rejected = true;
    }

    Assert(rejected, "liquid boundary stale commit");
    Assert(component.LavaLine == 900, "liquid boundary stale lava");
    Assert(component.WaterLine == 450, "liquid boundary stale water");
  }

  private static void TestBeachBoundaryStoresAllMembersAndRejectsStaleCommit()
  {
    BeachBoundaryComponent component = new(generationId: 12);
    BeachBoundarySnapshot expected = new(
      12,
      100,
      3900,
      80,
      24,
      12,
      6,
      8,
      120,
      600,
      4080,
      610,
      45);

    BeachBoundarySystem.Commit(component, expected);
    BeachBoundarySnapshot actual = BeachBoundaryQuery.Snapshot(component);

    Assert(actual == expected, "beach boundary query snapshot");
    Assert(actual.LeftBeachEnd == 100, "beach left boundary");
    Assert(actual.RightBeachStart == 3900, "beach right boundary");
    Assert(actual.BeachBordersWidth == 80, "beach border width");
    Assert(actual.BeachSandRandomCenter == 24, "beach random center");
    Assert(actual.BeachSandRandomWidthRange == 12, "beach random width range");
    Assert(actual.BeachSandDungeonExtraWidth == 6, "beach dungeon extra width");
    Assert(actual.BeachSandJungleExtraWidth == 8, "beach jungle extra width");
    Assert(actual.ShellStartXLeft == 120, "beach left shell x");
    Assert(actual.ShellStartYLeft == 600, "beach left shell y");
    Assert(actual.ShellStartXRight == 4080, "beach right shell x");
    Assert(actual.ShellStartYRight == 610, "beach right shell y");
    Assert(actual.OceanWaterStartRandomMin == 45, "beach ocean water minimum");

    bool staleRejected = false;
    try
    {
      BeachBoundarySystem.Commit(
        component,
        expected with
        {
          GenerationId = 13,
          LeftBeachEnd = 1,
        });
    }
    catch (ArgumentException)
    {
      staleRejected = true;
    }

    Assert(staleRejected, "beach boundary stale commit");
    Assert(BeachBoundaryQuery.Snapshot(component) == expected,
      "beach boundary stale commit preserves state");
  }

  private static void TestOceanBiomeStateCommitsAllConstraintsAndTreasureState()
  {
    OceanBiomeConstraintComponent constraintComponent =
      new(generationId: 14);
    OceanBiomeConstraintSnapshot expectedConstraints = new(
      14,
      101,
      102,
      103,
      104,
      105,
      106,
      107,
      108);

    OceanBiomeConstraintSystem.Commit(constraintComponent, expectedConstraints);
    OceanBiomeConstraintSnapshot actualConstraints =
      OceanBiomeConstraintQuery.Snapshot(constraintComponent);

    Assert(actualConstraints.OceanWaterStartRandomMax == 101,
      "ocean constraint water start maximum");
    Assert(actualConstraints.OceanWaterForcedJungleLength == 102,
      "ocean constraint forced jungle length");
    Assert(actualConstraints.EvilBiomeBeachAvoidance == 103,
      "ocean constraint evil beach avoidance");
    Assert(actualConstraints.EvilBiomeAvoidanceMidFixer == 104,
      "ocean constraint evil middle fixer");
    Assert(actualConstraints.LakesBeachAvoidance == 105,
      "ocean constraint lake avoidance");
    Assert(actualConstraints.SmallHolesBeachAvoidance == 106,
      "ocean constraint small-hole avoidance");
    Assert(actualConstraints.SurfaceCavesBeachAvoidance == 107,
      "ocean constraint surface-cave avoidance");
    Assert(actualConstraints.SurfaceCavesBeachAvoidance2 == 108,
      "ocean constraint second surface-cave avoidance");

    bool staleRejected = false;
    try
    {
      OceanBiomeConstraintSystem.Commit(
        constraintComponent,
        expectedConstraints with { GenerationId = 15 });
    }
    catch (ArgumentException)
    {
      staleRejected = true;
    }

    Assert(staleRejected, "ocean constraint stale commit");
    Assert(OceanBiomeConstraintQuery.Snapshot(constraintComponent) == expectedConstraints,
      "ocean constraint stale commit preserves state");

    OceanCaveTreasureStateComponent treasureComponent =
      new(generationId: 14);
    TilePosition first = new(200, 300);
    TilePosition second = new(201, 301);
    TilePosition overflow = new(202, 302);

    Assert(OceanCaveTreasureSystem.TryAppend(treasureComponent, first),
      "ocean treasure first append");
    Assert(OceanCaveTreasureSystem.TryAppend(treasureComponent, second),
      "ocean treasure second append");
    Assert(!OceanCaveTreasureSystem.TryAppend(treasureComponent, overflow),
      "ocean treasure capacity rejection");

    OceanCaveTreasureSnapshot treasureSnapshot =
      OceanCaveTreasureQuery.Snapshot(treasureComponent);
    Assert(treasureSnapshot.GenerationId == 14, "ocean treasure generation");
    Assert(treasureSnapshot.Count == OceanCaveTreasureStateComponent.Capacity,
      "ocean treasure count");
    Assert(treasureSnapshot.Positions[0] == first, "ocean treasure first position");
    Assert(treasureSnapshot.Positions[1] == second, "ocean treasure second position");
    Assert(((IList<TilePosition>)treasureSnapshot.Positions).IsReadOnly,
      "ocean treasure snapshot read-only");

    OceanCaveTreasureSystem.Clear(treasureComponent);
    Assert(treasureComponent.Count == 0, "ocean treasure clear");
    Assert(treasureSnapshot.Count == OceanCaveTreasureStateComponent.Capacity,
      "ocean treasure snapshot isolation");
  }

  private static void TestUndergroundDesertStateCommitsLayoutAndLarvaState()
  {
    UndergroundDesertStructureComponent structureComponent =
      new(generationId: 16);
    UndergroundDesertStructureSnapshot expectedLayout = new(
      16,
      new UndergroundDesertRectangle(10, 20, 30, 40),
      new UndergroundDesertRectangle(15, 25, 10, 20),
      300,
      100,
      12,
      48);

    UndergroundDesertStructureSystem.Commit(structureComponent, expectedLayout);
    Assert(
      UndergroundDesertStructureQuery.Snapshot(structureComponent) == expectedLayout,
      "underground desert layout commit");

    bool staleRejected = false;
    try
    {
      UndergroundDesertStructureSystem.Commit(
        structureComponent,
        expectedLayout with { GenerationId = 17 });
    }
    catch (ArgumentException)
    {
      staleRejected = true;
    }

    Assert(staleRejected, "underground desert layout stale commit");
    Assert(
      UndergroundDesertStructureQuery.Snapshot(structureComponent) == expectedLayout,
      "underground desert stale commit preserves state");

    UndergroundDesertLarvaPlacementComponent larvaComponent =
      new(generationId: 16);
    TilePosition first = new(30, 40);
    TilePosition second = new(31, 41);

    Assert(UndergroundDesertLarvaPlacementSystem.TryAppend(larvaComponent, first),
      "underground desert first larva");
    Assert(UndergroundDesertLarvaPlacementSystem.TryAppend(larvaComponent, second),
      "underground desert second larva");

    UndergroundDesertLarvaPlacementSnapshot larvaSnapshot =
      UndergroundDesertLarvaPlacementQuery.Snapshot(larvaComponent);
    Assert(larvaSnapshot.GenerationId == 16, "underground desert larva generation");
    Assert(larvaSnapshot.Count == 2, "underground desert larva count");
    Assert(larvaSnapshot.Positions[0] == first, "underground desert first larva position");
    Assert(larvaSnapshot.Positions[1] == second, "underground desert second larva position");
    Assert(((IList<TilePosition>)larvaSnapshot.Positions).IsReadOnly,
      "underground desert larva snapshot read-only");

    for (int index = larvaComponent.Count;
      index < UndergroundDesertLarvaPlacementComponent.Capacity;
      index++)
    {
      Assert(
        UndergroundDesertLarvaPlacementSystem.TryAppend(
          larvaComponent,
          new TilePosition(index, index + 100)),
        "underground desert larva append within capacity");
    }

    Assert(
      !UndergroundDesertLarvaPlacementSystem.TryAppend(
        larvaComponent,
        new TilePosition(999, 1099)),
      "underground desert larva capacity rejection");
    UndergroundDesertLarvaPlacementSystem.Clear(larvaComponent);
    Assert(larvaComponent.Count == 0, "underground desert larva clear");
    Assert(larvaSnapshot.Count == 2, "underground desert larva snapshot isolation");
  }

  private static void TestPyramidPlacementCommitBoundary()
  {
    PyramidPlacementStateComponent component =
      new(generationId: 18, capacity: 3);
    PyramidPlacementSnapshot expected = new(
      18,
      3,
      2,
      new[] { 10, 11 },
      new[] { 20, 21 });

    PyramidPlacementCommitSystem.Commit(component, expected);
    PyramidPlacementSnapshot actual = PyramidPlacementQuery.Snapshot(component);
    Assert(actual.GenerationId == expected.GenerationId,
      "pyramid placement generation");
    Assert(actual.Capacity == expected.Capacity,
      "pyramid placement capacity");
    Assert(actual.Count == expected.Count,
      "pyramid placement count");
    Assert(actual.XPositions[0] == expected.XPositions[0],
      "pyramid placement first x");
    Assert(actual.XPositions[1] == expected.XPositions[1],
      "pyramid placement second x");
    Assert(actual.YPositions[0] == expected.YPositions[0],
      "pyramid placement first y");
    Assert(actual.YPositions[1] == expected.YPositions[1],
      "pyramid placement second y");

    bool staleRejected = false;
    try
    {
      PyramidPlacementCommitSystem.Commit(
        component,
        expected with { GenerationId = 19 });
    }
    catch (ArgumentException)
    {
      staleRejected = true;
    }

    Assert(staleRejected, "pyramid placement stale commit");
    actual = PyramidPlacementQuery.Snapshot(component);
    Assert(actual.Count == expected.Count &&
      actual.XPositions[0] == expected.XPositions[0] &&
      actual.XPositions[1] == expected.XPositions[1] &&
      actual.YPositions[0] == expected.YPositions[0] &&
      actual.YPositions[1] == expected.YPositions[1],
      "pyramid placement stale commit preserves state");

    bool unpairedRejected = false;
    try
    {
      PyramidPlacementCommitSystem.Commit(
        component,
        expected with
        {
          XPositions = new[] { 10, 11, 12 },
          YPositions = new[] { 20, 21, 22 }
        });
    }
    catch (ArgumentException)
    {
      unpairedRejected = true;
    }

    Assert(unpairedRejected, "pyramid placement rejects unused coordinates");
    actual = PyramidPlacementQuery.Snapshot(component);
    Assert(actual.Count == expected.Count &&
      actual.XPositions[0] == expected.XPositions[0] &&
      actual.XPositions[1] == expected.XPositions[1] &&
      actual.YPositions[0] == expected.YPositions[0] &&
      actual.YPositions[1] == expected.YPositions[1],
      "pyramid placement rejected commit preserves state");
  }

  private static void TestJungleChestAndLootCommitBoundary()
  {
    JungleChestAndLootGenerationStateComponent component =
      new(generationId: 20);
    JungleChestAndLootGenerationSnapshot expected = new(
      20,
      7,
      true,
      JungleChestAndLootGenerationStateComponent.Capacity,
      2,
      new[] { 30, 31 },
      new[] { 40, 41 });

    JungleChestPlacementSystem.Commit(component, expected);
    JungleChestAndLootGenerationSnapshot actual =
      JungleChestAndLootGenerationQuery.Snapshot(component);
    Assert(actual.GenerationId == expected.GenerationId,
      "jungle chest generation");
    Assert(actual.JungleItemCount == expected.JungleItemCount,
      "jungle chest item cursor");
    Assert(actual.GennedLivingMahoganyWands == expected.GennedLivingMahoganyWands,
      "jungle chest unique item flag");
    Assert(actual.Capacity == expected.Capacity && actual.Count == expected.Count,
      "jungle chest capacity and count");
    Assert(actual.ChestXPositions[0] == expected.ChestXPositions[0] &&
      actual.ChestXPositions[1] == expected.ChestXPositions[1] &&
      actual.ChestYPositions[0] == expected.ChestYPositions[0] &&
      actual.ChestYPositions[1] == expected.ChestYPositions[1],
      "jungle chest paired coordinate order");

    bool staleRejected = false;
    try
    {
      JungleChestPlacementSystem.Commit(
        component,
        expected with { GenerationId = 21 });
    }
    catch (ArgumentException)
    {
      staleRejected = true;
    }

    Assert(staleRejected, "jungle chest stale commit");
    actual = JungleChestAndLootGenerationQuery.Snapshot(component);
    Assert(actual.JungleItemCount == expected.JungleItemCount &&
      actual.Count == expected.Count &&
      actual.ChestXPositions[0] == expected.ChestXPositions[0] &&
      actual.ChestYPositions[0] == expected.ChestYPositions[0],
      "jungle chest stale commit preserves state");

    bool extraCoordinatesRejected = false;
    try
    {
      JungleChestPlacementSystem.Commit(
        component,
        expected with
        {
          ChestXPositions = new[] { 30, 31, 32 },
          ChestYPositions = new[] { 40, 41, 42 }
        });
    }
    catch (ArgumentException)
    {
      extraCoordinatesRejected = true;
    }

    Assert(extraCoordinatesRejected, "jungle chest rejects unused coordinates");
  }

  private static void TestDungeonAndFloatingIslandStateBoundaries()
  {
    DungeonLayoutControlComponent dungeonComponent =
      new(generationId: 22);
    DungeonLayoutSnapshot dungeonSnapshot = new(
      22,
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      new[] { new DungeonRecordSnapshot(0), new DungeonRecordSnapshot(1) },
      1);

    DungeonLayoutControlSystem.Commit(dungeonComponent, dungeonSnapshot);
    DungeonLayoutSnapshot actualDungeon =
      DungeonLayoutQuery.Snapshot(dungeonComponent);
    Assert(actualDungeon.TLeft == 1 && actualDungeon.TRight == 2 &&
      actualDungeon.TTop == 3 && actualDungeon.TBottom == 4 &&
      actualDungeon.TRooms == 5 && actualDungeon.LAltarX == 6 &&
      actualDungeon.LAltarY == 7,
      "dungeon layout scalars");
    Assert(DungeonLayoutQuery.CurrentRecord(dungeonComponent) ==
      new DungeonRecordSnapshot(1),
      "dungeon active record");

    dungeonComponent.SelectCurrentDungeon(-1);
    Assert(dungeonComponent.CurrentDungeon == 0,
      "dungeon selection lower clamp");
    dungeonComponent.SelectCurrentDungeon(1);
    bool upperIndexRejected = false;
    dungeonComponent.SelectCurrentDungeon(2);
    try
    {
      DungeonLayoutQuery.CurrentRecord(dungeonComponent);
    }
    catch (ArgumentOutOfRangeException)
    {
      upperIndexRejected = true;
    }

    Assert(upperIndexRejected, "dungeon selection preserves upper failure");

    DungeonSpecialRewardGenerationComponent rewardComponent =
      new(generationId: 22);
    DungeonSpecialRewardGenerationSystem.Commit(
      rewardComponent,
      new DungeonSpecialRewardGenerationSnapshot(22, true, false));
    DungeonSpecialRewardGenerationSnapshot rewardSnapshot =
      DungeonSpecialRewardGenerationQuery.Snapshot(rewardComponent);
    Assert(rewardSnapshot.GeneratedShadowKey && !rewardSnapshot.GeneratedRamRune,
      "dungeon reward flags");

    FloatingIslandPlacementStateComponent islandComponent =
      new(generationId: 22);
    Assert(FloatingIslandPlacementSystem.TryAppend(
      islandComponent,
      new FloatingIslandHouseSnapshot(22, true, 100, 200, 3)),
      "floating island first house");
    Assert(FloatingIslandPlacementSystem.TryAppend(
      islandComponent,
      new FloatingIslandHouseSnapshot(22, false, 101, 201, 4)),
      "floating island second house");
    FloatingIslandPlacementSnapshot islandSnapshot =
      FloatingIslandPlacementQuery.Snapshot(islandComponent);
    Assert(islandSnapshot.Count == 2 && islandSnapshot.Houses[0].X == 100 &&
      islandSnapshot.Houses[1].Y == 201,
      "floating island paired metadata");
    Assert(((IList<FloatingIslandHouseSnapshot>)islandSnapshot.Houses).IsReadOnly,
      "floating island snapshot read-only");
    Assert(FloatingIslandPlacementSystem.TryAppend(
      islandComponent,
      new FloatingIslandHouseSnapshot(22, true, 102, 202, 5)),
      "floating island capacity remains bounded");
  }

  private static void Assert(bool condition, string name)
  {
    if (!condition)
    {
      throw new InvalidOperationException($"Assertion failed: {name}");
    }
  }

  private sealed class DeterministicGenerationRandomSource : IGenerationRandomSource
  {
    public int CallCount { get; private set; }

    public int NextInt(
      GenerationRandomStream stream,
      int minimumInclusive,
      int maximumExclusive)
    {
      CallCount++;
      if (minimumInclusive >= maximumExclusive)
      {
        return minimumInclusive;
      }

      if (stream == GenerationRandomStream.Terrain &&
          minimumInclusive == 0 && maximumExclusive == 5)
      {
        return 0;
      }

      if (minimumInclusive == 90 && maximumExclusive == 110)
      {
        return 100;
      }

      if (minimumInclusive == 5 && maximumExclusive == 40)
      {
        return 5;
      }

      if (minimumInclusive == 5 && maximumExclusive == 30)
      {
        return 5;
      }

      if (minimumInclusive == 0 && maximumExclusive == 7)
      {
        return 1;
      }

      return minimumInclusive + (maximumExclusive - minimumInclusive) / 2;
    }
  }

  private sealed class InitialBeachPaddingRandomSource : IGenerationRandomSource
  {
    public int CallCount { get; private set; }

    public int FirstFeatureSelectionCall { get; private set; } = -1;

    public int NextInt(
      GenerationRandomStream stream,
      int minimumInclusive,
      int maximumExclusive)
    {
      CallCount++;
      if (stream == GenerationRandomStream.Terrain &&
          minimumInclusive == 0 && maximumExclusive == 5 &&
          FirstFeatureSelectionCall < 0)
      {
        FirstFeatureSelectionCall = CallCount;
        return 0;
      }

      if (minimumInclusive == 90 && maximumExclusive == 110)
      {
        return 100;
      }

      if (minimumInclusive == 0 && maximumExclusive == 3)
      {
        return 1;
      }

      if (minimumInclusive == 5 && maximumExclusive == 40)
      {
        return 5;
      }

      if (minimumInclusive == 5 && maximumExclusive == 30)
      {
        return 5;
      }

      if (minimumInclusive == -2 && maximumExclusive == 3)
      {
        return 0;
      }

      if (minimumInclusive == 0)
      {
        return 1;
      }

      return minimumInclusive + (maximumExclusive - minimumInclusive) / 2;
    }
  }
}
