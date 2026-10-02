using System;
using System.Collections.Generic;
using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Definitions;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void TestRulesOutsideSkyblockWorld()
{
  WorldSkyblockGenerationScanSnapshot scan = new(
    generationId: 1,
    scanVersion: 1,
    worldTileCount: 100,
    currentActiveTiles: 1,
    activeTileTypes: new HashSet<ushort> { 26 },
    wallTypes: new HashSet<ushort> { 87 });
  WorldSkyblockGenerationRulesSelection rules = WorldSkyblockGenerationRulesQuery.Evaluate(
    new WorldSkyblockGenerationRulesInput(
      scan,
      new HashSet<ushort>(),
      new HashSet<ushort> { 87 },
      skyblockWorld: false));

  Assert(!rules.NoAltars, "Altar presence must be derived outside Skyblock worlds.");
  Assert(!rules.NoDungeon, "Dungeon-wall presence must be derived outside Skyblock worlds.");
  Assert(!rules.NoTemple, "Temple-wall presence must be derived outside Skyblock worlds.");
  Assert(!rules.LowTiles, "Low-tile policy must remain disabled outside Skyblock worlds.");
}

static void TestLoadScanAndCommit()
{
  WorldSkyblockGenerationScanComponent component = new(
    generationId: 2,
    tileTypeCount: 500,
    wallTypeCount: 200);
  RecordingGridReader grid = new();
  grid.Set(40, 40, new WorldSkyblockGenerationTileObservation(true, 58, 87));
  RecordingEffects effects = new();

  WorldSkyblockGenerationRulesCommitResult result =
    WorldSkyblockGenerationScanSystem.ScanAndCommit(
      component,
      new WorldSkyblockGenerationDimensions(82, 82),
      scanVersion: 3,
      grid,
      new HashSet<ushort>(),
      new HashSet<ushort>(),
      skyblockWorld: true,
      effects);

  Assert(grid.ReadCount == 4, "The load scan must exclude the outer 40-tile border.");
  Assert(!result.CommittedRules.NoHellstone, "Active Hellstone must be detected.");
  Assert(!result.CommittedRules.NoTemple, "Temple wall must be detected.");
  Assert(result.CommittedRules.LowTiles, "A sparse Skyblock scan must set lowTiles.");
  Assert(result.LowTilesChanged, "The first lowTiles transition must be reported.");
  Assert(component.Lifecycle == WorldSkyblockGenerationScanLifecycle.Ready,
    "Commit must clear the active scan lifecycle.");
  Assert(component.LastCommittedRules == result.CommittedRules,
    "Commit must retain the authoritative derived rules.");
  Assert(effects.Calls.Count == 2 &&
      effects.Calls[0] == "clear-dungeon" &&
      effects.Calls[1] == "low-tiles:true",
    "Dungeon clearing must precede the lowTiles notification.");
}

static void TestIncrementalColumnsAndCommit()
{
  WorldSkyblockGenerationScanComponent component = new(
    generationId: 3,
    tileTypeCount: 500,
    wallTypeCount: 200);
  RecordingGridReader grid = new();
  grid.Set(0, 40, new WorldSkyblockGenerationTileObservation(true, 26, 0));
  RecordingEffects effects = new();
  WorldSkyblockGenerationRulesCommitResult? completed = null;

  for (int x = 0; x < 82; x++)
  {
    WorldSkyblockGenerationRulesCommitResult? result =
      WorldSkyblockGenerationScanSystem.AccumulateColumnAndCommit(
        component,
        new WorldSkyblockGenerationDimensions(82, 82, worldSurfaceY: 40),
        x,
        scanVersion: 4,
        grid,
        new HashSet<ushort>(),
        new HashSet<ushort>(),
        skyblockWorld: true,
        effects);

    if (x < 81)
    {
      Assert(!result.HasValue, "Rules must not commit before the last world column.");
    }
    else
    {
      completed = result;
    }
  }

  Assert(grid.ReadCount == 164, "Incremental scanning must read every column's inner Y range.");
  Assert(completed.HasValue, "The final world column must calculate and commit the scan.");
  WorldSkyblockGenerationRulesCommitResult completedResult = completed ??
    throw new InvalidOperationException("The final-column result is missing.");
  Assert(!completedResult.CommittedRules.NoAltars,
    "The first column's altar must remain in the accumulated scan.");
  Assert(completedResult.CommittedRules.LowTiles,
    "Incremental active-tile counts must use the complete world area denominator.");
  Assert(component.Lifecycle == WorldSkyblockGenerationScanLifecycle.Ready,
    "The final-column commit must reset scan scratch state.");
  Assert(effects.Calls.Count == 2 && effects.Calls[0] == "clear-dungeon",
    "Incremental commit must preserve the commit effect order.");
}

static void TestIncrementalColumnsRequireOrder()
{
  WorldSkyblockGenerationScanComponent component = new(
    generationId: 4,
    tileTypeCount: 500,
    wallTypeCount: 200);
  bool rejected = false;
  try
  {
    WorldSkyblockGenerationScanSystem.AccumulateColumn(
      component,
      new WorldSkyblockGenerationDimensions(82, 82),
      x: 1,
      scanVersion: 5,
      new RecordingGridReader());
  }
  catch (InvalidOperationException)
  {
    rejected = true;
  }

  Assert(rejected, "Incremental scans must reject a missing first column.");
}

static void TestIncrementalCommitRequiresFinalColumn()
{
  WorldSkyblockGenerationScanComponent component = new(
    generationId: 5,
    tileTypeCount: 500,
    wallTypeCount: 200);
  WorldSkyblockGenerationScanSystem.AccumulateColumn(
    component,
    new WorldSkyblockGenerationDimensions(82, 82, worldSurfaceY: 40),
    x: 0,
    scanVersion: 6,
    new RecordingGridReader());

  bool rejected = false;
  try
  {
    WorldSkyblockGenerationRulesCommitSystem.Commit(
      component,
      new WorldSkyblockGenerationRulesSelection(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        GenerationId: 5,
        ScanVersion: 6),
      new RecordingEffects());
  }
  catch (InvalidOperationException)
  {
    rejected = true;
  }

  Assert(rejected, "Incremental rules must not commit before observing the final column.");
}

TestRulesOutsideSkyblockWorld();
TestLoadScanAndCommit();
TestIncrementalColumnsAndCommit();
TestIncrementalColumnsRequireOrder();
TestIncrementalCommitRequiresFinalColumn();
Console.WriteLine(
  "PASS: P18 core Skyblock rules, load scan, incremental scan, and commit boundary");

sealed class RecordingGridReader : IWorldSkyblockGenerationGridReader
{
  private readonly Dictionary<(int X, int Y), WorldSkyblockGenerationTileObservation> _tiles =
    new();

  public int ReadCount { get; private set; }

  public void Set(int x, int y, WorldSkyblockGenerationTileObservation observation)
  {
    _tiles[(x, y)] = observation;
  }

  public WorldSkyblockGenerationTileObservation Read(int x, int y)
  {
    ReadCount++;
    return _tiles.TryGetValue((x, y), out WorldSkyblockGenerationTileObservation observation)
      ? observation
      : new WorldSkyblockGenerationTileObservation(false, 0, 0);
  }
}

sealed class RecordingEffects : ISkyblockGenerationEffectsPort
{
  public List<string> Calls { get; } = new();

  public void ClearDungeonCoordinates()
  {
    Calls.Add("clear-dungeon");
  }

  public void NotifyLowTilesChanged(bool lowTiles)
  {
    Calls.Add($"low-tiles:{lowTiles.ToString().ToLowerInvariant()}");
  }
}
