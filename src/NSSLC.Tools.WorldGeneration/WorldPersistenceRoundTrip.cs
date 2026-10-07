using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSSLC.WorldGeneration;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;

internal static class WorldPersistenceRoundTrip {
  public static int Run(string[] args) {
    if (args.Length < 1 || args.Length > 5) {
      throw new ArgumentException(
          "Usage: --roundtrip <outputDirectory> [seed] [Small|Medium|Large] [evil] [difficulty]");
    }
    string outputDirectory = Path.GetFullPath(args[0]);
    Directory.CreateDirectory(outputDirectory);
    string seed = args.Length > 1 ? args[1] : "12345";
    GeneratedWorldSize size = args.Length > 2 ? Enum.Parse<GeneratedWorldSize>(args[2], true)
        : GeneratedWorldSize.Small;
    GeneratedWorldEvil evil = args.Length > 3 ? Enum.Parse<GeneratedWorldEvil>(args[3], true)
        : GeneratedWorldEvil.Corruption;
    int difficulty = args.Length > 4 ? int.Parse(args[4]) : 0;
    string worldPath = Path.Combine(outputDirectory, "generated.wld");
    if (File.Exists(worldPath)) {
      throw new IOException("Use a new output directory to keep existing world saves intact.");
    }
    var stopwatch = Stopwatch.StartNew();
    GeneratedWorld generated = WorldCreation.Create(
        new WorldCreationRequest(seed, size, evil, difficulty),
        pass => Console.WriteLine($"[{stopwatch.Elapsed}] {pass}"));
    double generationSeconds = stopwatch.Elapsed.TotalSeconds;
    WorldPersistenceDocument document = GeneratedWorldDocumentProjection.Create(
        generated, "RoundTrip " + seed, Guid.NewGuid(), DateTime.UtcNow);
    var saveCoordinator = WorldStorageCoordinatorFactory.CreateSaveCoordinator();
    WorldSaveProjection saved = saveCoordinator.Save(new WorldSaveCommand(
        worldPath, document, new WorldBackupPolicy(0)),
        WorldStorageCoordinatorFactory.CreateSaveEncoder(),
        WorldStorageCoordinatorFactory.CreateSaveValidationQuery());
    Require(saved.Committed, $"Save failed: {saved.Failure}");
    Console.WriteLine($"Saved {new FileInfo(worldPath).Length} bytes to {worldPath}");

    // The target contains no references to generation globals or to the original tile buffer.
    var loaded = new LoadedWorldSession();
    var catalog = new GeneratedWorldLoadApiCatalog();
    var bindings = WorldStorageCoordinatorFactory.CreateGeneratedWorldLoadBindings(loaded);
    WorldRecoveryOutcome result = WorldStorageCoordinatorFactory.CreateLoadCoordinator(catalog)
        .Load(worldPath, bindings);
    Require(result.CanPublishWorldLoaded && loaded.IsComplete,
        $"Load failed: {result.Failure}; {result.ApiExecution?.Failure}");
    Console.WriteLine($"Loaded through {catalog.Descriptors.Count} owner APIs; comparing world.");

    Require(loaded.Storage.TileMap.Width == generated.Width &&
        loaded.Storage.TileMap.Height == generated.Height, "Tile dimensions differ.");
    var buffer = new byte[generated.Height * 14];
    using var tileHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    for (int x = 0; x < generated.Width; x++) {
      for (int y = 0; y < generated.Height; y++) {
        TileCellState expected = WorldFileTileCodec.Normalize(generated.GetTile(x, y),
            generated.FrameImportant);
        TileCellState actual = loaded.Storage.TileMap.GetTile(x, y);
        Require(expected.Equals(actual), $"Persisted tile differs at ({x}, {y}).");
        Span<byte> bytes = buffer.AsSpan(y * 14, 14);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, actual.Type);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[2..], actual.Wall);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[4..], actual.FrameX);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[6..], actual.FrameY);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], actual.TileHeader);
        bytes[10] = actual.LiquidAmount;
        bytes[11] = actual.Header;
        bytes[12] = actual.Header2;
        bytes[13] = actual.Header3;
      }
      tileHash.AppendData(buffer);
    }
    string persistedTileHash = Convert.ToHexString(tileHash.GetHashAndReset());
    IReadOnlyList<WorldChestSnapshot> loadedChests =
        loaded.Storage.WorldContainers.CreateSnapshot();
    Require(loadedChests.Count == generated.Chests.Count, "Chest counts differ.");
    for (int index = 0; index < generated.Chests.Count; index++) {
      GeneratedChest expected = generated.Chests[index];
      WorldChestSnapshot actual = loadedChests[index];
      Require(actual.Anchor == new TileCoordinate(expected.X, expected.Y) &&
          actual.Name == expected.Name && actual.Items.Count == expected.Items.Count,
          $"Chest {index} metadata differs.");
      for (int slot = 0; slot < expected.Items.Count; slot++) {
        GeneratedItem item = expected.Items[slot];
        Require(actual.Items[slot].Stack == item.Stack &&
            (item.Stack == 0 || (actual.Items[slot].Type == item.Type &&
            actual.Items[slot].Prefix == item.Prefix)), $"Chest {index}, slot {slot} differs.");
      }
    }
    Require(loaded.Storage.Npcs.ActiveCount == generated.Npcs.Count, "NPC counts differ.");
    for (int index = 0; index < generated.Npcs.Count; index++) {
      GeneratedNpc expected = generated.Npcs[index];
      Require(loaded.Storage.Npcs.TryGetOccupiedAt(index, out _, out _,
          out WorldEntityState? entity),
          $"NPC {index} is missing.");
      var actual = entity as WorldNpcState ??
          throw new InvalidOperationException("Wrong NPC state.");
      Require(actual.NetId == expected.Type && actual.Name == expected.Name &&
          actual.X == expected.X && actual.Y == expected.Y &&
          actual.Home == new TileCoordinate(expected.HomeX, expected.HomeY) &&
          actual.Homeless == expected.Homeless && actual.IsTownNpc == expected.IsTownNpc &&
          actual.Variation == (expected.IsTownNpc ? expected.Variation : null) &&
          actual.HomelessDespawn == expected.HomelessDespawn, $"NPC {index} differs.");
    }
    IReadOnlyList<WorldSignSnapshot> signs = loaded.Storage.WorldSigns.CreateSnapshot();
    Require(signs.Count == generated.Signs.Count, "Sign counts differ.");
    for (int index = 0; index < signs.Count; index++) {
      Require(signs[index].Anchor == new TileCoordinate(generated.Signs[index].X,
          generated.Signs[index].Y) && signs[index].Text == generated.Signs[index].Text,
          $"Sign {index} differs.");
    }
    Require(loaded.Storage.TileEntities.Count == generated.TileEntities.Count,
        "Tile entity counts differ.");
    Require(loaded.FrameImportant.SequenceEqual(generated.FrameImportant),
        "The frame importance table differs.");
    WorldRoundTripMetadataComparison.Compare(document, loaded);
    object report = new {
      Succeeded = true,
      Seed = seed, Size = size, Evil = evil, Difficulty = difficulty,
      generated.WorldId, generated.Width, generated.Height, generated.SpawnX, generated.SpawnY,
      TileCountCompared = (long)generated.Width * generated.Height,
      PersistedTileSha256 = persistedTileHash,
      ChestsCompared = loadedChests.Count,
      ItemSlotsCompared = loadedChests.Sum(chest => chest.Items.Count),
      NonemptyItemSlotsCompared = loadedChests.Sum(chest =>
          chest.Items.Count(item => !item.IsEmpty)),
      NpcsCompared = loaded.Storage.Npcs.ActiveCount,
      SignsCompared = signs.Count,
      TileEntitiesCompared = loaded.Storage.TileEntities.Count,
      MetadataCompared = true,
      ApiCount = catalog.Descriptors.Count,
      SectionIds = document.SectionIds,
      LoadStatus = result.Status,
      result.CanPublishWorldLoaded,
      saved.Stages,
      WorldPath = worldPath,
      WorldFileBytes = new FileInfo(worldPath).Length,
      GenerationSeconds = generationSeconds,
      TotalSeconds = stopwatch.Elapsed.TotalSeconds
    };
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new JsonStringEnumConverter());
    string json = JsonSerializer.Serialize(report, options);
    File.WriteAllText(Path.Combine(outputDirectory, "roundtrip.json"), json);
    Console.WriteLine(json);
    return 0;
  }

  internal static void Require(bool condition, string message) {
    if (!condition) {
      throw new InvalidOperationException(message);
    }
  }
}
