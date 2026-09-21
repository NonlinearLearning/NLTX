using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Platform;
using Terraria.NonAuthoritative.Player;
using Terraria.NonAuthoritative.WorldSession;
using Terraria.NonAuthoritative.WorldSession.Calendar;

static class Program
{
  private static int Main()
  {
    try
    {
      TestExtensionFilterCopiesInput();
      TestPathParsingPreservesVersion4Rules();
      TestCloudPathsBypassLocalFullPath();
      TestRuntimeCapabilityQueryUsesExplicitProbe();
      TestFileMetadataRoundTripAndValidation();
      TestFavoritesAdapterPreservesVersion4JsonShape();
      TestCloudAdapterPreservesPathAndClassifiesUnavailableProvider();
      TestWorldSaveOrdersValidationBeforeBackupPublication();
      TestWorldSaveValidationFailurePreservesPreviousFiles();
      TestPlayerSaveSessionClockAndPolicyBoundary();
      TestWorldIdentitySnapshotKeepsLegacyAndGuidNamesSeparate();
      TestWorldValidityAndRulesRemainReadOnlyHandoffs();
      TestWorldTileHeaderCodecRoundTripAndMalformedInput();
      TestWorldRecoveryUsesBoundedBackupFallback();
      TestWorldTemporaryEventContextCopiesAndRestoresTransientState();
      TestConfigurationAdapterPreservesCallbackOrderAndFormats();
      Console.WriteLine("C01, C02, C03, C04, C05, C06, C07, C08, C09, C10 and C11 focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestExtensionFilterCopiesInput()
  {
    string[] extensions = { "png", "jpg" };
    ExtensionFilterValue filter = new("Images", extensions);
    extensions[0] = "exe";

    Require(filter.Name == "Images", "filter name must be retained");
    Require(filter.Extensions.Count == 2, "filter extension count must be retained");
    Require(filter.Extensions[0] == "png", "filter extensions must be copied");
  }

  private static void TestPathParsingPreservesVersion4Rules()
  {
    Require(
      FilePathParsingAdapter.GetFileName("C:\\worlds\\dotted.name.wld") ==
        "dotted.name.wld",
      "filename projection must preserve dotted names");
    Require(
      FilePathParsingAdapter.GetFileName("C:/worlds/extensionless", includeExtension: false) ==
        "extensionless",
      "filename projection must preserve extensionless names");
    Require(
      FilePathParsingAdapter.GetFileName("C:/worlds/name.wld", includeExtension: false) ==
        "name",
      "filename projection must remove only the final extension");
    Require(
      FilePathParsingAdapter.GetParentFolderPath("C:\\worlds\\name.wld") ==
        "C:\\worlds\\",
      "parent projection must preserve the original separator");
    Require(
      FilePathParsingAdapter.GetParentFolderPath("world.wld") == string.Empty,
      "parent projection must be empty for a path without a separator");
  }

  private static void TestCloudPathsBypassLocalFullPath()
  {
    const string cloudPath = "cloud:/worlds/world.wld";
    Require(
      FilePathParsingAdapter.GetFullPath(cloudPath, isCloud: true) == cloudPath,
      "cloud paths must not be normalized as local paths");
  }

  private static void TestRuntimeCapabilityQueryUsesExplicitProbe()
  {
    RuntimeCapabilityQuery supported = new(() => true);
    RuntimeCapabilityQuery unsupported = new(() => false);

    Require(supported.IsReflectionContextAvailable(), "supported probe must be reported");
    Require(!unsupported.IsReflectionContextAvailable(), "unsupported probe must be reported");
  }

  private static void TestFileMetadataRoundTripAndValidation()
  {
    FileMetadataValue expected = new(
      SaveFileType.World,
      revision: 7,
      isFavorite: true);
    byte[] bytes = FileMetadataCodec.Write(expected);
    Require(bytes.Length == FileMetadataCodec.Size, "metadata must use the 20-byte format");

    FileMetadataValue actual = FileMetadataCodec.Read(bytes, SaveFileType.World);
    Require(actual == expected, "metadata round trip must preserve all fields");

    byte[] badMagic = (byte[])bytes.Clone();
    badMagic[0] ^= 1;
    RequireThrows<FormatException>(
      () => FileMetadataCodec.Read(badMagic, SaveFileType.World),
      "bad metadata magic must be rejected");

    byte[] badType = (byte[])bytes.Clone();
    badType[7] = (byte)SaveFileType.Player;
    RequireThrows<FormatException>(
      () => FileMetadataCodec.Read(badType, SaveFileType.World),
      "unexpected metadata type must be rejected");
  }

  private static void TestFavoritesAdapterPreservesVersion4JsonShape()
  {
    string directory = Directory.CreateTempSubdirectory("nltx-p14-favorites-").FullName;
    try
    {
      string path = Path.Combine(directory, "favorites.json");
      FilePlatformAdapter platform = new();
      FavoritesIndexAdapter favorites = new(platform, path, isCloudSave: false);
      SaveFileReferenceSnapshot world = new(path: "C:\\worlds\\alpha.wld", isCloudSave: false, type: SaveFileType.World, isFavorite: false);

      FilePlatformOperationResult setResult = favorites.SetFavorite(world, isFavorite: true);
      Require(setResult.Succeeded, "favorites update must report a successful write");
      string json = File.ReadAllText(path);
      Require(json.Contains("\"World\"", StringComparison.Ordinal), "favorites JSON must use the file type key");
      Require(json.Contains("alpha.wld", StringComparison.Ordinal), "favorites JSON must use the file name key");

      FavoritesIndexAdapter reloaded = new(platform, path, isCloudSave: false);
      FilePlatformOperationResult loadResult = reloaded.Load();
      Require(loadResult.Succeeded, "favorites load must report success");
      Require(reloaded.IsFavorite(world), "favorites load must restore the favorite bit");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static void TestCloudAdapterPreservesPathAndClassifiesUnavailableProvider()
  {
    FakeCloudFileStore cloud = new();
    FilePlatformAdapter platform = new(cloud);
    const string path = "cloud:/worlds/world.wld";

    FileReadResult read = platform.ReadAllBytes(path, isCloud: true);
    Require(read.Succeeded, "cloud read must use the injected provider");
    Require(cloud.LastReadPath == path, "cloud path must reach the provider unchanged");

    FilePlatformAdapter unavailable = new();
    FilePlatformOperationResult write = unavailable.WriteAllBytes(path, new byte[] { 1 }, isCloud: true);
    Require(!write.Succeeded, "missing cloud provider must not report a write success");
    Require(
      write.Failure.Kind == FilePlatformFailureKind.ProviderUnavailable,
      "missing cloud provider must be classified");
  }

  private static void TestWorldSaveOrdersValidationBeforeBackupPublication()
  {
    string directory = Directory.CreateTempSubdirectory("nltx-p14-world-save-").FullName;
    try
    {
      string path = Path.Combine(directory, "world.wld");
      File.WriteAllBytes(path, new byte[] { 1, 2 });
      File.WriteAllBytes(path + ".bak", new byte[] { 9 });
      FilePlatformAdapter platform = new();
      WorldSaveTransactionSystem system = new(platform);
      WorldSaveCommand command = new(path, isCloudSave: false, new WorldBackupPolicy(2));
      RecordingWorldSaveEncoder encoder = new(new byte[] { 3, 4 });
      WorldSaveValidationQuery validation = new(bytes => bytes.Span.SequenceEqual(new byte[] { 3, 4 }));

      WorldSaveProjection projection = system.Save(command, encoder, validation);

      Require(projection.Committed, "valid world save must commit");
      Require(
        projection.Stages.SequenceEqual(new[]
        {
          WorldSaveStage.Capture,
          WorldSaveStage.Encode,
          WorldSaveStage.Commit,
          WorldSaveStage.ReadBack,
          WorldSaveStage.Validate,
          WorldSaveStage.Backup,
          WorldSaveStage.Publish
        }),
        "world save stages must preserve the Version4 ordering");
      Require(File.ReadAllBytes(path).SequenceEqual(new byte[] { 3, 4 }), "primary must contain the new bytes");
      Require(File.ReadAllBytes(path + ".bak").SequenceEqual(new byte[] { 1, 2 }), "primary must rotate to backup");
      Require(File.ReadAllBytes(path + ".bak2").SequenceEqual(new byte[] { 9 }), "existing backup must rotate after validation");
      Require(encoder.Called, "save must call the encoder at the capture/encode boundary");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static void TestWorldSaveValidationFailurePreservesPreviousFiles()
  {
    string directory = Directory.CreateTempSubdirectory("nltx-p14-world-save-fail-").FullName;
    try
    {
      string path = Path.Combine(directory, "world.wld");
      File.WriteAllBytes(path, new byte[] { 5, 6 });
      File.WriteAllBytes(path + ".bak", new byte[] { 7 });
      FilePlatformAdapter platform = new();
      WorldSaveTransactionSystem system = new(platform);
      WorldSaveCommand command = new(path, isCloudSave: false, new WorldBackupPolicy(1));
      RecordingWorldSaveEncoder encoder = new(new byte[] { 8, 9 });
      WorldSaveValidationQuery validation = new(_ => false);

      WorldSaveProjection projection = system.Save(command, encoder, validation);

      Require(!projection.Committed, "invalid read-back must not commit");
      Require(projection.Failure.Kind == FilePlatformFailureKind.InvalidData, "validation failure must be classified");
      Require(File.ReadAllBytes(path).SequenceEqual(new byte[] { 5, 6 }), "validation failure must restore the old primary");
      Require(File.ReadAllBytes(path + ".bak").SequenceEqual(new byte[] { 7 }), "validation failure must preserve the old backup");
      Require(!projection.Stages.Contains(WorldSaveStage.Backup), "validation failure must not rotate backups");
      Require(!projection.Stages.Contains(WorldSaveStage.Publish), "validation failure must not publish success");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static void TestPlayerSaveSessionClockAndPolicyBoundary()
  {
    FakePlayerSaveClock clock = new();
    PlayerSaveSessionComponent session = new("Alice", "players/Alice.plr", isCloudSave: false);
    session.SetPlayTime(TimeSpan.FromMinutes(5));
    PlayerSaveSystem system = new(session, clock, new FilePlatformAdapter());

    system.Start();
    system.Start();
    clock.Advance(TimeSpan.FromSeconds(3));
    system.Update();
    Require(session.PlayTime == TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(3), "active timer must accumulate elapsed time");

    system.Stop();
    clock.Advance(TimeSpan.FromSeconds(4));
    system.Update();
    Require(session.PlayTime == TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(3), "stopped timer must not accumulate time");
    Require(!session.IsTimerActive, "stop must clear the transient timer state");

    string directory = Directory.CreateTempSubdirectory("nltx-p14-player-save-").FullName;
    try
    {
      string path = Path.Combine(directory, "Alice.plr");
      PlayerSaveCommand command = new(path, isCloudSave: false, new PlayerSavePolicy(ServerSideCharacter: false));
      RecordingPlayerSaveEncoder encoder = new(new byte[] { 1, 2, 3 });
      PlayerSaveOutcome outcome = system.Save(command, encoder);
      Require(outcome.Committed, "normal player save must commit encoded bytes");
      Require(File.ReadAllBytes(path).SequenceEqual(new byte[] { 1, 2, 3 }), "player save must use the platform adapter");

      PlayerSaveCommand serverCommand = new(path + ".server", isCloudSave: false, new PlayerSavePolicy(ServerSideCharacter: true));
      RecordingPlayerSaveEncoder serverEncoder = new(new byte[] { 4 });
      PlayerSaveOutcome skipped = system.Save(serverCommand, serverEncoder);
      Require(skipped.SkippedByPolicy, "server-side character policy must skip file serialization");
      Require(!serverEncoder.Called, "server-side policy must be checked before encoding");
      Require(!File.Exists(serverCommand.Path), "skipped player save must not create a file");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static void TestWorldIdentitySnapshotKeepsLegacyAndGuidNamesSeparate()
  {
    Guid uniqueId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    WorldDescriptorPersistenceSnapshot legacy = new(
      worldId: 42,
      uniqueId,
      worldGeneratorVersion: 777389080576UL,
      seedText: "12345",
      creationTime: DateTime.UnixEpoch,
      lastPlayed: DateTime.UnixEpoch.AddDays(1),
      worldSizeX: 4200,
      worldSizeY: 1200,
      gameMode: 1);
    WorldDescriptorPersistenceSnapshot guidNamed = legacy with
    {
      WorldGeneratorVersion = 777389080577UL
    };

    Require(legacy.MapFileName == "42", "legacy worlds must keep the numeric map name");
    Require(guidNamed.MapFileName == uniqueId.ToString(), "new worlds must use the GUID map name");
    Require(legacy.WorldSizeName == "Small", "known dimensions must project to the Version4 size name");
    Require(legacy.TryGetNumericSeed(out int seed) && seed == 12345, "numeric seed text must remain queryable");

    RequireThrows<ArgumentOutOfRangeException>(
      () => new WorldDescriptorPersistenceSnapshot(
        1,
        Guid.Empty,
        1,
        new string('x', 41),
        DateTime.UnixEpoch,
        DateTime.UnixEpoch,
        1,
        1,
        0),
      "overlong seed text must be rejected before persistence");
    RequireThrows<ArgumentOutOfRangeException>(
      () => new WorldDescriptorPersistenceSnapshot(
        1,
        Guid.Empty,
        1,
        "seed",
        DateTime.UnixEpoch,
        DateTime.UnixEpoch,
        0,
        1,
        0),
      "invalid dimensions must be rejected before persistence");
  }

  private static void TestWorldValidityAndRulesRemainReadOnlyHandoffs()
  {
    WorldValiditySnapshot valid = new(WorldLoadStatus.Ok, detail: null);
    WorldValiditySnapshot malformed = new(WorldLoadStatus.Malformed, "bad header\r\nsecret path");
    Require(WorldValidityQuery.IsPlayable(valid), "ok load status must be playable");
    Require(!WorldValidityQuery.IsPlayable(malformed), "malformed load status must not be playable");
    Require(malformed.Diagnostic == "bad header secret path", "diagnostics must be sanitized before projection");

    WorldRulesHandoff rules = new(
      gameMode: 2,
      drunkWorld: true,
      notTheBees: false,
      forTheWorthy: true,
      anniversary: false,
      dontStarve: false,
      remixWorld: false,
      noTrapsWorld: false,
      zenithWorld: true,
      skyblockWorld: false,
      hasCorruption: true,
      isHardMode: true,
      defeatedMoonlord: false,
      seedOptionsInOrder: new[] { "drunk", "worthy", "zenith" });

    Require(rules.SerializedSeedSum == 1 + 4 + 128, "seed flags must retain Version4 bit weights");
    Require(!rules.HasCrimson, "corruption must be the single evil source");
    Require(rules.SeedOptionsInOrder.Count == 3, "seed option handoff must be materialized");
    RequireThrows<NotSupportedException>(
      () => ((IList<string>)rules.SeedOptionsInOrder)[0] = "changed",
      "seed option projection must not expose a mutable list");
  }

  private static void TestWorldTileHeaderCodecRoundTripAndMalformedInput()
  {
    WorldTileHeaderCoreValue core = new(
      active: true,
      tileType: 300,
      wall: true,
      wallType: 400,
      liquid: TileLiquidKind.Lava,
      liquidAmount: 123,
      wire1: true,
      wire2: false,
      wire3: true,
      slope: 3,
      runLength: 300);
    WorldTileHeaderExtensionValue extension = new(
      actuator: true,
      inactive: false,
      wire4: true,
      tileColor: 5,
      wallColor: 6,
      invisibleBlock: true,
      invisibleWall: false,
      fullbrightBlock: false,
      fullbrightWall: true,
      shimmer: false,
      reservedHeader4Bits: 0xA0);

    byte[] bytes = WorldTileHeaderCodec.Encode(core, extension);
    WorldTileHeaderValue decoded = WorldTileHeaderCodec.Decode(bytes);
    Require(decoded.Core == core, "tile header core flags must round trip");
    Require(decoded.Extension == extension, "tile header extension flags must round trip");
    Require((bytes[0] & WorldTileHeaderCoreCodec.Header1Continuation) != 0, "header one must announce header two");
    Require((bytes[2] & WorldTileHeaderExtensionCodec.Header3Continuation) != 0, "header three must announce header four");
    RequireThrows<FormatException>(
      () => WorldTileHeaderCodec.Decode(bytes[..^1]),
      "truncated tile headers must be rejected");
    RequireThrows<ArgumentOutOfRangeException>(
      () => new WorldTileHeaderCoreValue(
        active: true,
        tileType: 1,
        wall: false,
        wallType: 0,
        liquid: TileLiquidKind.None,
        liquidAmount: 0,
        wire1: false,
        wire2: false,
        wire3: false,
        slope: 8,
        runLength: 0),
      "slope values outside the three-bit protocol must be rejected");
  }

  private static void TestWorldRecoveryUsesBoundedBackupFallback()
  {
    string directory = Directory.CreateTempSubdirectory("nltx-p14-world-recovery-").FullName;
    try
    {
      string path = Path.Combine(directory, "world.wld");
      File.WriteAllBytes(path, new byte[] { 1 });
      File.WriteAllBytes(path + ".bak", new byte[] { 2 });
      int validationCalls = 0;
      WorldRecoveryTransactionAdapter recovery = new(
        new FilePlatformAdapter(),
        bytes =>
        {
          validationCalls++;
          return bytes.Span.SequenceEqual(new byte[] { 2 });
        });

      WorldRecoveryOutcome recovered = recovery.Load(
        path,
        isCloudSave: false,
        new WorldRecoveryPolicy(normalAttempts: 2, backupAttempts: 1));

      Require(recovered.Status == WorldRecoveryStatus.RecoveredFromBackup, "valid backup must recover a failed primary");
      Require(recovered.CanPublishWorldLoaded, "only validated recovery may open the publication gate");
      Require(File.ReadAllBytes(path).SequenceEqual(new byte[] { 2 }), "recovery must replace primary with validated backup bytes");
      Require(validationCalls == 4, "normal and backup validation attempts must remain bounded");

      File.WriteAllBytes(path, new byte[] { 3 });
      File.WriteAllBytes(path + ".bak", new byte[] { 4 });
      WorldRecoveryTransactionAdapter failedRecovery = new(
        new FilePlatformAdapter(),
        _ => false);
      WorldRecoveryOutcome failed = failedRecovery.Load(
        path,
        isCloudSave: false,
        new WorldRecoveryPolicy(normalAttempts: 1, backupAttempts: 1));
      Require(!failed.CanPublishWorldLoaded, "invalid primary and backup must not publish WorldLoaded");
      Require(File.ReadAllBytes(path).SequenceEqual(new byte[] { 3 }), "failed recovery must preserve primary bytes");
      Require(File.ReadAllBytes(path + ".bak").SequenceEqual(new byte[] { 4 }), "failed recovery must preserve backup bytes");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private static void TestWorldTemporaryEventContextCopiesAndRestoresTransientState()
  {
    List<int> celebrating = new() { 3, 5 };
    WorldTemporaryEventContext context = new(
      time: 12.5,
      raining: true,
      maxRain: 0.5f,
      rainTime: 10,
      dayTime: false,
      bloodMoon: true,
      eclipse: false,
      moonPhase: 2,
      cultistDelay: 100,
      partyGenuine: true,
      partyManual: false,
      partyCooldown: 4,
      celebratingNpcIds: celebrating,
      sandstormHappening: true,
      sandstormTimeLeft: 20,
      sandstormSeverity: 0.3f,
      sandstormIntendedSeverity: 0.4f,
      lanternNightGenuine: false,
      lanternNightManual: true,
      lanternNightNextNightIsGenuine: true,
      lanternNightCooldown: 2,
      coinRain: 1,
      meteorShowerCount: 2);
    celebrating.Add(7);
    Require(context.CelebratingNpcIds.Count == 2, "temporary event context must copy mutable NPC ids");

    RecordingTemporaryEventSink sink = new();
    WorldTemporaryEventContextSystem system = new();
    WorldTemporaryEventRestoreResult restore = system.Restore(context, sink);
    Require(restore.Applied, "temporary event context restore must report application");
    Require(sink.LastContext == context, "restore must pass the complete context to the sink");

    WorldTemporaryEventContext normalReset = WorldTemporaryEventContext.Reset(
      new WorldResetTimePolicy(GraveyardBloodmoonEnabled: false, TenthAnniversaryWorld: true, SkyblockWorld: false));
    Require(normalReset.DayTime && normalReset.Time == 13500.0, "normal reset must restore Version4 day-time defaults");
    Require(normalReset.PartyGenuine, "tenth anniversary reset must keep the genuine party exception");

    WorldTemporaryEventContext graveyardReset = WorldTemporaryEventContext.Reset(
      new WorldResetTimePolicy(GraveyardBloodmoonEnabled: true, TenthAnniversaryWorld: true, SkyblockWorld: false));
    Require(!graveyardReset.DayTime && graveyardReset.BloodMoon && graveyardReset.Time == 1.0, "graveyard reset must keep the blood moon exception");

  }

  private static void TestConfigurationAdapterPreservesCallbackOrderAndFormats()
  {
    string directory = Directory.CreateTempSubdirectory("nltx-p14-config-").FullName;
    try
    {
      FilePlatformAdapter platform = new();
      string jsonPath = Path.Combine(directory, "preferences.json");
      PreferencesStoreAdapter preferences = new(platform, jsonPath, useBson: false);
      List<string> events = new();
      preferences.OnSave += _ => events.Add("save");
      preferences.OnLoad += _ => events.Add("load");
      preferences.OnProcessText += (ref string text) =>
      {
        events.Add("process");
        text = text.Replace("Alice", "Alice", StringComparison.Ordinal);
      };
      Require(preferences.Put("Name", "Alice").Succeeded, "preference mutation without autosave must succeed");
      Require(preferences.Save().Succeeded, "JSON preferences save must succeed");
      Require(events.SequenceEqual(new[] { "save", "process" }), "JSON callback order must be save then process");

      PreferencesStoreAdapter loaded = new(platform, jsonPath, useBson: false);
      loaded.OnLoad += _ => events.Add("load");
      Require(loaded.Load().Loaded, "JSON preferences load must succeed");
      Require(loaded.Get("Name", "") == "Alice", "JSON preferences must round trip typed values");
      Require(events.Last() == "load", "load callback must run after document replacement");

      string bsonPath = Path.Combine(directory, "preferences.dat");
      PreferencesStoreAdapter bson = new(platform, bsonPath, useBson: true);
      bson.Put("Enabled", true);
      Require(bson.Save().Succeeded, "BSON preferences save must use an explicit format branch");
      PreferencesStoreAdapter bsonLoaded = new(platform, bsonPath, useBson: true);
      Require(bsonLoaded.Load().Loaded, "BSON preferences load must succeed");
      Require(bsonLoaded.Get("Enabled", false), "BSON preferences must round trip booleans");

      ConfigurationAdapter configuration = new(new Dictionary<string, object?> { ["Count"] = 3L });
      Require(configuration.Get("Count", 0) == 3, "configuration adapter must provide typed read-only lookup");

      string directoryPath = Path.Combine(directory, "existing-directory");
      Directory.CreateDirectory(directoryPath);
      PreferencesStoreAdapter failing = new(platform, directoryPath, useBson: false)
      {
        AutoSave = true
      };
      PreferencesSaveResult failedSave = failing.Put("WillFail", 1);
      Require(!failedSave.Succeeded, "autosave write failure must be visible to the caller");
    }
    finally
    {
      Directory.Delete(directory, recursive: true);
    }
  }

  private sealed class RecordingTemporaryEventSink : IWorldTemporaryEventSink
  {
    public WorldTemporaryEventContext? LastContext { get; private set; }

    public void Apply(WorldTemporaryEventContext context)
    {
      LastContext = context;
    }
  }

  private sealed class FakePlayerSaveClock : IPlayerSaveClock
  {
    public TimeSpan Now { get; private set; }

    public void Advance(TimeSpan elapsed)
    {
      Now += elapsed;
    }
  }

  private sealed class RecordingPlayerSaveEncoder : IPlayerSaveEncoder
  {
    private readonly byte[] _bytes;

    public RecordingPlayerSaveEncoder(byte[] bytes)
    {
      _bytes = bytes;
    }

    public bool Called { get; private set; }

    public PlayerSaveEncodeResult Encode(PlayerSaveSessionSnapshot session, PlayerSaveCommand command)
    {
      Called = true;
      return PlayerSaveEncodeResult.Success(_bytes);
    }
  }

  private sealed class RecordingWorldSaveEncoder : IWorldSaveEncoder
  {
    private readonly byte[] _bytes;

    public RecordingWorldSaveEncoder(byte[] bytes)
    {
      _bytes = bytes;
    }

    public bool Called { get; private set; }

    public WorldSaveEncodeResult Encode(WorldSaveCommand command)
    {
      Called = true;
      return WorldSaveEncodeResult.Success(_bytes);
    }
  }

  private sealed class FakeCloudFileStore : ICloudFileStore
  {
    public string? LastReadPath { get; private set; }

    public bool Exists(string path)
    {
      return true;
    }

    public byte[] ReadAllBytes(string path)
    {
      LastReadPath = path;
      return new byte[] { 7 };
    }

    public bool WriteAllBytes(string path, ReadOnlyMemory<byte> data)
    {
      return true;
    }

    public bool Copy(string sourcePath, string destinationPath)
    {
      return true;
    }

    public bool Move(string sourcePath, string destinationPath)
    {
      return true;
    }

    public bool Delete(string path)
    {
      return true;
    }
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private static void RequireThrows<TException>(
    Action action,
    string message)
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
}
