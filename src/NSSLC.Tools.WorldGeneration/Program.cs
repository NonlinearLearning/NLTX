using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using NSSLC.WorldGeneration;

try {
  string[] seeds = (args.Length > 0 ? args[0] : "12345").Split(',');
  string? reportPath = args.Length > 1 ? args[1] : null;
  GeneratedWorldSize size = args.Length > 2 ? Enum.Parse<GeneratedWorldSize>(args[2], true)
    : GeneratedWorldSize.Small;
  GeneratedWorldEvil evil = args.Length > 3 ? Enum.Parse<GeneratedWorldEvil>(args[3], true)
    : GeneratedWorldEvil.Corruption;
  int difficulty = args.Length > 4 ? int.Parse(args[4]) : 0;
  var summaries = new List<WorldSummary>();
  var previousWorlds = new Dictionary<string, WorldSummary>();
  GeneratedWorld? firstWorld = null;
  foreach (string seed in seeds) {
    var stopwatch = Stopwatch.StartNew();
    GeneratedWorld world = WorldCreation.Create(new WorldCreationRequest(seed, size, evil,
      difficulty), pass => Console.WriteLine($"[{seed} {stopwatch.Elapsed}] {pass}"));
    firstWorld ??= world;
    WorldSummary summary = Summarize(world, stopwatch.Elapsed.TotalSeconds);
    Console.WriteLine(JsonSerializer.Serialize(summary,
      new JsonSerializerOptions { WriteIndented = true }));
    if (previousWorlds.TryGetValue(seed, out WorldSummary? previous) &&
        (previous.TileHash != summary.TileHash || previous.InventoryHash != summary.InventoryHash)) {
      string? firstDifference = previous.Passes.Zip(summary.Passes)
        .FirstOrDefault(pair => pair.First.RandNext != pair.Second.RandNext).First?.Name;
      throw new InvalidOperationException($"Repeated generation changed world data for {seed}; " +
        $"first random-state difference: {firstDifference ?? "none"}.");
    }
    previousWorlds[seed] = summary;
    summaries.Add(summary);
  }
  if (seeds.Length > 1 && Summarize(firstWorld!, 0).TileHash != summaries[0].TileHash) {
    throw new InvalidOperationException("Later generation modified the previously returned world.");
  }
  if (reportPath != null) {
    object report = summaries.Count == 1 ? summaries[0] : summaries;
    File.WriteAllText(reportPath, JsonSerializer.Serialize(report,
      new JsonSerializerOptions { WriteIndented = true }));
  }
  return 0;
} catch (Exception exception) {
  Console.Error.WriteLine(exception);
  return 1;
}

static WorldSummary Summarize(GeneratedWorld world, double durationSeconds) {
  var tilesByType = new SortedDictionary<ushort, long>();
  long active = 0;
  long walls = 0;
  long liquid = 0;
  using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  var row = new byte[world.Height * 14];
  for (int x = 0; x < world.Width; x++) {
    for (int y = 0; y < world.Height; y++) {
      GeneratedTile tile = world.GetTile(x, y);
      if (tile.Type >= NSSLC.WorldGeneration.ID.TileID.Count ||
          tile.Wall >= NSSLC.WorldGeneration.ID.WallID.Count) {
        throw new InvalidOperationException($"Invalid tile metadata at ({x}, {y}).");
      }
      if (tile.Active) {
        active++;
        tilesByType[tile.Type] = tilesByType.GetValueOrDefault(tile.Type) + 1;
      }
      walls += tile.Wall > 0 ? 1 : 0;
      liquid += tile.Liquid > 0 ? 1 : 0;
      Span<byte> bytes = row.AsSpan(y * 14, 14);
      BinaryPrimitives.WriteUInt16LittleEndian(bytes, tile.Type);
      BinaryPrimitives.WriteUInt16LittleEndian(bytes[2..], tile.Wall);
      BinaryPrimitives.WriteInt16LittleEndian(bytes[4..], tile.FrameX);
      BinaryPrimitives.WriteInt16LittleEndian(bytes[6..], tile.FrameY);
      BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], tile.TileHeader);
      bytes[10] = tile.Liquid;
      bytes[11] = tile.Header;
      bytes[12] = tile.Header2;
      bytes[13] = tile.Header3;
    }
    hash.AppendData(row);
  }
  foreach (GeneratedChest chest in world.Chests) {
    if (chest.X < 0 || chest.X >= world.Width || chest.Y < 0 || chest.Y >= world.Height ||
        !world.GetTile(chest.X, chest.Y).Active) {
      throw new InvalidOperationException("A generated chest has an invalid location.");
    }
    foreach (GeneratedItem item in chest.Items) {
      if (item.Stack < 0 || (item.Stack > 0 && (item.Type <= 0 || item.Type >= ItemID.Count)) ||
          item.Prefix >= NSSLC.WorldGeneration.ID.PrefixID.Count) {
        throw new InvalidOperationException("A generated chest contains invalid inventory data.");
      }
    }
  }
  return new WorldSummary {
    Seed = world.Seed,
    WorldId = world.WorldId,
    Width = world.Width,
    Height = world.Height,
    SpawnX = world.SpawnX,
    SpawnY = world.SpawnY,
    Surface = world.Surface,
    RockLayer = world.RockLayer,
    Crimson = world.Crimson,
    DurationSeconds = durationSeconds,
    ActiveTiles = active,
    Walls = walls,
    LiquidTiles = liquid,
    ChestCount = world.Chests.Count,
    ItemCount = world.Chests.Sum(chest => chest.Items.Count(item => item.Stack > 0)),
    PrefixedItems = world.Chests.Sum(chest => chest.Items.Count(item => item.Prefix > 0)),
    InventoryHash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
      world.Chests))),
    Npcs = world.Npcs,
    PassCount = world.Passes.Count,
    TileHash = Convert.ToHexString(hash.GetHashAndReset()),
    TilesByType = tilesByType,
    Passes = world.Passes
  };
}

sealed class WorldSummary {
  public required string Seed { get; init; }
  public int WorldId { get; init; }
  public int Width { get; init; }
  public int Height { get; init; }
  public int SpawnX { get; init; }
  public int SpawnY { get; init; }
  public double Surface { get; init; }
  public double RockLayer { get; init; }
  public bool Crimson { get; init; }
  public double DurationSeconds { get; init; }
  public long ActiveTiles { get; init; }
  public long Walls { get; init; }
  public long LiquidTiles { get; init; }
  public int ChestCount { get; init; }
  public int ItemCount { get; init; }
  public int PrefixedItems { get; init; }
  public required string TileHash { get; init; }
  public required string InventoryHash { get; init; }
  public required IReadOnlyList<GeneratedNpc> Npcs { get; init; }
  public int PassCount { get; init; }
  public required SortedDictionary<ushort, long> TilesByType { get; init; }
  public required IReadOnlyList<CompletedGenerationPass> Passes { get; init; }
}
