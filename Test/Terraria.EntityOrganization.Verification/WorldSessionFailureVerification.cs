using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EntityEcs;
using Terraria.Items;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Relationships;
using Terraria.WorldStorage;
using StorageNpcSlot = Terraria.WorldStorage.NpcSlot;

/// <summary>
/// Verifies the real Application world-load/recovery path around failed candidates, cancellation,
/// publication uncertainty, and consecutive published-session replacement.
/// </summary>
internal static class WorldSessionFailureVerification
{
  private const string FixtureApiId = "fixture.world-session.load";
  private const string FixtureOwnerId = LoadedWorldSession.OwnerContextId;
  private const string FixtureSectionId = "fixture.world-session";

  public static void Run()
  {
    NSSLC.WorldGeneration.Main.InitializeHeadlessRuntime();

    VerifyPreparationFailureCleanup();
    VerifyRecoveryCommitFailureCleanup();
    VerifyIssuedIdentityHistory();

    var harness = new SessionHarness();
    LoadedWorldSession previous = harness.PublishSession("previous");
    VerifyCancellationPreservesPrevious(harness, previous);
    VerifyPublicationFailurePreservesPrevious(harness, previous);
    harness.VerifySuccessfulSessionSwitch(previous);
    VerifyGeneratedHandlerLifecycle();
  }

  private static void VerifyIssuedIdentityHistory()
  {
    Guid issuedUuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    Guid replacementUuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
    var queuedUuids = new Queue<Guid>(new[] { issuedUuid, issuedUuid, replacementUuid });
    var registry = new EntityIdentityRegistry(() =>
      queuedUuids.Count == 0
        ? throw new InvalidOperationException("The UUID fixture queue was exhausted.")
        : queuedUuids.Dequeue());
    var firstSession = new LoadedWorldSession(registry);
    EntityReference firstReference;
    try
    {
      RuntimeEntityHandle firstRoot = CreatePublishedRoot(firstSession, "issued-history-first");
      Require(firstSession.EntityRuntime.TryGetReference(
          firstRoot,
          EntityReferenceScope.Npc,
          out firstReference),
        "The first history root must expose a scoped reference.");
    }
    finally
    {
      DisposeFixtureSession(firstSession);
    }

    var secondSession = new LoadedWorldSession(registry);
    try
    {
      bool duplicateRejected = false;
      try
      {
        _ = secondSession.EntityRuntime.CreateEntity();
      }
      catch (InvalidOperationException)
      {
        duplicateRejected = true;
      }

      Require(duplicateRejected,
        "A retired UUID must remain issued and reject reuse in a fresh runtime.");
      Require(secondSession.EntityRuntime.EntityCount == 0 &&
          secondSession.Storage.ProjectileIdentities.Count == 0 &&
          secondSession.Storage.TileEntities.Count == 0,
        "A rejected duplicate UUID allocation must leave no runtime or storage projection.");

      RuntimeEntityHandle replacementRoot = CreatePublishedRoot(
        secondSession,
        "issued-history-replacement");
      Require(secondSession.EntityRuntime.TryGetReference(
          replacementRoot,
          EntityReferenceScope.Npc,
          out EntityReference replacementReference),
        "A fresh UUID must allow the next runtime root to publish.");
      Require(replacementReference.EntityId != firstReference.EntityId &&
          replacementReference.RuntimeId != firstReference.RuntimeId,
        "The replacement root must use a new UUID and a new runtime id.");
    }
    finally
    {
      DisposeFixtureSession(secondSession);
    }
  }

  private static RuntimeEntityHandle CreatePublishedRoot(
    LoadedWorldSession session,
    string label)
  {
    RuntimeEntityHandle root = session.EntityRuntime.CreateEntity();
    Require(session.EntityRuntime.TryAttach(root, new TestWorldRootComponent(label)),
      "The identity-history root must accept its test component.");
    Require(session.EntityRuntime.TryPublishEntity(root),
      "The identity-history root must publish.");
    return root;
  }

  private static void VerifyPreparationFailureCleanup()
  {
    var session = new LoadedWorldSession();
    var fixture = new FixtureCatalog(FixtureMode.PreparationFailure);
    try
    {
      WorldLoadCoordinator coordinator = WorldStorageCoordinatorFactory.CreateLoadCoordinator(
        fixture.FileStore,
        fixture.Decoder,
        fixture.Validator,
        fixture);
      WorldRecoveryOutcome outcome = coordinator.Load(
        "world-session-prepare-failure.wld",
        CreateBindings(session));

      Require(outcome.Status == WorldRecoveryStatus.Failed,
        "A controlled prepare failure must fail the single-load coordinator.");
      Require(outcome.ApiExecution?.Stage == WorldLoadApiStage.Preparation,
        "The prepare fixture must report the preparation execution boundary.");
      Require(fixture.LastCandidate is not null,
        "The prepare fixture must allocate a candidate before failing.");
      VerifyCandidateClean(fixture.LastCandidate!);
      Require(!session.IsComplete && !session.IsPublished &&
          !session.IsPublicationUncertain,
        "A prepare failure must not complete or publish its candidate.");
    }
    finally
    {
      DisposeFixtureSession(session);
    }
  }

  private static void VerifyRecoveryCommitFailureCleanup()
  {
    var session = new LoadedWorldSession();
    var fixture = new FixtureCatalog(FixtureMode.CommitFailure);
    try
    {
      WorldLoadRecoveryCoordinator coordinator = CreateRecoveryCoordinator(fixture);
      WorldLoadRecoveryResult result = coordinator.Recover(
        session,
        "world-session-commit-failure.wld",
        CreateBindings(session),
        new FixtureRecoveryEffects(fixture),
        CancellationToken.None);

      Require(!result.Succeeded,
        "A controlled commit failure must not report a loaded world.");
      Require(result.LastLoadOutcome?.ApiExecution?.Stage == WorldLoadApiStage.Commit,
        "A partial candidate commit must report the commit execution boundary.");
      Require(result.Failure.Kind != WorldStorageFailureKind.None,
        "A commit failure must carry a terminal storage failure.");
      Require(fixture.LastCandidate is not null,
        "The commit fixture must allocate a candidate before failing.");
      VerifyCandidateClean(fixture.LastCandidate!);
      Require(!session.IsPublished && !session.IsPublicationUncertain,
        "A failed unpublished candidate must never become published or uncertain.");
    }
    finally
    {
      DisposeFixtureSession(session);
    }
  }

  private static void VerifyGeneratedHandlerLifecycle()
  {
    NSSLC.WorldGeneration.Main.worldPathName = Path.GetFullPath(
      Path.Combine("src", "World", "科研.wld"));
    var fixture = new GeneratedHandlerFixture();
    WorldStorageIoGate ioGate = WorldStorageCoordinatorFactory.CreateWorldStorageIoGate();

    WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandler(
      ioGate,
      fixture.FileStore,
      fixture.Decoder,
      fixture.Validator,
      fixture,
      fixture.CreateRecoveryEffects,
      fixture.ObserveResult,
      fixture.ObserveException,
      fixture.GetCancellationToken,
      fixture.CreateSession);

    fixture.Mode = GeneratedHandlerMode.Success;
    NSSLC.WorldGeneration.Main.LoadWorld();
    LoadedWorldSession first = fixture.RequirePublishedSession("initial generated load");
    EntityReference firstReference = fixture.RequireCandidateReference();
    Require(ReferenceEquals(
        WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
        first) &&
        ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, first.World),
      "A generated load must publish both the host pointer and legacy world session.");
    Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld &&
        first.IsPublished,
      "A successful generated load must release the load gate after publication.");

    fixture.RunGeneratedCandidateFailureScenario(
      GeneratedHandlerMode.PreparationFailure,
      WorldLoadApiStage.Preparation,
      first,
      firstReference,
      "generated preparation failure");
    fixture.RunGeneratedCandidateFailureScenario(
      GeneratedHandlerMode.CommitFailure,
      WorldLoadApiStage.Commit,
      first,
      firstReference,
      "generated commit failure");
    fixture.RunFailureScenario(
      GeneratedHandlerMode.CanceledDuringLoad,
      first,
      firstReference,
      "generated cancellation");
    fixture.RunFailureScenario(
      GeneratedHandlerMode.PublicationFailure,
      first,
      firstReference,
      "generated publication failure");
    fixture.RunFailureScenario(
      GeneratedHandlerMode.FinalizeFailure,
      first,
      firstReference,
      "generated finalization failure");
    fixture.RunDirtySessionFactoryScenario(first, firstReference);
    fixture.RunBorrowBlockedRetirementScenario(first, firstReference);

    fixture.Mode = GeneratedHandlerMode.Success;
    fixture.ClearObservation();
    NSSLC.WorldGeneration.Main.LoadWorld();
    LoadedWorldSession next = fixture.RequirePublishedSession("successful generated switch");
    Require(!ReferenceEquals(first, next) && first.WorldRuntimeId != next.WorldRuntimeId,
      "A successful generated switch must allocate a fresh runtime id.");
    Require(first.IsDisposed && !next.IsDisposed &&
        ReferenceEquals(WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession, next) &&
        ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, next.World),
      "The old generated session must retire only after the new session commits.");
    Require(!next.EntityRuntime.TryResolve(firstReference, out _),
      "A reference from the retired generated runtime must be rejected by the new runtime.");
    Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld,
      "A successful generated switch must leave the world-load gate released.");
  }

  private static void VerifyCancellationPreservesPrevious(
    SessionHarness harness,
    LoadedWorldSession previous)
  {
    var cancellation = new CancellationTokenSource();
    var fixture = new FixtureCatalog(FixtureMode.CanceledDuringLoad, cancellation);
    var candidate = new LoadedWorldSession(harness.IdentityRegistry);
    try
    {
      WorldLoadRecoveryCoordinator coordinator = CreateRecoveryCoordinator(fixture);
      WorldLoadRecoveryResult result = coordinator.Recover(
        candidate,
        "world-session-canceled.wld",
        CreateBindings(candidate),
        new FixtureRecoveryEffects(fixture),
        cancellation.Token);

      Require(!result.Succeeded &&
          result.TerminalAction == Terraria.WorldGeneration.Components.WorldLoadRecoveryAction.ReportLoadFailure,
        "Cancellation must terminate recovery without publication.");
      Require(result.Failure.Kind == WorldStorageFailureKind.Canceled,
        "Cancellation must remain classified as canceled at the recovery boundary.");
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          previous),
        "Canceling a candidate load must preserve the previously published session.");
      Require(!previous.IsDisposed &&
          previous.EntityRuntime.TryResolve(harness.PreviousReference, out _),
        "The previous session reference must remain usable after candidate cancellation.");
      Require(fixture.LastCandidate is not null,
        "The cancellation fixture must allocate a candidate before cancellation.");
      VerifyCandidateClean(fixture.LastCandidate!);
      Require(!candidate.IsPublished && !candidate.IsPublicationUncertain,
        "A canceled candidate must not publish or enter publication uncertainty.");
    }
    finally
    {
      cancellation.Dispose();
      DisposeFixtureSession(candidate);
    }
  }

  private static void VerifyPublicationFailurePreservesPrevious(
    SessionHarness harness,
    LoadedWorldSession previous)
  {
    var fixture = new FixtureCatalog(FixtureMode.Success);
    var candidate = new LoadedWorldSession(harness.IdentityRegistry);
    try
    {
      WorldLoadRecoveryCoordinator coordinator = CreateRecoveryCoordinator(fixture);
      WorldLoadRecoveryResult result = coordinator.Recover(
        candidate,
        "world-session-publication-failure.wld",
        CreateBindings(candidate),
        new FixtureRecoveryEffects(fixture, failPublication: true),
        CancellationToken.None);

      Require(!result.Succeeded,
        "A publication effect failure must fail the recovery result.");
      Require(result.Failure.Kind != WorldStorageFailureKind.None,
        "A publication failure must carry a terminal storage failure.");
      Require(candidate.IsPublicationUncertain == false && !candidate.IsPublished,
        "Recovery cleanup must settle a failed candidate out of publication uncertainty.");
      Require(fixture.LastCandidate is not null,
        "The publication fixture must allocate a complete candidate.");
      VerifyCandidateClean(fixture.LastCandidate!);
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          previous),
        "Publication failure must leave the previous session active.");
      Require(!previous.IsDisposed &&
          previous.EntityRuntime.TryResolve(harness.PreviousReference, out _),
        "Publication failure must preserve the previous session reference.");
    }
    finally
    {
      DisposeFixtureSession(candidate);
    }
  }

  private static WorldLoadRecoveryCoordinator CreateRecoveryCoordinator(
    FixtureCatalog fixture)
  {
    return WorldStorageCoordinatorFactory.CreateLoadRecoveryCoordinator(
      fixture.FileStore,
      fixture.Decoder,
      fixture.Validator,
      fixture);
  }

  private static WorldLoadApiRuntimeBindings CreateBindings(LoadedWorldSession session)
  {
    return new WorldLoadApiRuntimeBindingsBuilder()
      .AddOwnerContext(LoadedWorldSession.OwnerContextId, session)
      .Build();
  }

  private static CandidateEvidence AllocateCandidate(
    LoadedWorldSession session,
    string label)
  {
    EntityRuntime runtime = session.EntityRuntime;
    RuntimeEntityHandle root = runtime.CreateEntity();
    Require(runtime.TryAttach(root, new TestWorldRootComponent(label)),
      "The candidate root must accept its test component.");
    Require(runtime.TryPublishEntity(root),
      "The candidate root must publish before it can expose a reference.");
    Require(runtime.TryGetReference(root, EntityReferenceScope.Npc, out EntityReference reference),
      "The candidate root must expose a scoped reference.");

    WorldStorageRoot storage = session.Storage;
    var savedNpc = new WorldNpcState(
      netId: 1,
      legacyTypeName: null,
      isTownNpc: false,
      name: $"world-session-{label}",
      x: 16.0f,
      y: 32.0f,
      homeless: false,
      home: default,
      variation: null,
      homelessDespawn: false);
    Require(storage.Npcs.TryAllocate(
          savedNpc,
          out StorageNpcSlot npcSlot,
          out uint npcGeneration),
      "The candidate must allocate an NPC slot projection.");

    var projectileIdentity = new OwnerProjectileIdentity(new PlayerSlot(0), 23);
    var projectileHandle = new ProjectileHandle(new ProjectileSlot(4), 1);
    Require(storage.ProjectileIdentities.TryRegister(projectileIdentity, projectileHandle),
      "The candidate must allocate a Projectile identity projection.");

    var tileSnapshot = new TileEntitySnapshot(
      new TileEntityId(7),
      new TileEntityTypeId(2),
      new TileCoordinate(12, 16),
      Array.Empty<ItemState>(),
      logicOn: true);
    storage.TileEntities.CommitRuntimeSnapshot(
      new TileEntityStoreSnapshot(new[] { tileSnapshot }, nextId: 8));
    Require(storage.TileEntities.Count == 1 && storage.TileEntityUpdates.Count == 1,
      "The candidate must allocate TileEntity and schedule projections.");
    Require(storage.TileEntities.TryGetEntityReference(
        tileSnapshot.Id,
        out EntityReference tileEntityReference),
      "The candidate TileEntity must expose a runtime reference before cleanup.");

    return new CandidateEvidence(
      session,
      runtime,
      storage,
      root,
      reference,
      npcSlot,
      npcGeneration,
      projectileIdentity,
      projectileHandle,
      tileSnapshot.Id,
      tileEntityReference);
  }

  private static void CompleteCandidate(LoadedWorldSession session)
  {
    var footer = WorldLoadSection<WorldFileFooterSection>.Present(
      new WorldFileFooterSection(
        true,
        session.World.Descriptor.Name,
        session.World.Descriptor.WorldId));
    var footerApi = new WorldFooterLoadApi();
    WorldLoadPrepareResult<WorldLoadSection<WorldFileFooterSection>> prepared =
      footerApi.PrepareLoad(session, footer);
    Require(prepared.IsPrepared,
      "The footer fixture must prepare a complete candidate.");
    WorldLoadSection<WorldFileFooterSection> preparedData = prepared.PreparedData;
    WorldLoadCommitResult commit = footerApi.CommitLoad(session, in preparedData);
    Require(commit.Succeeded && session.IsComplete,
      "The footer fixture must complete the candidate session.");
  }

  private static void CleanupCandidate(CandidateEvidence evidence)
  {
    EntityRuntime runtime = evidence.Session.EntityRuntime;
    if (runtime.TryGetStatus(evidence.Root, out _))
    {
      Require(runtime.TryBeginTermination(evidence.Root),
        "Candidate cleanup must begin root termination through EntityRuntime.");
      Require(runtime.TryRemoveEntity(evidence.Root),
        "Candidate cleanup must remove the root through EntityRuntime.");
    }

    evidence.Storage.Dispose();
    evidence.Cleaned = true;
  }

  private static void VerifyCandidateClean(CandidateEvidence evidence)
  {
    Require(evidence.AllocatedRuntimeEntityCount >= 2,
      "The candidate cleanup fixture must capture both its world root and TileEntity runtime root.");
    Require(evidence.Cleaned,
      "The candidate cleanup seam must run before the recovery result is observed.");
    Require(evidence.Runtime.EntityCount == 0,
      "Candidate cleanup must remove every captured runtime root.");
    Require(!evidence.Session.EntityRuntime.TryGetStatus(evidence.Root, out _),
      "Candidate cleanup must remove the runtime root.");
    Require(!evidence.Session.EntityRuntime.TryResolve(evidence.Reference, out _),
      "A reference captured from a failed candidate must be stale after cleanup.");
    Require(ThrowsObjectDisposed(() =>
        evidence.Storage.Npcs.TryGet(evidence.NpcSlot, evidence.NpcGeneration, out _)),
      "Candidate cleanup must dispose the NPC slot projection.");
    Require(ThrowsObjectDisposed(() => _ = evidence.Storage.ProjectileIdentities.Count),
      "Candidate cleanup must dispose the Projectile identity index.");
    Require(evidence.Session.EntityRuntime.EntityCount == 0 &&
        !evidence.Session.EntityRuntime.TryResolve(evidence.TileEntityReference, out _),
      "Candidate cleanup must remove TileEntity roots and invalidate their references.");
    Require(ThrowsObjectDisposed(() => _ = evidence.Storage.TileEntities.Count) &&
        ThrowsObjectDisposed(() => _ = evidence.Storage.TileEntities.NextId) &&
        ThrowsObjectDisposed(() =>
          evidence.Storage.TileEntities.CreateSnapshot()) &&
        ThrowsObjectDisposed(() =>
          evidence.Storage.TileEntities.TryGetRuntimeState(evidence.TileEntityId, out _)) &&
        ThrowsObjectDisposed(() =>
          evidence.Storage.TileEntities.TryGetEntityReference(evidence.TileEntityId, out _)),
      "Candidate cleanup must reject live TileEntity reads after TileEntityStore disposal.");
    int runtimeEntityCount = evidence.Session.EntityRuntime.EntityCount;
    Require(ThrowsObjectDisposed(() => evidence.Storage.TileEntities.CommitRuntimeSnapshot(
          new TileEntityStoreSnapshot(
            new[]
            {
              new TileEntitySnapshot(
                new TileEntityId(9),
                new TileEntityTypeId(2),
                new TileCoordinate(20, 24),
                Array.Empty<ItemState>(),
                logicOn: true),
            },
            nextId: 10))) &&
        evidence.Session.EntityRuntime.EntityCount == runtimeEntityCount,
      "Candidate cleanup must reject TileEntity mutation without allocating a new runtime root.");
    Require(ThrowsObjectDisposed(() => _ = evidence.Storage.TileEntityUpdates.Count),
      "Candidate cleanup must dispose the TileEntity schedule projection.");
  }

  private static void VerifyCapturedCandidateDisposed(
    CandidateEvidence evidence,
    string scenario)
  {
    Require(evidence.AllocatedRuntimeEntityCount >= 2 &&
        evidence.Runtime.EntityCount == 0,
      $"The production finally must remove every runtime root from the failed {scenario} candidate.");
    Require(ThrowsObjectDisposed(() =>
        evidence.Storage.Npcs.TryGet(evidence.NpcSlot, evidence.NpcGeneration, out _)) &&
        ThrowsObjectDisposed(() => _ = evidence.Storage.ProjectileIdentities.Count) &&
        ThrowsObjectDisposed(() => _ = evidence.Storage.TileEntities.Count) &&
        ThrowsObjectDisposed(() => _ = evidence.Storage.TileEntityUpdates.Count) &&
        ThrowsObjectDisposed(() => evidence.Storage.TileEntities.CreateSnapshot()) &&
        ThrowsObjectDisposed(() => evidence.Storage.TileEntities.CommitRuntimeSnapshot(
          new TileEntityStoreSnapshot(Array.Empty<TileEntitySnapshot>(), nextId: 0))),
      $"The production finally must dispose every captured storage projection for the failed {scenario} candidate.");
  }

  private static void DisposeFixtureSession(LoadedWorldSession session)
  {
    if (!session.IsDisposed)
    {
      session.Dispose();
    }
  }

  private static bool ThrowsObjectDisposed(Action action)
  {
    try
    {
      action.Invoke();
      return false;
    }
    catch (ObjectDisposedException)
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

  private enum FixtureMode
  {
    Success,
    PreparationFailure,
    CommitFailure,
    CanceledDuringLoad,
  }

  private sealed class FixtureCatalog : IWorldLoadApiCatalog
  {
    private readonly FixtureMode _mode;
    private readonly CancellationTokenSource? _cancellation;
    private readonly bool _requireFixtureSection;

    public FixtureCatalog(
      FixtureMode mode,
      CancellationTokenSource? cancellation = null,
      bool requireFixtureSection = true)
    {
      _mode = mode;
      _cancellation = cancellation;
      _requireFixtureSection = requireFixtureSection;
      Descriptors = Array.AsReadOnly(new[]
      {
        new WorldLoadApiDescriptor(
          FixtureApiId,
          FixtureOwnerId,
          FixtureSectionId,
          minimumFormatVersion: 1,
          maximumFormatVersion: 1,
          requirement: WorldLoadSectionRequirement.Required),
      });
    }

    public IReadOnlyList<WorldLoadApiDescriptor> Descriptors { get; }

    public FixtureWorldFileStore FileStore { get; } = new();

    public FixtureDocumentDecoder Decoder { get; } = new();

    public FixtureDocumentValidator Validator { get; } = new();

    public CandidateEvidence? LastCandidate { get; private set; }

    public WorldLoadApiExecutionResult Execute(WorldLoadApiBindings bindings)
    {
      if (!bindings.TryGetOwnerContext(FixtureOwnerId, out LoadedWorldSession session))
      {
        throw new InvalidOperationException(
          "The world-session fixture did not receive its owner context.");
      }

      if (_requireFixtureSection &&
          (!bindings.TryGetSection<FixtureWorldSection>(FixtureSectionId, out var section) ||
           !section.IsPresent))
      {
        throw new InvalidOperationException(
          "The world-session fixture did not receive its required section.");
      }

      if (_mode == FixtureMode.PreparationFailure)
      {
        LastCandidate = AllocateCandidate(session, _mode.ToString());
        CleanupCandidate(LastCandidate);
        return WorldLoadApiExecutionResult.Failed(
          WorldLoadApiStage.Preparation,
          FixtureApiId,
          FixtureOwnerId,
          WorldLoadApiFailure.Create(
            "ControlledPreparationFailure",
            "The fixture rejected the candidate during preparation."));
      }

      if (_mode == FixtureMode.CommitFailure)
      {
        LastCandidate = AllocateCandidate(session, _mode.ToString());
        CleanupCandidate(LastCandidate);
        return WorldLoadApiExecutionResult.Failed(
          WorldLoadApiStage.Commit,
          FixtureApiId,
          FixtureOwnerId,
          WorldLoadApiFailure.Create(
            "ControlledCommitFailure",
            "The fixture rejected the candidate after partial commit."));
      }

      if (_mode == FixtureMode.CanceledDuringLoad)
      {
        LastCandidate = AllocateCandidate(session, _mode.ToString());
        CleanupCandidate(LastCandidate);
        _cancellation!.Cancel();
        bindings.CancellationToken.ThrowIfCancellationRequested();
      }

      CompleteCandidate(session);
      LastCandidate = AllocateCandidate(session, _mode.ToString());
      return WorldLoadApiExecutionResult.Completed();
    }
  }

  private sealed class FixtureDocumentDecoder : IWorldPersistenceDocumentDecoder
  {
    private readonly WorldPersistenceDocument _document = new(
      formatVersion: 1,
      sections: new[]
      {
        WorldPersistenceSection.Create(FixtureSectionId, new FixtureWorldSection()),
      });

    public WorldPersistenceDecodeResult Decode(ReadOnlyMemory<byte> fileBytes)
    {
      return fileBytes.IsEmpty
        ? WorldPersistenceDecodeResult.Failed(
          WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData))
        : WorldPersistenceDecodeResult.Decoded(_document);
    }
  }

  private sealed class FixtureDocumentValidator : IWorldPersistenceDocumentValidator
  {
    public WorldStorageFailure Validate(WorldPersistenceDocument document)
    {
      return document.FormatVersion == 1 &&
          document.TryGetSection<FixtureWorldSection>(
            FixtureSectionId,
            out WorldLoadSection<FixtureWorldSection> section) &&
          section.IsPresent
        ? WorldStorageFailure.None
        : WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData);
    }
  }

  private sealed class FixtureWorldFileStore : IWorldFileStore
  {
    public WorldStorageReadResult ReadAllBytes(string path)
    {
      return WorldStorageReadResult.FromBytes(new byte[] { 1 });
    }

    public WorldStorageOperationResult WriteAllBytes(
      string path,
      ReadOnlyMemory<byte> data)
    {
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult Delete(string path, bool forceDelete)
    {
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult CheckBackupExists(
      string worldPath,
      out bool backupExists)
    {
      backupExists = false;
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult RestoreBackupAndDelete(string worldPath)
    {
      return WorldStorageOperationResult.Success;
    }
  }

  private sealed class FixtureRecoveryEffects
    : IWorldLoadRecoveryEffects
  {
    private readonly FixtureCatalog _fixture;
    private readonly bool _failPublication;

    public FixtureRecoveryEffects(FixtureCatalog fixture, bool failPublication = false)
    {
      _fixture = fixture;
      _failPublication = failPublication;
    }

    public WorldStorageOperationResult PublishLoadedWorld(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return _failPublication
        ? WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.IoFailure,
            "The fixture rejected publication."))
        : WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult SettleLoadedWorld(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult FinalizeLoadedWorld(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult ResetWorld(LoadedWorldSession session)
    {
      if (_fixture.LastCandidate is CandidateEvidence candidate && !candidate.Cleaned)
      {
        CleanupCandidate(candidate);
      }

      return WorldStorageOperationResult.Success;
    }
  }

  private enum GeneratedHandlerMode
  {
    Success,
    PreparationFailure,
    CommitFailure,
    CanceledDuringLoad,
    PublicationFailure,
    FinalizeFailure,
    BorrowBlockedRetirement,
    DirtySessionRejected,
  }

  private sealed class GeneratedHandlerFixture : IWorldLoadApiCatalog
  {
    public readonly FixtureWorldFileStore FileStore = new();
    public readonly FixtureDocumentDecoder Decoder = new();
    public readonly FixtureDocumentValidator Validator = new();
    public readonly EntityIdentityRegistry IdentityRegistry = new();

    public IReadOnlyList<WorldLoadApiDescriptor> Descriptors { get; } =
      Array.AsReadOnly(new[]
      {
        new WorldLoadApiDescriptor(
          FixtureApiId,
          FixtureOwnerId,
          FixtureSectionId,
          minimumFormatVersion: 1,
          maximumFormatVersion: 1,
          requirement: WorldLoadSectionRequirement.Required),
      });

    public GeneratedHandlerMode Mode { get; set; }
    public CancellationTokenSource? Cancellation { get; private set; }
    public LoadedWorldSession? LastSession { get; private set; }
    public CandidateEvidence? LastCandidate { get; private set; }
    public CandidateEvidence? PublishedCandidate { get; private set; }
    public DirtySessionEvidence? DirtySession { get; private set; }
    public WorldLoadRecoveryResult? LastResult { get; private set; }
    public Exception? LastException { get; private set; }

    public LoadedWorldSession CreateSession()
    {
      if (Mode == GeneratedHandlerMode.DirtySessionRejected)
      {
        DirtySessionEvidence dirtySession = CreateDirtySession();
        DirtySession = dirtySession;
        LastSession = dirtySession.Session;
        return dirtySession.Session;
      }

      LastSession = new LoadedWorldSession(IdentityRegistry);
      return LastSession;
    }

    public CancellationToken GetCancellationToken()
    {
      return Cancellation?.Token ?? CancellationToken.None;
    }

    public WorldLoadApiExecutionResult Execute(WorldLoadApiBindings bindings)
    {
      if (!bindings.TryGetOwnerContext(
            LoadedWorldSession.OwnerContextId,
            out LoadedWorldSession session))
      {
        throw new InvalidOperationException(
          "The generated-handler fixture did not receive its session owner context.");
      }

      if (Mode == GeneratedHandlerMode.PreparationFailure)
      {
        LastCandidate = AllocateCandidate(session, "generated-preparation-failure");
        return WorldLoadApiExecutionResult.Failed(
          WorldLoadApiStage.Preparation,
          FixtureApiId,
          FixtureOwnerId,
          WorldLoadApiFailure.Create(
            "ControlledGeneratedPreparationFailure",
            "The generated-handler fixture rejected the candidate during preparation."));
      }

      if (Mode == GeneratedHandlerMode.CommitFailure)
      {
        LastCandidate = AllocateCandidate(session, "generated-commit-failure");
        return WorldLoadApiExecutionResult.Failed(
          WorldLoadApiStage.Commit,
          FixtureApiId,
          FixtureOwnerId,
          WorldLoadApiFailure.Create(
            "ControlledGeneratedCommitFailure",
            "The generated-handler fixture rejected the candidate after partial commit."));
      }

      if (Mode == GeneratedHandlerMode.CanceledDuringLoad)
      {
        LastCandidate = AllocateCandidate(session, "generated-canceled");
        Cancellation!.Cancel();
        bindings.CancellationToken.ThrowIfCancellationRequested();
      }

      CompleteCandidate(session);
      LastCandidate = AllocateCandidate(session, $"generated-{Mode}");
      if (Mode == GeneratedHandlerMode.CanceledDuringLoad)
      {
        throw new InvalidOperationException(
          "The generated-handler cancellation fixture must cancel before completion.");
      }

      return WorldLoadApiExecutionResult.Completed();
    }

    public IWorldLoadRecoveryEffects CreateRecoveryEffects(LoadedWorldSession session)
    {
      return new GeneratedRecoveryEffects(this);
    }

    public void ObserveResult(WorldLoadRecoveryResult result)
    {
      LastResult = result;
      if (result.Succeeded && LastCandidate is not null)
      {
        PublishedCandidate = LastCandidate;
      }
    }

    public void ObserveException(Exception exception)
    {
      LastException = exception;
    }

    public void ClearObservation()
    {
      LastResult = null;
      LastException = null;
      LastCandidate = null;
      if (Cancellation is not null)
      {
        Cancellation.Dispose();
        Cancellation = null;
      }
    }

    public LoadedWorldSession RequirePublishedSession(string scenario)
    {
      Require(LastException is null,
        $"The {scenario} generated handler must not escape an exception.");
      Require(LastResult?.Succeeded == true && LastSession is not null &&
          LastSession.IsPublished && !LastSession.IsDisposed,
        $"The {scenario} generated handler must publish a live session.");
      return LastSession!;
    }

    public EntityReference RequireCandidateReference()
    {
      Require(LastCandidate is not null,
        "The generated-handler fixture must capture its candidate root.");
      return LastCandidate!.Reference;
    }

    public void RunFailureScenario(
      GeneratedHandlerMode mode,
      LoadedWorldSession previous,
      EntityReference previousReference,
      string scenario)
    {
      ClearObservation();
      Mode = mode;
      if (mode == GeneratedHandlerMode.CanceledDuringLoad)
      {
        Cancellation = new CancellationTokenSource();
      }

      NSSLC.WorldGeneration.Main.LoadWorld();
      Require(LastResult is not null && !LastResult.Succeeded && LastException is null,
        $"The {scenario} generated handler must return a failed recovery result.");
      VerifyFactoryDisposedCandidate(scenario);
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          previous) &&
          ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, previous.World),
        $"The {scenario} must preserve both active session pointers.");
      Require(!previous.IsDisposed &&
          previous.EntityRuntime.TryResolve(previousReference, out _),
        $"The {scenario} must preserve the previous session reference.");
      Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld &&
          !previous.Lifecycle.LoadFailed,
          $"The {scenario} must leave the previous lifecycle usable and the load gate released.");
    }

    public void RunGeneratedCandidateFailureScenario(
      GeneratedHandlerMode mode,
      WorldLoadApiStage expectedStage,
      LoadedWorldSession previous,
      EntityReference previousReference,
      string scenario)
    {
      ClearObservation();
      Mode = mode;
      NSSLC.WorldGeneration.Main.LoadWorld();

      Require(LastResult is not null && !LastResult.Succeeded && LastException is null,
        $"The {scenario} generated handler must return a failed recovery result.");
      WorldLoadRecoveryResult result = LastResult!;
      Require(result.Failure.Kind != WorldStorageFailureKind.None,
        $"The {scenario} generated handler must preserve a classified recovery failure.");
      WorldRecoveryOutcome outcome = result.LastLoadOutcome ??
        throw new InvalidOperationException(
          $"The {scenario} generated handler must retain its failed load outcome.");
      Require(outcome.ApiExecution?.Stage == expectedStage &&
          LastCandidate is not null &&
          ReferenceEquals(LastCandidate.Session, LastSession),
        $"The {scenario} must report the expected API stage after allocating its partial candidate.");
      if (expectedStage == WorldLoadApiStage.Commit)
      {
        Require(outcome.RequiresCandidateDiscard,
          "A generated commit failure must mark the partial candidate unsafe for reuse.");
      }

      VerifyFactoryDisposedCandidate(scenario);
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          previous) &&
          ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, previous.World),
        $"The {scenario} must preserve both active session pointers.");
      Require(!previous.IsDisposed &&
          previous.EntityRuntime.TryResolve(previousReference, out _),
        $"The {scenario} must preserve the previous session reference.");
      Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld &&
          !previous.Lifecycle.LoadFailed,
        $"The {scenario} must leave the previous lifecycle usable and the load gate released.");
    }

    public void RunDirtySessionFactoryScenario(
      LoadedWorldSession previous,
      EntityReference previousReference)
    {
      ClearObservation();
      Mode = GeneratedHandlerMode.DirtySessionRejected;
      int previousEntityCount = previous.EntityRuntime.EntityCount;

      try
      {
        NSSLC.WorldGeneration.Main.LoadWorld();

        Require(LastResult is null,
          "A dirty session rejected by the factory preflight must not produce a recovery result.");
        Require(LastException is InvalidOperationException exception &&
            exception.Message.Contains(
              "fresh world session",
              StringComparison.Ordinal),
          "A dirty generated session must be rejected as a fresh-session preflight error.");
        Require(DirtySession is not null && !DirtySession.Session.IsDisposed,
          "A session rejected before recovery remains host-owned and must not be disposed by the factory.");
        Require(DirtySession!.Session.CommittedSections.Count == 0 &&
            DirtySession.Session.EntityRuntime.EntityCount == 1 &&
            DirtySession.Session.EntityRuntime.TryResolve(DirtySession.Reference, out _),
          "The host-owned dirty session root must remain available after factory preflight rejection without relying on committed-section state.");
        Require(ReferenceEquals(
            WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
            previous) &&
            ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, previous.World),
          "A dirty session factory rejection must not change either active world pointer.");
        Require(previous.EntityRuntime.EntityCount == previousEntityCount &&
            !previous.IsDisposed &&
            previous.EntityRuntime.TryResolve(previousReference, out _),
          "A dirty session factory rejection must not mutate the previous runtime or reference.");
        Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld &&
            NSSLC.WorldGeneration.WorldGen.loadFailed &&
            !previous.Lifecycle.LoadFailed,
          "A dirty session preflight error must classify as load failure while releasing the gate and preserving the previous lifecycle.");
      }
      finally
      {
        DisposeHostOwnedDirtySession();
      }
    }

    public void RunBorrowBlockedRetirementScenario(
      LoadedWorldSession previous,
      EntityReference previousReference)
    {
      ClearObservation();
      Mode = GeneratedHandlerMode.BorrowBlockedRetirement;
      CandidateEvidence previousCandidate = PublishedCandidate ??
        throw new InvalidOperationException(
          "The borrow-blocked retirement fixture requires the previously published candidate.");
      Require(ReferenceEquals(previousCandidate.Session, previous),
        "The borrow-blocked retirement fixture must borrow the currently active previous session.");

      bool loadInvokedWhileBorrowed = false;
      bool inspected = previous.EntityRuntime.TryInspect<TestWorldRootComponent>(
        previousCandidate.Root,
        (in TestWorldRootComponent _) =>
        {
          loadInvokedWhileBorrowed = true;
          NSSLC.WorldGeneration.Main.LoadWorld();
        });
      Require(inspected && loadInvokedWhileBorrowed,
        "The borrow-blocked retirement fixture must invoke Main.LoadWorld while the previous root is borrowed.");
      Require(LastResult is not null && !LastResult.Succeeded && LastException is null,
        "A borrow-blocked retirement must fail after the production active-session exchange preflight.");
      Require(LastResult!.Failure.Kind != WorldStorageFailureKind.None &&
          LastResult.Failure.Detail?.Contains(
            "previous world session cannot be retired",
            StringComparison.Ordinal) == true,
        "The production CommitPublishedSession preflight must classify the borrowed previous retirement failure.");
      VerifyFactoryDisposedCandidate("generated borrowed previous retirement");
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          previous) &&
          ReferenceEquals(NSSLC.WorldGeneration.Main.ActiveWorldSession, previous.World) &&
          !previous.IsDisposed &&
          previous.EntityRuntime.TryResolve(previousReference, out _),
        "A borrow-blocked retirement must preserve the previous active session atomically.");
      Require(!NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld &&
          !previous.Lifecycle.LoadFailed,
        "A borrow-blocked retirement must release the gate without marking the previous lifecycle failed.");
    }

    private void VerifyFactoryDisposedCandidate(string scenario)
    {
      Require(LastSession is not null && LastSession.IsDisposed,
        $"The generated handler must dispose the failed {scenario} candidate in its finally path.");
      Require(LastCandidate is not null &&
          ReferenceEquals(LastCandidate.Session, LastSession),
        $"The generated handler must retain captured partial-candidate evidence for {scenario}.");
      Require(ThrowsObjectDisposed(() => _ = LastSession!.EntityRuntime) &&
          ThrowsObjectDisposed(() => _ = LastSession!.Storage),
          $"The disposed {scenario} candidate must reject runtime and storage access.");
      VerifyCapturedCandidateDisposed(LastCandidate!, scenario);
    }

    private DirtySessionEvidence CreateDirtySession()
    {
      var session = new LoadedWorldSession(IdentityRegistry);
      RuntimeEntityHandle root = session.EntityRuntime.CreateEntity();
      Require(session.EntityRuntime.TryAttach(root, new TestWorldRootComponent("generated-dirty")),
        "The dirty session root must accept its host-owned component.");
      Require(session.EntityRuntime.TryPublishEntity(root),
        "The dirty session root must publish before the host returns it from sessionFactory.");
      Require(session.EntityRuntime.TryGetReference(
          root,
          EntityReferenceScope.Npc,
          out EntityReference reference),
        "The dirty session root must expose a reference for the host-ownership assertion.");
      return new DirtySessionEvidence(session, root, reference);
    }

    private void DisposeHostOwnedDirtySession()
    {
      if (DirtySession is null || DirtySession.Session.IsDisposed)
      {
        return;
      }

      // The factory rejects this session before recovery owns it. The fixture, acting as the
      // sessionFactory host, is responsible for disposing the pre-existing root after assertions.
      DirtySession.Session.Dispose();
    }

    private sealed class GeneratedRecoveryEffects(
      GeneratedHandlerFixture fixture) : IWorldLoadRecoveryEffects
    {
      public WorldStorageOperationResult PublishLoadedWorld(
        LoadedWorldSession session,
        CancellationToken cancellationToken)
      {
        cancellationToken.ThrowIfCancellationRequested();
        return fixture.Mode == GeneratedHandlerMode.PublicationFailure
          ? WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.IoFailure,
              "The generated-handler fixture rejected publication."))
          : WorldStorageOperationResult.Success;
      }

      public WorldStorageOperationResult SettleLoadedWorld(
        LoadedWorldSession session,
        CancellationToken cancellationToken)
      {
        cancellationToken.ThrowIfCancellationRequested();
        return WorldStorageOperationResult.Success;
      }

      public WorldStorageOperationResult FinalizeLoadedWorld(
        LoadedWorldSession session,
        CancellationToken cancellationToken)
      {
        cancellationToken.ThrowIfCancellationRequested();
        return fixture.Mode == GeneratedHandlerMode.FinalizeFailure
          ? WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "The generated-handler fixture rejected finalization."))
          : WorldStorageOperationResult.Success;
      }

      public WorldStorageOperationResult ResetWorld(LoadedWorldSession session)
      {
        return WorldStorageOperationResult.Success;
      }
    }
  }

  private sealed class SessionHarness
  {
    public EntityIdentityRegistry IdentityRegistry { get; } = new();

    public EntityReference PreviousReference { get; private set; }

    public LoadedWorldSession PublishSession(string label)
    {
      var fixture = new FixtureCatalog(FixtureMode.Success);
      var session = new LoadedWorldSession(IdentityRegistry);
      WorldLoadRecoveryCoordinator coordinator = CreateRecoveryCoordinator(fixture);
      WorldLoadRecoveryResult result = coordinator.Recover(
        session,
        $"world-session-{label}.wld",
        CreateBindings(session),
        new FixtureRecoveryEffects(fixture),
        CancellationToken.None);
      Require(result.Succeeded && session.IsComplete && session.IsPublished,
        "The real factory-created recovery coordinator must publish a complete session.");
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          session),
        "The published session must be visible through the host-scoped active registry.");

      CandidateEvidence candidate = fixture.LastCandidate ??
        throw new InvalidOperationException("The publish fixture did not allocate a root.");
      PreviousReference = candidate.Reference;
      return session;
    }

    public void VerifySuccessfulSessionSwitch(LoadedWorldSession previous)
    {
      EntityReference retiredReference = PreviousReference;
      LoadedWorldSession next = PublishSession("next");
      Require(!ReferenceEquals(previous, next) &&
          previous.WorldRuntimeId != next.WorldRuntimeId,
        "A successful world switch must allocate a fresh runtime identity.");
      Require(previous.IsDisposed,
        "The previous session must retire only after the new session is published.");
      Require(!next.IsDisposed && next.IsPublished,
        "The new session must remain live after a successful switch.");
      Require(next.EntityRuntime.TryResolve(retiredReference, out _) == false,
        "A reference from the retired runtime must be rejected by the new runtime.");
      Require(ReferenceEquals(
          WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession,
          next),
        "The host registry must point to the newest published session.");
    }
  }

  private sealed class CandidateEvidence(
    LoadedWorldSession session,
    EntityRuntime runtime,
    WorldStorageRoot storage,
    RuntimeEntityHandle root,
    EntityReference reference,
    StorageNpcSlot npcSlot,
    uint npcGeneration,
    OwnerProjectileIdentity projectileIdentity,
    ProjectileHandle projectileHandle,
    TileEntityId tileEntityId,
    EntityReference tileEntityReference)
  {
    public LoadedWorldSession Session { get; } = session;
    public EntityRuntime Runtime { get; } = runtime;
    public int AllocatedRuntimeEntityCount { get; } = runtime.EntityCount;
    public WorldStorageRoot Storage { get; } = storage;
    public RuntimeEntityHandle Root { get; } = root;
    public EntityReference Reference { get; } = reference;
    public StorageNpcSlot NpcSlot { get; } = npcSlot;
    public uint NpcGeneration { get; } = npcGeneration;
    public OwnerProjectileIdentity ProjectileIdentity { get; } = projectileIdentity;
    public ProjectileHandle ProjectileHandle { get; } = projectileHandle;
    public TileEntityId TileEntityId { get; } = tileEntityId;
    public EntityReference TileEntityReference { get; } = tileEntityReference;
    public bool Cleaned { get; set; }
  }

  private sealed class DirtySessionEvidence(
    LoadedWorldSession session,
    RuntimeEntityHandle root,
    EntityReference reference)
  {
    public LoadedWorldSession Session { get; } = session;
    public RuntimeEntityHandle Root { get; } = root;
    public EntityReference Reference { get; } = reference;
  }

  private sealed class FixtureWorldSection
  {
  }

  private readonly record struct TestWorldRootComponent(string Label);
}
