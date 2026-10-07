using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using EntityEcs;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.IO;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Relationships;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;
using Terraria.WorldInteraction.TileEntities;
using RuntimeMain = NSSLC.WorldGeneration.Main;
using RuntimeTileEntity = NSSLC.WorldGeneration.TileEntity;
using RuntimeWorldGen = NSSLC.WorldGeneration.WorldGen;
using RuntimeBirthdayParty = NSSLC.WorldGeneration.BirthdayParty;
using RuntimeDd2Event = NSSLC.WorldGeneration.DD2Event;
using RuntimeLanternNight = NSSLC.WorldGeneration.LanternNight;
using RuntimeNpc = NSSLC.WorldGeneration.NPC;
using RuntimeSandstorm = NSSLC.WorldGeneration.Sandstorm;

/// <summary>
/// Verifies rollback after a real WorldFile candidate has reached late finalization.
/// The production WorldFile decoder, generated API catalog and load/recovery coordinator are used.
/// </summary>
internal static class GeneratedWorldRollbackVerification
{
  private const ushort ActiveTileFlag = 0x20;
  private const ushort SignTileType = 55;
  private const ushort PressurePlateTileType = 135;
  private const ushort LogicSensorTileType = 423;
  private const string FixtureSignText = "Entity rollback fixture sign";

  private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
  {
    IncludeFields = true
  };

  public static void Run(string smallWorldPath, string mediumWorldPath)
  {
    string smallInputPath = Path.GetFullPath(smallWorldPath);
    string mediumPath = Path.GetFullPath(mediumWorldPath);
    Require(File.Exists(smallInputPath), $"Small WorldFile does not exist: {smallInputPath}");
    Require(File.Exists(mediumPath), $"Medium WorldFile does not exist: {mediumPath}");
    Require(!string.Equals(smallInputPath, mediumPath, StringComparison.OrdinalIgnoreCase),
      "Small and medium WorldFile paths must be different.");
    string smallPath = PrepareEnrichedWorldFile(smallInputPath);
    mediumPath = PrepareEnrichedWorldFile(mediumPath);
    Require(!string.Equals(smallPath, mediumPath, StringComparison.OrdinalIgnoreCase),
      "Enriched small and medium WorldFile paths must be different.");
    string rollbackDiagnosticsDirectory = Path.Combine(
      FindRepositoryRoot(),
      "Build",
      "diagnostics",
      "EntityOrganization",
      "real-world-rollback",
      $"rollback-diagnostics-{DateTime.UtcNow.Ticks}");
    Directory.CreateDirectory(rollbackDiagnosticsDirectory);
    string previousSnapshotPath = Path.Combine(
      rollbackDiagnosticsDirectory,
      "previous-legacy-snapshot.json");
    string currentSnapshotPath = Path.Combine(
      rollbackDiagnosticsDirectory,
      "current-restored-legacy-snapshot.json");
    Console.WriteLine($"Rollback diagnostics directory: {rollbackDiagnosticsDirectory}");

    RuntimeMain.InitializeHeadlessRuntime();
    RuntimeMain.rand = new NSSLC.WorldGeneration.Utilities.UnifiedRandom(319);
    var hostIdentityRegistry = new EntityIdentityRegistry();
    var sessions = new List<LoadedWorldSession>();
    LoadedWorldSession? previousSession = null;
    LoadedWorldSession? failedCandidate = null;
    LoadedWorldSession? restoredSession = null;
    EntityRuntime? failedCandidateRuntime = null;
    TileEntityStore? failedCandidateTileEntityStore = null;
    WorldPressurePlateRegistryComponent? failedCandidatePressurePlateStore = null;
    EntityReference previousReference = EntityReference.None;
    RuntimeEntityHandle previousHandle = default;
    TileEntityId previousTileEntityId = default;
    int previousRootCount = 0;
    int failedCandidateInitialRootCount = 0;
    string? previousRuntimeTileEntitySnapshot = null;
    string? previousLegacySnapshot = null;
    WorldLoadRecoveryResult? loadResult = null;
    Exception? loadException = null;
    int candidatePublishCount = 0;
    int finalizeCount = 0;
    int previousReprojectCount = 0;
    int smallWidth = 0;
    int smallHeight = 0;
    int mediumWidth = 0;
    int mediumHeight = 0;

    WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap(
      WorldStorageCoordinatorFactory.CreateWorldStorageIoGate(),
      publishRemainingWorldState: PublishCandidate,
      preparePreReleaseWeather: CompleteWithoutCancellation,
      finalizeLoadedWorld: FinalizeCandidate,
      resultObserver: result => loadResult = result,
      exceptionObserver: exception => loadException = exception,
      sessionFactory: CreateSession,
      reprojectPreviousWorldState: ReprojectPrevious);

    RuntimeMain.worldPathName = smallPath;
    new WorldFileData(Path.GetFileNameWithoutExtension(smallPath)).SetAsActive();
    RuntimeMain.LoadWorld();

    Require(loadException is null, "The initial small-world load threw an exception: " + loadException);
    Require(loadResult?.Succeeded == true, "The initial small-world load did not succeed.");
    previousSession = RequireSession(sessions, 0, "initial small-world load");
    smallWidth = previousSession.World.Descriptor.SizeX;
    smallHeight = previousSession.World.Descriptor.SizeY;
    Require(ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        previousSession) && previousSession.IsPublished && !previousSession.IsDisposed,
      "The small world must be the active published session.");
    Require(!RuntimeWorldGen.isGeneratingOrLoadingWorld,
      "The initial small-world load must release the world-load gate.");
    VerifyWorldHasRestorableProjection(previousSession, "enriched small");
    CaptureRootEvidence(previousSession, out previousTileEntityId, out previousReference,
      out previousHandle);
    previousRootCount = previousSession.EntityRuntime.EntityCount;
    previousRuntimeTileEntitySnapshot = Serialize(RuntimeTileEntity.CreatePersistenceSnapshot());
    previousLegacySnapshot = CaptureLegacySnapshot(previousSession);
    File.WriteAllText(previousSnapshotPath, previousLegacySnapshot);

    loadResult = null;
    loadException = null;
    RuntimeMain.worldPathName = mediumPath;
    RuntimeMain.LoadWorld();
    Console.WriteLine(Serialize(new
    {
      loadResult?.Failure,
      loadResult?.CleanupFailure,
      loadResult?.TerminalAction,
      Gate = RuntimeWorldGen.isGeneratingOrLoadingWorld,
      previousReprojectCount
    }));

    Require(loadException is null, "The controlled medium-world rollback threw an exception: " +
      loadException);
    Require(loadResult is not null && !loadResult.Succeeded,
      "The controlled late-finalize failure must fail the medium-world load.");
    Require(failedCandidate?.IsDisposed == true,
      "The failed medium candidate must be disposed after rollback.");
    Require(failedCandidateRuntime is not null && failedCandidateRuntime.EntityCount == 0,
      $"Factory cleanup must remove every failed-candidate EntityRuntime root; " +
      $"the candidate held {failedCandidateInitialRootCount} before rollback.");
    Require(failedCandidateInitialRootCount > 0,
      "The enriched medium candidate must own at least one real TileEntity runtime root before " +
      "the controlled failure.");
    Require(failedCandidateTileEntityStore is not null &&
        ThrowsObjectDisposed(() => _ = failedCandidateTileEntityStore.Count) &&
        ThrowsObjectDisposed(() => failedCandidateTileEntityStore.CommitRuntimeSnapshot(
          new TileEntityStoreSnapshot(Array.Empty<TileEntitySnapshot>(), nextId: 0))),
        "Factory cleanup must dispose the failed candidate's TileEntity store for reads and writes.");
    Require(failedCandidatePressurePlateStore is not null &&
        ThrowsObjectDisposed(() => failedCandidatePressurePlateStore.Replace(
          Array.Empty<TileCoordinate>())),
      "Factory cleanup must dispose the failed candidate's pressure-plate store for writes.");
    Require(ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        previousSession) && !previousSession.IsDisposed && previousSession.IsPublished,
      "Rollback must restore the original small session as the active published session.");
    Require(!RuntimeWorldGen.isGeneratingOrLoadingWorld,
      "A successful rollback must release the world-load gate.");
    Require(candidatePublishCount == 2 && finalizeCount == 2 && previousReprojectCount == 1,
      "The failing load must publish two candidates, fail its second finalize, and reproject " +
      "the previous session once.");
    Require(ReferenceEquals(restoredSession, previousSession),
      "The independent restore callback must receive the original session object.");
    VerifyRootEvidence(previousSession, previousTileEntityId, previousReference, previousHandle,
      previousRootCount, previousRuntimeTileEntitySnapshot!);
    Require(CaptureLegacySnapshot(previousSession) == previousLegacySnapshot,
      "Legacy WorldFile metadata, tile map, core time/weather/events, background styles, " +
      "TreeTops or " +
      "TileEntity/chest/sign/pressure-plate projection changed after rollback.");

    loadResult = null;
    loadException = null;
    RuntimeMain.worldPathName = mediumPath;
    RuntimeMain.LoadWorld();

    Require(loadException is null, "The medium-world retry threw an exception: " + loadException);
    Require(loadResult?.Succeeded == true, "The medium-world retry did not succeed.");
    LoadedWorldSession nextSession = RequireSession(sessions, 2, "medium-world retry");
    mediumWidth = nextSession.World.Descriptor.SizeX;
    mediumHeight = nextSession.World.Descriptor.SizeY;
    Require(mediumWidth > smallWidth && mediumHeight > smallHeight,
      "The supplied WorldFiles must be a Terraria small world followed by a medium world.");
    Require(ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        nextSession) && nextSession.IsPublished && !nextSession.IsDisposed,
      "The successful medium retry must publish its new session.");
    Require(previousSession.IsDisposed,
      "The successful retry must retire the previous small-world session.");
    Require(!nextSession.EntityRuntime.TryResolve(previousReference, out _),
      "A reference from the retired small-world runtime must not resolve in the retry session.");
    Require(candidatePublishCount == 3 && finalizeCount == 3 && previousReprojectCount == 1,
      "The successful retry must publish and finalize once without another previous-session " +
      "reprojection.");
    Require(!RuntimeWorldGen.isGeneratingOrLoadingWorld,
      "The successful medium retry must release the world-load gate.");

    Console.WriteLine(
      "Real WorldFile rollback verification passed: " +
      $"small={smallWidth}x{smallHeight}, medium={mediumWidth}x{mediumHeight}, " +
      $"candidatePublishes={candidatePublishCount}, previousReprojects={previousReprojectCount}.");

    WorldStorageOperationResult PublishCandidate(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      candidatePublishCount++;
      if (cancellationToken.IsCancellationRequested)
      {
        return Canceled("Candidate publication was canceled.");
      }

      if (candidatePublishCount == 2)
      {
        failedCandidate = session;
        mediumWidth = session.World.Descriptor.SizeX;
        mediumHeight = session.World.Descriptor.SizeY;
        Require(session.IsComplete && session.IsPublicationUncertain && !session.IsPublished,
          "The medium candidate must be complete and publication-uncertain at its host callback.");
        Require(RuntimeWorldGen.isGeneratingOrLoadingWorld,
          "The candidate publication callback must run under the world-load gate.");
        VerifyWorldHasRestorableProjection(session, "enriched medium");
      }

      return WorldStorageOperationResult.Success;
    }

    WorldStorageOperationResult FinalizeCandidate(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      finalizeCount++;
      if (cancellationToken.IsCancellationRequested)
      {
        return Canceled("Candidate finalization was canceled.");
      }

      if (finalizeCount == 2)
      {
        Require(ReferenceEquals(failedCandidate, session) && session.IsPublished &&
            ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session),
          "The controlled failure must happen after the medium candidate was staged as active.");
        failedCandidateRuntime = session.EntityRuntime;
        failedCandidateInitialRootCount = session.EntityRuntime.EntityCount;
        failedCandidateTileEntityStore = session.Storage.TileEntities;
        failedCandidatePressurePlateStore = session.Storage.PressurePlates;
        return WorldStorageOperationResult.Failed(WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "Controlled late-finalize failure for real WorldFile rollback verification."));
      }

      return WorldStorageOperationResult.Success;
    }

    WorldStorageOperationResult ReprojectPrevious(LoadedWorldSession session)
    {
      previousReprojectCount++;
      restoredSession = session;
      if (previousReprojectCount != 1 || !ReferenceEquals(session, previousSession) ||
          !session.IsPublished || session.IsDisposed ||
          !ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, session) ||
          !RuntimeWorldGen.isGeneratingOrLoadingWorld)
      {
        return Invalid("The rollback callback did not receive the active old session under the gate.");
      }

      File.WriteAllText(previousSnapshotPath, previousLegacySnapshot!);
      string currentLegacySnapshot;
      try
      {
        currentLegacySnapshot = CaptureLegacySnapshot(session);
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
        throw;
      }

      File.WriteAllText(currentSnapshotPath, currentLegacySnapshot);
      if (!HasSameRootEvidence(session, previousTileEntityId, previousReference, previousHandle,
            previousRootCount, previousRuntimeTileEntitySnapshot!) ||
          currentLegacySnapshot != previousLegacySnapshot)
      {
        return Invalid("The old session root or built-in legacy projection changed before host " +
          "reprojection.");
      }

      return HasSameRootEvidence(session, previousTileEntityId, previousReference,
          previousHandle, previousRootCount, previousRuntimeTileEntitySnapshot!)
        ? WorldStorageOperationResult.Success
        : Invalid("Host restoration changed an existing EntityReference or RuntimeEntityHandle.");
    }

    LoadedWorldSession CreateSession()
    {
      var session = new LoadedWorldSession(hostIdentityRegistry);
      sessions.Add(session);
      return session;
    }
  }

  private static WorldStorageOperationResult CompleteWithoutCancellation(
    LoadedWorldSession session,
    CancellationToken cancellationToken)
  {
    return cancellationToken.IsCancellationRequested
      ? Canceled("World-load preparation was canceled.")
      : WorldStorageOperationResult.Success;
  }

  private static string PrepareEnrichedWorldFile(string sourcePath)
  {
    byte[] sourceBytes = File.ReadAllBytes(sourcePath);
    WorldPersistenceDocument sourceDocument = DecodeWorldFile(sourceBytes, sourcePath);
    WorldFileDocumentValidator validator = new();
    WorldStorageFailure sourceValidation = validator.Validate(sourceDocument);
    Require(sourceValidation.Kind == WorldStorageFailureKind.None,
      $"The small source WorldFile is invalid: {sourceValidation.Kind}: {sourceValidation.Detail}");

    WorldFileEnvironmentSection environment = GetRequired<WorldFileEnvironmentSection>(
      sourceDocument,
      WorldFileEnvironmentSection.SectionId);
    WorldFileTilePayloadSection sourceTilePayload = GetRequired<WorldFileTilePayloadSection>(
      sourceDocument,
      WorldFileTilePayloadSection.SectionId);
    WorldFileTileCodec tileCodec = new();
    TileMapSnapshot sourceTiles = tileCodec.Decode(sourceTilePayload);
    TileCellState[] tileCells = CopyTiles(sourceTiles);

    WorldFileTileEntityCodec tileEntityCodec = new();
    WorldFileTileEntitySection sourceTileEntities = GetTileEntitySection(sourceDocument);
    List<TileEntitySnapshot> tileEntities = tileEntityCodec.Decode(sourceTileEntities).ToList();
    WorldFileSignSection sourceSigns = GetOptional(
      sourceDocument,
      WorldFileSignSection.SectionId,
      WorldFileSignSection.Empty);
    WorldFilePressurePlateSection sourcePressurePlates = GetOptional(
      sourceDocument,
      WorldFilePressurePlateSection.SectionId,
      WorldFilePressurePlateSection.Empty);

    RequireFrameImportant(sourceTilePayload.FrameImportant, SignTileType);
    RequireFrameImportant(sourceTilePayload.FrameImportant, PressurePlateTileType);
    RequireFrameImportant(sourceTilePayload.FrameImportant, LogicSensorTileType);

    Require(sourceTiles.Width >= 5 && sourceTiles.Height >= 5,
      "The source WorldFile is too small for the sign fixture rectangle.");
    int startX = Math.Clamp(environment.SpawnTileX, 1, sourceTiles.Width - 3);
    int startY = Math.Clamp(environment.SpawnTileY + 2, 1, sourceTiles.Height - 3);
    var reserved = new HashSet<TileCoordinate>(tileEntities.Select(entity => entity.Anchor));
    reserved.UnionWith(sourceSigns.Signs.Select(sign => new TileCoordinate(sign.X, sign.Y)));
    reserved.UnionWith(sourcePressurePlates.Plates.Select(plate =>
      new TileCoordinate(plate.X, plate.Y)));

    TileCoordinate logicSensorAnchor = FindInactiveAnchor(
      tileCells,
      sourceTiles.Width,
      sourceTiles.Height,
      startX,
      startY,
      reserved);
    reserved.Add(logicSensorAnchor);

    TileCoordinate pressurePlateAnchor = FindInactiveAnchor(
      tileCells,
      sourceTiles.Width,
      sourceTiles.Height,
      startX,
      startY,
      reserved);
    reserved.Add(pressurePlateAnchor);

    TileCoordinate signAnchor = FindInactiveRectangle(
      tileCells,
      sourceTiles.Width,
      sourceTiles.Height,
      startX,
      startY,
      reserved,
      rectangleWidth: 2,
      rectangleHeight: 2);
    ReserveRectangle(reserved, signAnchor, rectangleWidth: 2, rectangleHeight: 2);

    SetActiveTile(tileCells, sourceTiles.Height, logicSensorAnchor, LogicSensorTileType);
    SetActiveTile(tileCells, sourceTiles.Height, pressurePlateAnchor, PressurePlateTileType);
    SetSignTiles(tileCells, sourceTiles.Height, signAnchor);

    int nextTileEntityId = tileEntities.Count == 0
      ? 0
      : checked(tileEntities.Max(entity => entity.Id.Value) + 1);
    var fixtureTileEntity = new TileEntitySnapshot(
      new TileEntityId(nextTileEntityId),
      new TileEntityTypeId(2),
      logicSensorAnchor,
      Array.Empty<ItemState>(),
      logicCheck: (byte)LogicCheckType.PlayerAbove,
      logicOn: true);
    tileEntities.Add(fixtureTileEntity);

    var fixtureTileMap = new TileMapSnapshot(sourceTiles.Width, sourceTiles.Height, tileCells);
    WorldFileTilePayloadSection fixtureTilePayload = tileCodec.Encode(
      fixtureTileMap,
      sourceTilePayload.FrameImportant);
    WorldFileTileEntitySection fixtureTileEntitySection = tileEntityCodec.Encode(tileEntities);
    var fixtureSigns = new WorldFileSignSection(sourceSigns.Signs
      .Concat(new[] { new WorldFileSignRecord(FixtureSignText, signAnchor.X, signAnchor.Y) })
      .ToArray());
    var fixturePressurePlates = new WorldFilePressurePlateSection(sourcePressurePlates.Plates
      .Concat(new[] { new WorldFilePressurePlateRecord(
        pressurePlateAnchor.X,
        pressurePlateAnchor.Y) })
      .ToArray());

    WorldPersistenceDocument fixtureDocument = sourceDocument.WithReplacements(
      WorldPersistenceSection.Create(WorldFileTilePayloadSection.SectionId, fixtureTilePayload),
      WorldPersistenceSection.Create(WorldFileTileEntitySection.SectionId,
        fixtureTileEntitySection),
      WorldPersistenceSection.Create(WorldFileSignSection.SectionId, fixtureSigns),
      WorldPersistenceSection.Create(WorldFilePressurePlateSection.SectionId,
        fixturePressurePlates));
    WorldStorageFailure fixtureValidation = validator.Validate(fixtureDocument);
    Require(fixtureValidation.Kind == WorldStorageFailureKind.None,
      $"The enriched WorldFile is invalid: {fixtureValidation.Kind}: " +
      fixtureValidation.Detail);

    WorldSaveEncodeResult encoded = new WorldFileDocumentEncoder().Encode(fixtureDocument);
    Require(encoded.Succeeded,
      $"The enriched WorldFile could not be encoded: {encoded.Failure.Kind}: " +
      encoded.Failure.Detail);
    byte[] fixtureBytes = encoded.Bytes.ToArray();
    VerifyEnrichedSmallWorldDocument(
      fixtureBytes,
      sourceDocument,
      logicSensorAnchor,
      pressurePlateAnchor,
      signAnchor,
      fixtureTileEntity.Id);

    string sourceHash = Convert.ToHexString(SHA256.HashData(sourceBytes));
    string fixtureDirectory = Path.Combine(
      FindRepositoryRoot(),
      "Build",
      "diagnostics",
      "EntityOrganization",
      "real-world-rollback");
    Directory.CreateDirectory(fixtureDirectory);
    string fixturePath = Path.Combine(
      fixtureDirectory,
      $"{Path.GetFileNameWithoutExtension(sourcePath)}-{sourceHash[..12]}-enriched.wld");
    Require(!string.Equals(Path.GetFullPath(fixturePath), Path.GetFullPath(sourcePath),
        StringComparison.OrdinalIgnoreCase),
      "The enriched small-world output must not overwrite its source WorldFile.");
    if (File.Exists(fixturePath))
    {
      Require(SHA256.HashData(File.ReadAllBytes(fixturePath)).SequenceEqual(
          SHA256.HashData(fixtureBytes)),
        $"An existing enriched fixture differs from the deterministic output: {fixturePath}");
    }
    else
    {
      File.WriteAllBytes(fixturePath, fixtureBytes);
    }

    Console.WriteLine(
      $"Prepared real small WorldFile fixture: {fixturePath}; " +
      $"sign=({signAnchor.X},{signAnchor.Y}), " +
      $"logicSensor=({logicSensorAnchor.X},{logicSensorAnchor.Y}), " +
      $"pressurePlate=({pressurePlateAnchor.X},{pressurePlateAnchor.Y}).");
    return fixturePath;
  }

  private static void VerifyEnrichedSmallWorldDocument(
    byte[] fixtureBytes,
    WorldPersistenceDocument sourceDocument,
    TileCoordinate logicSensorAnchor,
    TileCoordinate pressurePlateAnchor,
    TileCoordinate signAnchor,
    TileEntityId fixtureTileEntityId)
  {
    WorldPersistenceDocument fixtureDocument = DecodeWorldFile(fixtureBytes, "enriched fixture");
    WorldStorageFailure validation = new WorldFileDocumentValidator().Validate(fixtureDocument);
    Require(validation.Kind == WorldStorageFailureKind.None,
      $"The encoded small fixture is invalid: {validation.Kind}: {validation.Detail}");

    WorldFileSignSection signs = GetRequired<WorldFileSignSection>(
      fixtureDocument,
      WorldFileSignSection.SectionId);
    WorldFilePressurePlateSection plates = GetRequired<WorldFilePressurePlateSection>(
      fixtureDocument,
      WorldFilePressurePlateSection.SectionId);
    IReadOnlyList<TileEntitySnapshot> entities = new WorldFileTileEntityCodec().Decode(
      GetRequired<WorldFileTileEntitySection>(fixtureDocument,
        WorldFileTileEntitySection.SectionId));
    Require(signs.Signs.Any(sign => sign.X == signAnchor.X && sign.Y == signAnchor.Y &&
        sign.Text == FixtureSignText),
      "Encoded small fixture is missing its named sign record.");
    Require(plates.Plates.Any(plate =>
        plate.X == pressurePlateAnchor.X && plate.Y == pressurePlateAnchor.Y),
      "Encoded small fixture is missing its pressure-plate record.");
    Require(entities.Any(entity => entity.Id == fixtureTileEntityId && entity.Type.Value == 2 &&
        entity.Anchor == logicSensorAnchor && entity.LogicOn &&
        entity.LogicCheck == (byte)LogicCheckType.PlayerAbove),
      "Encoded small fixture is missing its supported Logic Sensor TileEntity record.");

    TileMapSnapshot fixtureTiles = new WorldFileTileCodec().Decode(
      GetRequired<WorldFileTilePayloadSection>(fixtureDocument,
        WorldFileTilePayloadSection.SectionId));
    Require(IsActiveTile(fixtureTiles.GetTile(
          logicSensorAnchor.X, logicSensorAnchor.Y), LogicSensorTileType) &&
        IsActiveTile(fixtureTiles.GetTile(
          pressurePlateAnchor.X, pressurePlateAnchor.Y), PressurePlateTileType),
      "Encoded small fixture TileEntity or pressure-plate anchors are missing their tiles.");
    Require(IsActiveTile(fixtureTiles.GetTile(signAnchor.X, signAnchor.Y), SignTileType) &&
        IsActiveTile(fixtureTiles.GetTile(signAnchor.X + 1, signAnchor.Y), SignTileType) &&
        IsActiveTile(fixtureTiles.GetTile(signAnchor.X, signAnchor.Y + 1), SignTileType) &&
        IsActiveTile(fixtureTiles.GetTile(signAnchor.X + 1, signAnchor.Y + 1), SignTileType),
      "Encoded small fixture named sign is missing its framed sign tiles.");

    WorldFileChestSection sourceChests = GetRequired<WorldFileChestSection>(
      sourceDocument,
      WorldFileChestSection.SectionId);
    WorldFileChestSection fixtureChests = GetRequired<WorldFileChestSection>(
      fixtureDocument,
      WorldFileChestSection.SectionId);
    WorldFileNpcSection sourceNpcs = GetRequired<WorldFileNpcSection>(
      sourceDocument,
      WorldFileNpcSection.SectionId);
    WorldFileNpcSection fixtureNpcs = GetRequired<WorldFileNpcSection>(
      fixtureDocument,
      WorldFileNpcSection.SectionId);
    Require(sourceChests.Chests.Count == fixtureChests.Chests.Count &&
        CountNonemptyChestItems(sourceChests) == CountNonemptyChestItems(fixtureChests) &&
        sourceNpcs.TownNpcs.Count == fixtureNpcs.TownNpcs.Count &&
        sourceNpcs.SavedNpcs.Count == fixtureNpcs.SavedNpcs.Count,
      "Enriching the WorldFile must preserve its original chests, nonempty items and NPC " +
      "records.");
  }

  private static void VerifyWorldHasRestorableProjection(
    LoadedWorldSession session,
    string worldLabel)
  {
    IReadOnlyList<WorldChestSnapshot> chests = session.Storage.WorldContainers.CreateSnapshot();
    IReadOnlyList<WorldSignSnapshot> signs = session.Storage.WorldSigns.CreateSnapshot();
    IReadOnlyList<TileEntitySnapshot> tileEntities = session.Storage.TileEntities.CreateSnapshot();
    IReadOnlyList<TileCoordinate> pressurePlates = session.Storage.PressurePlates.Anchors;

    Require(chests.Count > 0 && chests.Any(chest => chest.Items.Any(item => !item.IsEmpty)),
      $"{worldLabel} WorldFile prerequisite failed: expected a chest with a nonempty item.");
    Require(signs.Count > 0 && signs.Any(sign => !string.IsNullOrWhiteSpace(sign.Text)),
      $"{worldLabel} WorldFile prerequisite failed: expected a sign with nonempty text.");
    Require(tileEntities.Count > 0,
      $"{worldLabel} WorldFile prerequisite failed: expected a supported TileEntity.");
    Require(pressurePlates.Count > 0,
      $"{worldLabel} WorldFile prerequisite failed: expected a registered pressure plate.");
    Require(session.EntityRuntime.EntityCount > 0,
      $"{worldLabel} WorldFile prerequisite failed: its loaded owners created no EntityRuntime " +
      "roots.");
  }

  private static WorldPersistenceDocument DecodeWorldFile(byte[] bytes, string label)
  {
    WorldPersistenceDecodeResult decoded = new WorldFileDocumentDecoder().Decode(bytes);
    Require(decoded.Succeeded && decoded.Document is not null,
      $"WorldFile '{label}' could not be decoded: {decoded.Failure.Kind}: " +
      decoded.Failure.Detail);
    return decoded.Document!;
  }

  private static TSection GetRequired<TSection>(
    WorldPersistenceDocument document,
    string sectionId)
    where TSection : notnull
  {
    Require(document.TryGetSection<TSection>(sectionId, out WorldLoadSection<TSection> section) &&
        section.IsPresent,
      $"Required WorldFile section '{sectionId}' is absent.");
    return section.Value;
  }

  private static TSection GetOptional<TSection>(
    WorldPersistenceDocument document,
    string sectionId,
    TSection fallback)
    where TSection : notnull
  {
    return document.TryGetSection<TSection>(sectionId, out WorldLoadSection<TSection> section) &&
        section.IsPresent
      ? section.Value
      : fallback;
  }

  private static WorldFileTileEntitySection GetTileEntitySection(
    WorldPersistenceDocument document)
  {
    return GetOptional(
      document,
      WorldFileTileEntitySection.SectionId,
      new WorldFileTileEntitySection(0, ReadOnlyMemory<byte>.Empty));
  }

  private static TileCellState[] CopyTiles(TileMapSnapshot source)
  {
    var cells = new TileCellState[checked(source.Width * source.Height)];
    for (int x = 0; x < source.Width; x++)
    {
      for (int y = 0; y < source.Height; y++)
      {
        cells[x * source.Height + y] = source.GetTile(x, y);
      }
    }

    return cells;
  }

  private static TileCoordinate FindInactiveAnchor(
    TileCellState[] cells,
    int width,
    int height,
    int startX,
    int startY,
    IReadOnlySet<TileCoordinate> reserved)
  {
    for (int radius = 0; radius < width + height; radius++)
    {
      for (int offsetX = -radius; offsetX <= radius; offsetX++)
      {
        int offsetY = radius - Math.Abs(offsetX);
        var candidate = new TileCoordinate(startX + offsetX, startY + offsetY);
        if (IsInactive(candidate, cells, width, height, reserved))
        {
          return candidate;
        }

        if (offsetY > 0)
        {
          candidate = new TileCoordinate(startX + offsetX, startY - offsetY);
          if (IsInactive(candidate, cells, width, height, reserved))
          {
            return candidate;
          }
        }
      }
    }

    throw new InvalidDataException("The source WorldFile has no inactive tile for the fixture.");
  }

  private static TileCoordinate FindInactiveRectangle(
    TileCellState[] cells,
    int width,
    int height,
    int startX,
    int startY,
    IReadOnlySet<TileCoordinate> reserved,
    int rectangleWidth,
    int rectangleHeight)
  {
    int maximumRadius = checked(width + height);
    for (int radius = 0; radius < maximumRadius; radius++)
    {
      for (int offsetX = -radius; offsetX <= radius; offsetX++)
      {
        int offsetY = radius - Math.Abs(offsetX);
        var candidate = new TileCoordinate(startX + offsetX, startY + offsetY);
        if (IsInactiveRectangle(candidate, cells, width, height, reserved,
              rectangleWidth, rectangleHeight))
        {
          return candidate;
        }

        if (offsetY > 0)
        {
          candidate = new TileCoordinate(startX + offsetX, startY - offsetY);
          if (IsInactiveRectangle(candidate, cells, width, height, reserved,
                rectangleWidth, rectangleHeight))
          {
            return candidate;
          }
        }
      }
    }

    throw new InvalidDataException("The source WorldFile has no free sign-sized tile rectangle.");
  }

  private static bool IsInactiveRectangle(
    TileCoordinate anchor,
    TileCellState[] cells,
    int width,
    int height,
    IReadOnlySet<TileCoordinate> reserved,
    int rectangleWidth,
    int rectangleHeight)
  {
    if (anchor.X < 0 || anchor.Y < 0 || anchor.X + rectangleWidth > width ||
        anchor.Y + rectangleHeight > height)
    {
      return false;
    }

    for (int offsetX = 0; offsetX < rectangleWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < rectangleHeight; offsetY++)
      {
        var coordinate = new TileCoordinate(anchor.X + offsetX, anchor.Y + offsetY);
        if (reserved.Contains(coordinate) ||
            IsActive(cells[coordinate.X * height + coordinate.Y]))
        {
          return false;
        }
      }
    }

    return true;
  }

  private static bool IsInactive(
    TileCoordinate coordinate,
    TileCellState[] cells,
    int width,
    int height,
    IReadOnlySet<TileCoordinate> reserved)
  {
    return (uint)coordinate.X < (uint)width && (uint)coordinate.Y < (uint)height &&
      !reserved.Contains(coordinate) && !IsActive(cells[coordinate.X * height + coordinate.Y]);
  }

  private static void ReserveRectangle(
    ISet<TileCoordinate> reserved,
    TileCoordinate anchor,
    int rectangleWidth,
    int rectangleHeight)
  {
    for (int offsetX = 0; offsetX < rectangleWidth; offsetX++)
    {
      for (int offsetY = 0; offsetY < rectangleHeight; offsetY++)
      {
        reserved.Add(new TileCoordinate(anchor.X + offsetX, anchor.Y + offsetY));
      }
    }
  }

  private static void SetActiveTile(
    TileCellState[] cells,
    int height,
    TileCoordinate coordinate,
    ushort tileType)
  {
    int index = coordinate.X * height + coordinate.Y;
    TileCellState tile = cells[index];
    tile.TileHeader = (ushort)(tile.TileHeader | ActiveTileFlag);
    tile.Type = tileType;
    tile.FrameX = 0;
    tile.FrameY = 0;
    cells[index] = tile;
  }

  private static void SetSignTiles(TileCellState[] cells, int height, TileCoordinate anchor)
  {
    SetActiveTile(cells, height, anchor, SignTileType);
    SetActiveTile(cells, height, new TileCoordinate(anchor.X + 1, anchor.Y), SignTileType);
    SetActiveTile(cells, height, new TileCoordinate(anchor.X, anchor.Y + 1), SignTileType);
    SetActiveTile(cells, height, new TileCoordinate(anchor.X + 1, anchor.Y + 1), SignTileType);
    SetTileFrame(cells, height, anchor, frameX: 0, frameY: 0);
    SetTileFrame(cells, height, new TileCoordinate(anchor.X + 1, anchor.Y),
      frameX: 18, frameY: 0);
    SetTileFrame(cells, height, new TileCoordinate(anchor.X, anchor.Y + 1),
      frameX: 0, frameY: 18);
    SetTileFrame(cells, height, new TileCoordinate(anchor.X + 1, anchor.Y + 1),
      frameX: 18, frameY: 18);
  }

  private static void SetTileFrame(
    TileCellState[] cells,
    int height,
    TileCoordinate coordinate,
    short frameX,
    short frameY)
  {
    int index = coordinate.X * height + coordinate.Y;
    TileCellState tile = cells[index];
    tile.FrameX = frameX;
    tile.FrameY = frameY;
    cells[index] = tile;
  }

  private static bool IsActive(TileCellState tile)
  {
    return (tile.TileHeader & ActiveTileFlag) != 0;
  }

  private static bool IsActiveTile(TileCellState tile, ushort expectedType)
  {
    return IsActive(tile) && tile.Type == expectedType;
  }

  private static void RequireFrameImportant(IReadOnlyList<bool> frameImportant, ushort tileType)
  {
    Require(tileType < frameImportant.Count && frameImportant[tileType],
      $"WorldFile tile type {tileType} is not frame-important and cannot back the fixture.");
  }

  private static int CountNonemptyChestItems(WorldFileChestSection section)
  {
    return section.Chests.Sum(chest => chest.Items.Count(item => item.Stack != 0 || item.Type != 0));
  }

  private static string FindRepositoryRoot()
  {
    DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
      if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
          Directory.Exists(Path.Combine(directory.FullName, "src")) &&
          Directory.Exists(Path.Combine(directory.FullName, "Test")))
      {
        return directory.FullName;
      }

      directory = directory.Parent;
    }

    throw new DirectoryNotFoundException(
      "Could not locate the NLTX repository root for the enriched WorldFile fixture.");
  }

  private static bool ThrowsObjectDisposed(Action action)
  {
    try
    {
      action();
      return false;
    }
    catch (ObjectDisposedException)
    {
      return true;
    }
  }

  private static void CaptureRootEvidence(
    LoadedWorldSession session,
    out TileEntityId tileEntityId,
    out EntityReference reference,
    out RuntimeEntityHandle handle)
  {
    IReadOnlyList<TileEntitySnapshot> tileEntities = session.Storage.TileEntities.CreateSnapshot();
    Require(tileEntities.Count > 0, "A real TileEntity root is required for identity evidence.");
    tileEntityId = tileEntities[0].Id;
    Require(session.Storage.TileEntities.TryGetEntityReference(tileEntityId, out reference),
      "The loaded TileEntity must expose its existing EntityReference.");
    Require(session.EntityRuntime.TryResolve(reference, out handle),
      "The loaded TileEntity EntityReference must resolve before rollback.");
  }

  private static void VerifyRootEvidence(
    LoadedWorldSession session,
    TileEntityId expectedTileEntityId,
    EntityReference expectedReference,
    RuntimeEntityHandle expectedHandle,
    int expectedRootCount,
    string expectedRuntimeTileEntitySnapshot)
  {
    Require(HasSameRootEvidence(session, expectedTileEntityId, expectedReference, expectedHandle,
        expectedRootCount, expectedRuntimeTileEntitySnapshot),
      "The previous session's EntityReference, RuntimeEntityHandle, root count or " +
      "runtime TileEntity state changed.");
  }

  private static bool HasSameRootEvidence(
    LoadedWorldSession session,
    TileEntityId expectedTileEntityId,
    EntityReference expectedReference,
    RuntimeEntityHandle expectedHandle,
    int expectedRootCount,
    string expectedRuntimeTileEntitySnapshot)
  {
    if (session.IsDisposed || session.EntityRuntime.EntityCount != expectedRootCount ||
        !session.Storage.TileEntities.TryGetEntityReference(
          expectedTileEntityId, out EntityReference currentReference) ||
        currentReference != expectedReference ||
        !session.EntityRuntime.TryResolve(currentReference, out RuntimeEntityHandle currentHandle) ||
        currentHandle != expectedHandle)
    {
      return false;
    }

    return Serialize(RuntimeTileEntity.CreatePersistenceSnapshot()) ==
      expectedRuntimeTileEntitySnapshot;
  }

  private static string CaptureLegacySnapshot(LoadedWorldSession session)
  {
    WorldFileData metadata = RuntimeMain.ActiveWorldFileData ??
      throw new InvalidOperationException("The loaded world-file metadata projection is missing.");
    NSSLC.WorldGeneration.Tile[,] tiles = RuntimeMain.tile ??
      throw new InvalidOperationException("The legacy Main tile map is missing.");
    Require(tiles.GetLength(0) == RuntimeMain.maxTilesX &&
        tiles.GetLength(1) == RuntimeMain.maxTilesY,
      "Legacy Main tile dimensions do not match their allocated map.");

    return Serialize(new
    {
      TileMap = new
      {
        RuntimeMain.maxTilesX,
        RuntimeMain.maxTilesY,
        RuntimeMain.maxSectionsX,
        RuntimeMain.maxSectionsY,
        RuntimeMain.leftWorld,
        RuntimeMain.rightWorld,
        RuntimeMain.topWorld,
        RuntimeMain.bottomWorld,
        RuntimeMain.UnderworldLayer,
        RuntimeMain.worldSurface,
        RuntimeMain.rockLayer,
        RuntimeMain.spawnTileX,
        RuntimeMain.spawnTileY,
        RuntimeMain.dungeonX,
        RuntimeMain.dungeonY,
        RuntimeMain.isThereAWorldSurface,
        ArrayWidth = tiles.GetLength(0),
        ArrayHeight = tiles.GetLength(1),
        Sha256 = HashLegacyTiles(tiles)
      },
      Metadata = new
      {
        metadata.Seed,
        metadata.SeedText,
        metadata.WorldId,
        metadata.UniqueId,
        metadata.WorldGeneratorVersion,
        metadata.MetadataRevision,
        metadata.MetadataIsFavorite,
        metadata.CreationTime,
        metadata.LastPlayed
      },
      Core = new
      {
        RuntimeMain.worldName,
        RuntimeMain.GameMode,
        RuntimeMain.expertMode,
        RuntimeMain.masterMode,
        RuntimeMain.hardMode,
        RuntimeMain.drunkWorld,
        RuntimeMain.getGoodWorld,
        RuntimeMain.tenthAnniversaryWorld,
        RuntimeMain.dontStarveWorld,
        RuntimeMain.notTheBeesWorld,
        RuntimeMain.remixWorld,
        RuntimeMain.noTrapsWorld,
        RuntimeMain.zenithWorld,
        RuntimeMain.skyblockWorld,
        RuntimeMain.vampireSeed,
        RuntimeMain.infectedSeed,
        RuntimeMain.teamBasedSpawnsSeed,
        RuntimeMain.dualDungeonsSeed,
        RuntimeMain.dayTime,
        RuntimeMain.time,
        RuntimeMain.moonPhase,
        RuntimeMain.bloodMoon,
        RuntimeMain.eclipse,
        RuntimeMain.pumpkinMoon,
        RuntimeMain.raining,
        RuntimeMain.rainTime,
        RuntimeMain.maxRaining,
        RuntimeMain.cloudAlpha,
        RuntimeMain.coinRain,
        RuntimeMain.windSpeedTarget,
        RuntimeMain.windSpeedCurrent,
        RuntimeMain.slimeRain,
        RuntimeMain.slimeRainTime,
        RuntimeMain.slimeRainKillCount,
        RuntimeMain.slimeWarningTime,
        RuntimeMain.fastForwardTimeToDawn,
        RuntimeMain.fastForwardTimeToDusk,
        RuntimeMain.sundialCooldown,
        RuntimeMain.moondialCooldown,
        RuntimeMain.cloudBGActive,
        RuntimeMain.numClouds,
        RuntimeMain.forceHalloweenForToday,
        RuntimeMain.forceXMasForToday,
        RuntimeMain.forceHalloweenForever,
        RuntimeMain.forceXMasForever,
        RuntimeMain.afterPartyOfDoom,
        WorldGenTreeBG1 = RuntimeWorldGen.treeBG1,
        WorldGenTreeBG2 = RuntimeWorldGen.treeBG2,
        WorldGenTreeBG3 = RuntimeWorldGen.treeBG3,
        WorldGenTreeBG4 = RuntimeWorldGen.treeBG4,
        WorldGenCorruptBG = RuntimeWorldGen.corruptBG,
        WorldGenJungleBG = RuntimeWorldGen.jungleBG,
        WorldGenSnowBG = RuntimeWorldGen.snowBG,
        WorldGenHallowBG = RuntimeWorldGen.hallowBG,
        WorldGenCrimsonBG = RuntimeWorldGen.crimsonBG,
        WorldGenDesertBG = RuntimeWorldGen.desertBG,
        WorldGenOceanBG = RuntimeWorldGen.oceanBG,
        WorldGenMushroomBG = RuntimeWorldGen.mushroomBG,
        WorldGenUnderworldBG = RuntimeWorldGen.underworldBG,
        WorldGenManifest = RuntimeWorldGen.Manifest?.Serialize(),
        ExtraSpawnPoints = NSSLC.WorldGeneration.GameContent.ExtraSpawnPointManager
          .extraSpawnPoints
          .Select(point => new { point.X, point.Y }).ToArray(),
        WorldGenParamEvil = RuntimeWorldGen.WorldGenParam_Evil,
        RuntimeMain.invasionDelay,
        RuntimeMain.invasionSize,
        RuntimeMain.invasionType,
        RuntimeMain.invasionX,
        RuntimeMain.invasionSizeStart,
        RuntimeMain.invasionWarn,
        RuntimeMain.anglerQuestFinished,
        RuntimeMain.anglerQuest,
        AnglerCompletions = RuntimeMain.anglerWhoFinishedToday.ToArray(),
        RuntimeWorldGen.spawnEye,
        RuntimeWorldGen.spawnHardBoss,
        RuntimeWorldGen.spawnMeteor,
        RuntimeWorldGen.meteorShowerCount,
        RuntimeWorldGen.shadowOrbSmashed,
        RuntimeWorldGen.shadowOrbCount,
        RuntimeWorldGen.altarCount,
        RuntimeWorldGen.crimson,
        SavedOreTiers = new
        {
          RuntimeWorldGen.SavedOreTiers.Copper,
          RuntimeWorldGen.SavedOreTiers.Iron,
          RuntimeWorldGen.SavedOreTiers.Silver,
          RuntimeWorldGen.SavedOreTiers.Gold,
          RuntimeWorldGen.SavedOreTiers.Cobalt,
          RuntimeWorldGen.SavedOreTiers.Mythril,
          RuntimeWorldGen.SavedOreTiers.Adamantite
        },
        NpcProgression = CaptureNpcProgression(),
        CalendarEvents = new
        {
          BirthdayParty = new
          {
            RuntimeBirthdayParty.ManualParty,
            RuntimeBirthdayParty.GenuineParty,
            RuntimeBirthdayParty.PartyDaysOnCooldown,
            CelebratingNpcs = RuntimeBirthdayParty.CelebratingNPCs.ToArray()
          },
          LanternNight = new
          {
            RuntimeLanternNight.ManualLanterns,
            RuntimeLanternNight.GenuineLanterns,
            RuntimeLanternNight.NextNightIsLanternNight,
            RuntimeLanternNight.LanternNightsOnCooldown
          },
          Sandstorm = new
          {
            RuntimeSandstorm.Happening,
            RuntimeSandstorm.TimeLeft,
            RuntimeSandstorm.Severity,
            RuntimeSandstorm.IntendedSeverity
          }
        },
        Bestiary = RuntimeMain.BestiaryTracker.Snapshot
      },
      Appearance = new
      {
        TreeX = RuntimeMain.treeX.ToArray(),
        TreeStyles = RuntimeMain.treeStyle.ToArray(),
        CaveBackX = RuntimeMain.caveBackX.ToArray(),
        CaveBackStyles = RuntimeMain.caveBackStyle.ToArray(),
        RuntimeMain.moonType,
        RuntimeMain.iceBackStyle,
        RuntimeMain.jungleBackStyle,
        RuntimeMain.hellBackStyle,
        TreeTops = Enumerable.Range(0, NSSLC.WorldGeneration.GameContent.TreeTopsInfo.AreaId.Count)
          .Select(areaId => RuntimeWorldGen.TreeTops.GetTreeStyle(areaId)).ToArray()
      },
      TileEntities = new
      {
        Owner = Serialize(session.Storage.TileEntities.CreateSnapshot()),
        Legacy = Serialize(RuntimeTileEntity.CreatePersistenceSnapshot())
      },
      TownHousing = CaptureTownHousing(session),
      Chests = CaptureLegacyChests(),
      Signs = CaptureLegacySigns(),
      PressurePlates = CaptureLegacyPressurePlates()
    });
  }

  private static object CaptureTownHousing(LoadedWorldSession session)
  {
    TownHousingRegistryComponent registry = session.World.TownHousing;
    IReadOnlyList<KeyValuePair<TownHousingResidentKey, TilePosition>> assignments =
      TownHousingRegistrySystem.GetRoomAssignmentsSnapshot(registry);
    var assignedNpcTypes = new HashSet<int>();
    var managerAssignments = new List<RuntimeTownRoomSnapshot>(assignments.Count);

    foreach (KeyValuePair<TownHousingResidentKey, TilePosition> assignment in assignments)
    {
      int npcType = assignment.Key.NpcType;
      TilePosition expectedRoom = assignment.Value;
      Require(assignedNpcTypes.Add(npcType),
        $"Town housing snapshot contains duplicate NPC type {npcType}.");

      bool hasRoom = RuntimeWorldGen.TownManager.HasRoom(npcType, out var roomPosition);
      Require(RuntimeWorldGen.TownManager.HasRoomQuick(npcType) && hasRoom &&
          roomPosition.X == expectedRoom.X && roomPosition.Y == expectedRoom.Y,
        $"WorldGen.TownManager does not expose the session's room for NPC type {npcType}.");

      var managerOccupants = new List<int>();
      RuntimeWorldGen.TownManager.AddOccupantsToList(
        expectedRoom.X,
        expectedRoom.Y,
        managerOccupants);
      int[] registryOccupants = TownHousingRegistrySystem.GetOccupants(registry, expectedRoom)
        .Select(resident => resident.NpcType)
        .ToArray();
      Require(managerOccupants.SequenceEqual(registryOccupants),
        $"WorldGen.TownManager occupants differ from the session for NPC type {npcType}.");

      managerAssignments.Add(new RuntimeTownRoomSnapshot(
        npcType,
        expectedRoom.X,
        expectedRoom.Y,
        managerOccupants.ToArray()));
    }

    for (int npcType = 0; npcType < NSSLC.WorldGeneration.NPCID.Count; npcType++)
    {
      Require(RuntimeWorldGen.TownManager.HasRoomQuick(npcType) ==
          assignedNpcTypes.Contains(npcType),
        $"WorldGen.TownManager room membership differs from the session for NPC type " +
        $"{npcType}.");
    }

    return new
    {
      registry.Revision,
      registry.ResidentKeyMode,
      registry.AssignedRoomCount,
      registry.HomelessResidentCount,
      registry.IsEmpty,
      RegistryAssignments = assignments.Select(assignment => new
      {
        assignment.Key.NpcType,
        assignment.Value.X,
        assignment.Value.Y
      }).ToArray(),
      ManagerAssignments = managerAssignments.ToArray()
    };
  }

  private static object CaptureNpcProgression()
  {
    return new
    {
      RuntimeNpc.downedBoss1,
      RuntimeNpc.downedBoss2,
      RuntimeNpc.downedBoss3,
      RuntimeNpc.downedQueenBee,
      RuntimeNpc.downedSlimeKing,
      RuntimeNpc.downedMechBoss1,
      RuntimeNpc.downedMechBoss2,
      RuntimeNpc.downedMechBoss3,
      RuntimeNpc.downedMechBossAny,
      RuntimeNpc.downedPlantBoss,
      RuntimeNpc.downedGolemBoss,
      RuntimeNpc.downedFishron,
      RuntimeNpc.downedAncientCultist,
      RuntimeNpc.downedMoonlord,
      RuntimeNpc.downedEmpressOfLight,
      RuntimeNpc.downedQueenSlime,
      RuntimeNpc.downedDeerclops,
      RuntimeNpc.downedHalloweenKing,
      RuntimeNpc.downedHalloweenTree,
      RuntimeNpc.downedChristmasIceQueen,
      RuntimeNpc.downedChristmasTree,
      RuntimeNpc.downedChristmasSantank,
      RuntimeNpc.downedGoblins,
      RuntimeNpc.downedFrost,
      RuntimeNpc.downedPirates,
      RuntimeNpc.downedMartians,
      RuntimeNpc.downedClown,
      RuntimeNpc.savedGoblin,
      RuntimeNpc.savedWizard,
      RuntimeNpc.savedMech,
      RuntimeNpc.savedAngler,
      RuntimeNpc.savedStylist,
      RuntimeNpc.savedTaxCollector,
      RuntimeNpc.savedBartender,
      RuntimeNpc.savedGolfer,
      RuntimeNpc.boughtCat,
      RuntimeNpc.boughtDog,
      RuntimeNpc.boughtBunny,
      RuntimeNpc.combatBookWasUsed,
      RuntimeNpc.combatBookVolumeTwoWasUsed,
      RuntimeNpc.peddlersSatchelWasUsed,
      RuntimeNpc.unlockedMerchantSpawn,
      RuntimeNpc.unlockedDemolitionistSpawn,
      RuntimeNpc.unlockedPartyGirlSpawn,
      RuntimeNpc.unlockedDyeTraderSpawn,
      RuntimeNpc.unlockedTruffleSpawn,
      RuntimeNpc.unlockedArmsDealerSpawn,
      RuntimeNpc.unlockedNurseSpawn,
      RuntimeNpc.unlockedPrincessSpawn,
      RuntimeNpc.unlockedSlimeBlueSpawn,
      RuntimeNpc.unlockedSlimeGreenSpawn,
      RuntimeNpc.unlockedSlimeOldSpawn,
      RuntimeNpc.unlockedSlimePurpleSpawn,
      RuntimeNpc.unlockedSlimeRainbowSpawn,
      RuntimeNpc.unlockedSlimeRedSpawn,
      RuntimeNpc.unlockedSlimeYellowSpawn,
      RuntimeNpc.unlockedSlimeCopperSpawn,
      RuntimeNpc.downedTowerSolar,
      RuntimeNpc.downedTowerVortex,
      RuntimeNpc.downedTowerNebula,
      RuntimeNpc.downedTowerStardust,
      RuntimeNpc.TowerActiveSolar,
      RuntimeNpc.TowerActiveVortex,
      RuntimeNpc.TowerActiveNebula,
      RuntimeNpc.TowerActiveStardust,
      RuntimeNpc.LunarApocalypseIsUp,
      RuntimeNpc.MoonLordCountdown,
      RuntimeDd2Event.DownedInvasionT1,
      RuntimeDd2Event.DownedInvasionT2,
      RuntimeDd2Event.DownedInvasionT3
    };
  }

  private static RuntimeChestSnapshot[] CaptureLegacyChests()
  {
    var result = new List<RuntimeChestSnapshot>();
    for (int index = 0; index < RuntimeMain.chest.Length; index++)
    {
      NSSLC.WorldGeneration.Chest? chest = RuntimeMain.chest[index];
      if (chest is null)
      {
        continue;
      }

      result.Add(new RuntimeChestSnapshot(
        index,
        chest.x,
        chest.y,
        chest.name,
        chest.item.Select(item => new RuntimeItemSnapshot(item.type, item.stack, item.prefix))
          .ToArray()));
    }

    return result.ToArray();
  }

  private static RuntimeSignSnapshot[] CaptureLegacySigns()
  {
    var result = new List<RuntimeSignSnapshot>();
    for (int index = 0; index < RuntimeMain.sign.Length; index++)
    {
      NSSLC.WorldGeneration.Sign? sign = RuntimeMain.sign[index];
      if (sign is not null)
      {
        result.Add(new RuntimeSignSnapshot(index, sign.x, sign.y, sign.text));
      }
    }

    return result.ToArray();
  }

  private static RuntimePressurePlateProjectionSnapshot CaptureLegacyPressurePlates()
  {
    lock (PressurePlateHelper.EntityCreationLock)
    {
      RuntimePressurePlateSnapshot[] plates = PressurePlateHelper.PressurePlatesPressed
        .OrderBy(entry => entry.Key.X)
        .ThenBy(entry => entry.Key.Y)
        .Select(entry => new RuntimePressurePlateSnapshot(
          entry.Key.X,
          entry.Key.Y,
          entry.Value.ToArray()))
        .ToArray();
      return new RuntimePressurePlateProjectionSnapshot(
        PressurePlateHelper.NeedsFirstUpdate,
        plates);
    }
  }

  private static string HashLegacyTiles(NSSLC.WorldGeneration.Tile[,] tiles)
  {
    using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    int width = tiles.GetLength(0);
    int height = tiles.GetLength(1);
    var column = new byte[checked(height * 14)];
    for (int x = 0; x < width; x++)
    {
      for (int y = 0; y < height; y++)
      {
        NSSLC.WorldGeneration.Tile tile = tiles[x, y] ??
          throw new InvalidOperationException($"Legacy tile ({x}, {y}) is null.");
        Span<byte> bytes = column.AsSpan(y * 14, 14);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, tile.type);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[2..], tile.wall);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[4..], tile.frameX);
        BinaryPrimitives.WriteInt16LittleEndian(bytes[6..], tile.frameY);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes[8..], tile.sTileHeader);
        bytes[10] = tile.liquid;
        bytes[11] = tile.bTileHeader;
        bytes[12] = tile.bTileHeader2;
        bytes[13] = tile.bTileHeader3;
      }

      hash.AppendData(column);
    }

    return Convert.ToHexString(hash.GetHashAndReset());
  }

  private static string Serialize<T>(T value)
  {
    return JsonSerializer.Serialize(value, SnapshotJsonOptions);
  }

  private static LoadedWorldSession RequireSession(
    IReadOnlyList<LoadedWorldSession> sessions,
    int index,
    string label)
  {
    Require(sessions.Count > index, $"The {label} did not create a candidate session.");
    return sessions[index];
  }

  private static WorldStorageOperationResult Canceled(string message)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, message));
  }

  private static WorldStorageOperationResult Invalid(string message)
  {
    return WorldStorageOperationResult.Failed(
      WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, message));
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed record RuntimeChestSnapshot(
    int Index,
    int X,
    int Y,
    string Name,
    RuntimeItemSnapshot[] Items);

  private sealed record RuntimeItemSnapshot(int Type, int Stack, byte Prefix);

  private sealed record RuntimeSignSnapshot(int Index, int X, int Y, string Text);

  private sealed record RuntimePressurePlateProjectionSnapshot(
    bool NeedsFirstUpdate,
    RuntimePressurePlateSnapshot[] Plates);

  private sealed record RuntimePressurePlateSnapshot(int X, int Y, bool[] Pressed);

  private sealed record RuntimeTownRoomSnapshot(
    int NpcType,
    int X,
    int Y,
    int[] Occupants);
}
