using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.WorldStorage;
using static WorldPersistenceRoundTrip;

internal static class Program {
  private static int Main(string[] args) {
    try {
      Verify(args);
      return 0;
    } catch (Exception exception) {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void Verify(string[] args) {
    if (args.Length != 3) {
      throw new ArgumentException(
          "Usage: <existing.wld> <original-roundtrip.json> <new-load-report.json>");
    }
    string worldPath = Path.GetFullPath(args[0]);
    string baselinePath = Path.GetFullPath(args[1]);
    string reportPath = Path.GetFullPath(args[2]);
    Require(!File.Exists(reportPath), "Use a new report path to preserve existing evidence.");
    using JsonDocument baseline = JsonDocument.Parse(File.ReadAllBytes(baselinePath));
    JsonElement expected = baseline.RootElement;
    Require(expected.GetProperty("Succeeded").GetBoolean(), "The baseline did not succeed.");
    byte[] sourceBytes = File.ReadAllBytes(worldPath);
    string fileHash = Convert.ToHexString(SHA256.HashData(sourceBytes));
    var stopwatch = Stopwatch.StartNew();

    var target = new LoadedWorldSession();
    bool targetWasFresh = target.IsFresh;
    Require(targetWasFresh, "The load target is not fresh.");
    var catalog = new GeneratedWorldLoadApiCatalog();
    WorldLoadApiRuntimeBindings bindings =
        WorldStorageCoordinatorFactory.CreateGeneratedWorldLoadBindings(target);
    WorldLoadCoordinator loader = WorldStorageCoordinatorFactory.CreateLoadCoordinator(catalog);
    WorldRecoveryOutcome outcome = loader.Load(worldPath, bindings);
    Require(outcome.CanPublishWorldLoaded && target.IsComplete &&
        outcome.ApiExecution?.Succeeded == true,
        $"Load failed: {outcome.Failure}; {outcome.ApiExecution?.Failure}");
    double loadSeconds = stopwatch.Elapsed.TotalSeconds;

    WorldPersistenceDecodeResult decoded = new WorldFileDocumentDecoder().Decode(sourceBytes);
    Require(decoded.Succeeded && decoded.Document is not null, "Cannot inspect the source DTO.");
    WorldPersistenceDocument document = decoded.Document!;
    WorldRoundTripMetadataComparison.Compare(document, target);
    var descriptor = target.World.Descriptor;
    Require(descriptor.WorldId == expected.GetProperty("WorldId").GetInt32() &&
        descriptor.SizeX == expected.GetProperty("Width").GetInt32() &&
        descriptor.SizeY == expected.GetProperty("Height").GetInt32() &&
        descriptor.SpawnTileX == expected.GetProperty("SpawnX").GetInt32() &&
        descriptor.SpawnTileY == expected.GetProperty("SpawnY").GetInt32(),
        "World identity, dimensions or spawn differ from the original generation report.");
    string tileHash = HashTiles(target.Storage.TileMap);
    Require(tileHash == expected.GetProperty("PersistedTileSha256").GetString(),
        "Loaded tile hash differs from the original generation report.");

    IReadOnlyList<WorldChestSnapshot> chests = target.Storage.WorldContainers.CreateSnapshot();
    CompareChests(Get<WorldFileChestSection>(document, WorldFileChestSection.SectionId), chests);
    CompareNpcs(Get<WorldFileNpcSection>(document, WorldFileNpcSection.SectionId), target);
    CompareSignsAndEntities(document, target);
    Require(chests.Count == expected.GetProperty("ChestsCompared").GetInt32() &&
        chests.Sum(chest => chest.Items.Count) ==
            expected.GetProperty("ItemSlotsCompared").GetInt32() &&
        chests.Sum(chest => chest.Items.Count(item => !item.IsEmpty)) ==
            expected.GetProperty("NonemptyItemSlotsCompared").GetInt32() &&
        target.Storage.Npcs.ActiveCount == expected.GetProperty("NpcsCompared").GetInt32() &&
        target.Storage.WorldSigns.ActiveSignCount ==
            expected.GetProperty("SignsCompared").GetInt32() &&
        target.Storage.TileEntities.Count ==
            expected.GetProperty("TileEntitiesCompared").GetInt32(),
        "Loaded record counts differ from the original generation report.");
    Require(catalog.Descriptors.Count == expected.GetProperty("ApiCount").GetInt32(),
        "The load API count changed.");

    long revision = target.Storage.TileMap.MutationRevision;
    WorldRecoveryOutcome repeated = loader.Load(worldPath, bindings);
    Require(!repeated.CanPublishWorldLoaded && !repeated.RequiresWorldReset &&
        target.Storage.TileMap.MutationRevision == revision &&
        target.World.Descriptor.WorldId == expected.GetProperty("WorldId").GetInt32(),
        "Repeated load must reject without modifying the loaded session.");
    WorldRoundTripMetadataComparison.Compare(document, target);
    CompareChests(Get<WorldFileChestSection>(document, WorldFileChestSection.SectionId),
        target.Storage.WorldContainers.CreateSnapshot());
    CompareNpcs(Get<WorldFileNpcSection>(document, WorldFileNpcSection.SectionId), target);
    CompareSignsAndEntities(document, target);
    Require(HashTiles(target.Storage.TileMap) == tileHash,
        "Repeated load changed the stored tiles.");
    string finalFileHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(worldPath)));
    Require(fileHash == finalFileHash, "Loading changed the source archive.");
    object report = new {
      Succeeded = true,
      EntryPoint = "WorldLoadCoordinator.Load",
      Mode = "LoadExistingArchiveInSeparateProcess",
      WorldPath = worldPath,
      BaselinePath = baselinePath,
      TargetWasFresh = targetWasFresh,
      LoadStatus = outcome.Status,
      outcome.Attempts,
      outcome.CanPublishWorldLoaded,
      ApiExecutionSucceeded = outcome.ApiExecution?.Succeeded,
      ApiCount = catalog.Descriptors.Count,
      ApiIds = catalog.Descriptors.Select(api => api.ApiId).Order(StringComparer.Ordinal),
      descriptor.WorldId,
      Width = descriptor.SizeX,
      Height = descriptor.SizeY,
      SpawnX = descriptor.SpawnTileX,
      SpawnY = descriptor.SpawnTileY,
      TileCountCompared = (long)descriptor.SizeX * descriptor.SizeY,
      PersistedTileSha256 = tileHash,
      TileHashMatchesGenerationReport = true,
      ChestsCompared = chests.Count,
      ItemSlotsCompared = chests.Sum(chest => chest.Items.Count),
      NonemptyItemSlotsCompared = chests.Sum(chest => chest.Items.Count(item => !item.IsEmpty)),
      NpcsCompared = target.Storage.Npcs.ActiveCount,
      NpcRecords = GetNpcRecords(target),
      SignsCompared = target.Storage.WorldSigns.ActiveSignCount,
      TileEntitiesCompared = target.Storage.TileEntities.Count,
      MetadataCompared = true,
      RepeatedLoadRejected = true,
      SourceFileUnchanged = true,
      WorldFileSha256 = fileHash,
      LoadSeconds = loadSeconds,
      TotalSeconds = stopwatch.Elapsed.TotalSeconds
    };
    var options = new JsonSerializerOptions { WriteIndented = true };
    options.Converters.Add(new JsonStringEnumConverter());
    string json = JsonSerializer.Serialize(report, options);
    Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
    using (var writer = new StreamWriter(new FileStream(reportPath, FileMode.CreateNew))) {
      writer.Write(json);
    }
    Console.WriteLine(json);
  }

  private static string HashTiles(TileMapStore tiles) {
    using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    var column = new byte[checked(tiles.Height * 14)];
    for (int x = 0; x < tiles.Width; x++) {
      for (int y = 0; y < tiles.Height; y++) {
        TileCellState tile = tiles.GetTile(x, y);
        Span<byte> bytes = column.AsSpan(y * 14, 14);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, tile.Type);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[2..], tile.Wall);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[4..], tile.FrameX);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[6..], tile.FrameY);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], tile.TileHeader);
        bytes[10] = tile.LiquidAmount;
        bytes[11] = tile.Header;
        bytes[12] = tile.Header2;
        bytes[13] = tile.Header3;
      }
      hash.AppendData(column);
    }
    return Convert.ToHexString(hash.GetHashAndReset());
  }

  private static void CompareChests(WorldFileChestSection section,
      IReadOnlyList<WorldChestSnapshot> loaded) {
    Require(section.Chests.Count == loaded.Count, "Chest count differs from the archive DTO.");
    for (int index = 0; index < section.Chests.Count; index++) {
      WorldFileChestRecord expected = section.Chests[index];
      WorldChestSnapshot actual = loaded[index];
      Require(actual.Anchor == new TileCoordinate(expected.X, expected.Y) &&
          actual.Name == expected.Name && actual.Items.Count == expected.Items.Count,
          $"Chest {index} metadata differs.");
      for (int slot = 0; slot < expected.Items.Count; slot++) {
        WorldFileChestItem item = expected.Items[slot];
        Require(actual.Items[slot].Stack == item.Stack &&
            actual.Items[slot].Type == item.Type && actual.Items[slot].Prefix == item.Prefix,
            $"Chest {index}, slot {slot} differs from the archive DTO.");
      }
    }
  }

  private static void CompareNpcs(WorldFileNpcSection section, LoadedWorldSession loaded) {
    WorldFileNpcRecord[] expected = section.TownNpcs.Concat(section.SavedNpcs).ToArray();
    Require(expected.Length == loaded.Storage.Npcs.ActiveCount, "NPC counts differ.");
    for (int index = 0; index < expected.Length; index++) {
      Require(loaded.Storage.Npcs.TryGetOccupiedAt(index, out _, out _,
          out WorldEntityState? entity) && entity is WorldNpcState, $"NPC {index} is missing.");
      var actual = (WorldNpcState)entity!;
      WorldFileNpcRecord npc = expected[index];
      Require(actual.NetId == npc.NetId && actual.LegacyTypeName == npc.LegacyTypeName &&
          actual.Name == npc.Name && actual.IsTownNpc == npc.IsTownNpc &&
          actual.X == npc.PositionX && actual.Y == npc.PositionY &&
          actual.Home == new TileCoordinate(npc.HomeTileX, npc.HomeTileY) &&
          actual.Homeless == npc.Homeless && actual.Variation == npc.TownNpcVariationIndex &&
          actual.HomelessDespawn == npc.HomelessDespawn,
          $"NPC {index} differs from the archive DTO.");
    }
  }

  private static void CompareSignsAndEntities(WorldPersistenceDocument document,
      LoadedWorldSession target) {
    WorldFileSignSection section =
        Get<WorldFileSignSection>(document, WorldFileSignSection.SectionId);
    IReadOnlyList<WorldSignSnapshot> signs = target.Storage.WorldSigns.CreateSnapshot();
    Require(section.Signs.Count == signs.Count, "Sign counts differ.");
    for (int index = 0; index < signs.Count; index++) {
      Require(signs[index].Anchor == new TileCoordinate(section.Signs[index].X,
          section.Signs[index].Y) && signs[index].Text == section.Signs[index].Text,
          $"Sign {index} differs from the archive DTO.");
    }
    IReadOnlyList<TileEntitySnapshot> expectedEntities = new WorldFileTileEntityCodec().Decode(
        Get<WorldFileTileEntitySection>(document, WorldFileTileEntitySection.SectionId));
    IReadOnlyList<TileEntitySnapshot> entities = target.Storage.TileEntities.CreateSnapshot();
    Require(expectedEntities.Count == entities.Count, "Tile entity counts differ.");
    foreach (TileEntitySnapshot expected in expectedEntities) {
      TileEntitySnapshot actual = entities.Single(entity => entity.Id == expected.Id);
      Require(actual.Type == expected.Type && actual.Anchor == expected.Anchor &&
          actual.Items.SequenceEqual(expected.Items) && actual.NpcIndex == expected.NpcIndex &&
          actual.LogicCheck == expected.LogicCheck && actual.LogicOn == expected.LogicOn &&
          actual.Pose == expected.Pose, "A tile entity differs from the archive DTO.");
    }
  }

  private static object[] GetNpcRecords(LoadedWorldSession session) {
    var records = new List<object>(session.Storage.Npcs.ActiveCount);
    for (int index = 0; index < session.Storage.Npcs.Capacity; index++) {
      if (session.Storage.Npcs.TryGetOccupiedAt(index, out _, out _,
          out WorldEntityState? entity) && entity is WorldNpcState npc) {
        records.Add(new {
          npc.NetId,
          npc.LegacyTypeName,
          npc.IsTownNpc,
          npc.Name,
          npc.X,
          npc.Y,
          npc.Homeless,
          npc.Home,
          npc.Variation,
          npc.HomelessDespawn
        });
      }
    }
    return records.ToArray();
  }

  private static T Get<T>(WorldPersistenceDocument document, string id) where T : notnull {
    Require(document.TryGetSection<T>(id, out var section) && section.IsPresent,
        "Missing source DTO section " + id);
    return section.Value;
  }
}
