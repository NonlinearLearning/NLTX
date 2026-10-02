using Terraria.WorldGeneration.Adapters;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}

static void AssertEqual<T>(T expected, T actual, string message)
  where T : IEquatable<T>
{
  Assert(expected.Equals(actual), $"{message} Expected {expected}, got {actual}.");
}

static void AssertNextColumn(
  WorldTileMetricsComponent component,
  int maxTilesX,
  int expectedColumnX)
{
  for (int tick = 0; tick < 29; tick++)
  {
    Assert(
      !WorldTileMetricsSystem.AdvanceCadence(component, maxTilesX, out _),
      "The column scan must not run before the 30th tick.");
  }

  Assert(
    WorldTileMetricsSystem.AdvanceCadence(component, maxTilesX, out int columnX),
    "The column scan runs on the 30th tick.");
  AssertEqual(expectedColumnX, columnX, "The cadence returns the next world column.");
}

WorldTileMetricsComponent metrics = new();
int[] tileCounts = new int[640];
tileCounts[2] = 10;
tileCounts[477] = 4;
tileCounts[1] = 3;
tileCounts[60] = 2;
tileCounts[53] = 1;
tileCounts[161] = 5;
tileCounts[100] = 2;
tileCounts[101] = 3;
tileCounts[200] = 4;
tileCounts[300] = 6;
tileCounts[474] = 7;
tileCounts[195] = 8;

bool calculateSkyblock = WorldTileMetricsSystem.CompleteColumn(
  metrics,
  tileCounts,
  new[] { 100, 101 },
  new[] { 200 },
  new[] { 300 },
  true,
  0,
  4);
Assert(!calculateSkyblock, "A non-final column must not request Skyblock calculation.");
Assert(tileCounts.All(count => count == 0), "Alignment accumulation clears the tile scratch counts.");

WorldTileMetricsPublicationResult beforeColumnZero =
  WorldTileMetricsSystem.BeginColumn(metrics, 0);
Assert(beforeColumnZero.HasSnapshot, "Column zero publishes the completed metrics snapshot.");
Assert(beforeColumnZero.ShouldSendMessage57, "Version4 requests message 57 at column zero.");
AssertEqual(5, beforeColumnZero.Snapshot.GoodCount, "Hallow alignment total.");
AssertEqual(11, beforeColumnZero.Snapshot.EvilCount, "Corruption and remix evil total.");
AssertEqual(14, beforeColumnZero.Snapshot.BloodCount, "Crimson and remix blood total.");
AssertEqual(55, beforeColumnZero.Snapshot.SolidCount, "Solid alignment total.");
AssertEqual((byte)9, beforeColumnZero.Snapshot.GoodPercent, "Good percentage rounding.");
AssertEqual((byte)20, beforeColumnZero.Snapshot.EvilPercent, "Evil percentage rounding.");
AssertEqual((byte)25, beforeColumnZero.Snapshot.BloodPercent, "Blood percentage rounding.");
Console.WriteLine("PASS: Version4 tile alignment aggregation and publication");

WorldTileMetricsSystem.CompleteColumnStart(metrics, 0);
int[] nextTileCounts = new int[640];
nextTileCounts[200] = 2;
WorldTileMetricsSystem.AccumulateAlignmentCounts(
  metrics,
  nextTileCounts,
  Array.Empty<int>(),
  new[] { 200 },
  Array.Empty<int>(),
  false,
  clearCounts: true);
WorldTileMetricsPublicationResult restarted =
  WorldTileMetricsSystem.BeginColumn(metrics, 0);
AssertEqual(0, restarted.Snapshot.GoodCount, "Cleared alignment total.");
AssertEqual(2, restarted.Snapshot.EvilCount, "Counts after clearCounts are retained.");
AssertEqual(2, restarted.Snapshot.SolidCount, "The evil alignment total also contributes to solid count.");
Console.WriteLine("PASS: Version4 clearCounts reset before accumulation");

WorldTileMetricsSystem.CompleteColumnStart(metrics, 0);
calculateSkyblock = WorldTileMetricsSystem.CompleteColumn(
  metrics,
  nextTileCounts,
  Array.Empty<int>(),
  Array.Empty<int>(),
  Array.Empty<int>(),
  false,
  3,
  4);
Assert(calculateSkyblock, "The last world column requests Skyblock.Calculate.");
Console.WriteLine("PASS: Version4 final-column Skyblock handoff");

Dictionary<int, WorldTileMetricsTileSample> scanTiles = new()
{
  [40] = new WorldTileMetricsTileSample(5, 2, true),
  [41] = new WorldTileMetricsTileSample(5, 3, true),
  [42] = new WorldTileMetricsTileSample(0, 4, true),
  [43] = new WorldTileMetricsTileSample(7, 5, false),
  [44] = new WorldTileMetricsTileSample(6, 6, true)
};
List<int> scannedYs = new();
List<string> scanEffects = new();
int currentScanY = 0;
int[] scannedTileCounts = new int[16];

WorldTileMetricsSystem.ScanColumnTiles(
  3,
  40.0,
  100,
  new DelegateWorldTileMetricsTileSource(
    (columnX, y) =>
    {
      AssertEqual(3, columnX, "The tile adapter receives the requested column.");
      currentScanY = y;
      scannedYs.Add(y);
      scanEffects.Add($"read:{y}");
      return scanTiles.GetValueOrDefault(y);
    }),
  scannedTileCounts,
  wall => scanEffects.Add($"wall:{currentScanY}:{wall}"),
  () => scanEffects.Add($"active:{currentScanY}"),
  type => scanEffects.Add($"tile:{currentScanY}:{type}"));

AssertEqual(20, scannedYs.Count, "The scan covers both Version4 vertical ranges.");
Assert(
  scannedYs.SequenceEqual(Enumerable.Range(40, 20)),
  "The scan visits every tile in the Version4 surface and lower ranges once.");
AssertEqual(40, scannedYs[0], "The surface range starts at tile 40.");
AssertEqual(59, scannedYs[^1], "The lower range ends before maxTilesY - 40.");
AssertEqual(
  6,
  scannedTileCounts[5],
  "Surface and lower weights accumulate across the range boundary.");
AssertEqual(1, scannedTileCounts[6], "A distinct active type starts its own count.");
AssertEqual(
  0,
  scannedTileCounts[0],
  "An active type-zero tile does not replace the pending run type.");
AssertEqual(
  0,
  scannedTileCounts[7],
  "Inactive tiles do not contribute to tile type counts.");
Assert(
  scanEffects.Take(18).SequenceEqual(new[]
  {
    "read:40", "wall:40:2", "active:40", "tile:40:5",
    "read:41", "wall:41:3", "active:41", "tile:41:5",
    "read:42", "wall:42:4", "active:42", "tile:42:0",
    "read:43", "wall:43:5",
    "read:44", "wall:44:6", "active:44", "tile:44:6"
  }),
  "Tile reads and Skyblock effects retain their per-cell order.");
Console.WriteLine("PASS: Version4 column scan weights and per-tile effects");

WorldTileMetricsComponent cadenceMetrics = new();
for (int tick = 0; tick < 29; tick++)
{
  Assert(
    !WorldTileMetricsSystem.AdvanceCadence(cadenceMetrics, 3, out _),
    "The cadence does not scan during its first 29 ticks.");
}

Assert(
  WorldTileMetricsSystem.AdvanceCadence(cadenceMetrics, 3, out int firstColumnX),
  "The cadence scans on tick 30.");
AssertEqual(0, firstColumnX, "The first scheduled column is zero.");
Assert(
  !WorldTileMetricsSystem.AdvanceCadence(cadenceMetrics, 3, out _),
  "The cadence waits after the tick-30 scan.");
for (int tick = 0; tick < 28; tick++)
{
  Assert(
    !WorldTileMetricsSystem.AdvanceCadence(cadenceMetrics, 3, out _),
    "The cadence does not scan at tick 59.");
}

Assert(
  WorldTileMetricsSystem.AdvanceCadence(cadenceMetrics, 3, out int secondColumnX),
  "The cadence scans on tick 60.");
AssertEqual(1, secondColumnX, "The second scheduled column is one.");
AssertNextColumn(cadenceMetrics, 3, 2);
AssertNextColumn(cadenceMetrics, 3, 0);
Console.WriteLine("PASS: Version4 tile scan cadence and column wrap");

sealed class DelegateWorldTileMetricsTileSource(
  Func<int, int, WorldTileMetricsTileSample> readOrCreateTile) : IWorldTileMetricsTileSource
{
  public WorldTileMetricsTileSample ReadOrCreateTile(int x, int y)
  {
    return readOrCreateTile(x, y);
  }
}
