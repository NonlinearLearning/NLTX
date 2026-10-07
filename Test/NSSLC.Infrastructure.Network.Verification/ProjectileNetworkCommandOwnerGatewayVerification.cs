using System.Collections.Concurrent;
using System.Numerics;
using NSSLC.Infrastructure.Network;
using Terraria.Content;
using Terraria.Network;
using Terraria.NonAuthoritative.Persistence;
using Terraria.Projectile;
using Terraria.Relationships;
using Terraria.WorldStorage;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.NetworkVerification;

internal static class ProjectileNetworkCommandOwnerGatewayVerification
{
  private const int FirstProjectileType = 201;
  private const int ReplacementProjectileType = 202;
  private const int HostileProjectileType = 203;
  private const int InvalidHydrationProjectileType = 204;
  private const int ServerProjectileType = 949;

  private readonly record struct ProjectileSnapshot(
    bool Exists,
    ProjectileHandle Handle,
    RuntimeEntityHandle RuntimeHandle,
    EntityReference EntityReference,
    int ProjectileType,
    int OwnerSlot,
    int Identity,
    int SlotIndex,
    ProjectileHandle IndexedHandle);

  private readonly record struct WorldCounts(int Slots, int Identities, int RuntimeEntities);

  private sealed class OwnerThreadScheduler : IProjectileNetworkOwnerThreadScheduler, IDisposable
  {
    private readonly BlockingCollection<Action> _work = new();
    private readonly ManualResetEventSlim _executionGate = new(initialState: true);
    private readonly object _pauseGate = new();
    private readonly Thread _thread;
    private TaskCompletionSource? _blockedWork;
    private Action? _beforeNextWork;
    private int _ownerThreadId;

    public OwnerThreadScheduler()
    {
      _thread = new Thread(Run)
      {
        IsBackground = true,
        Name = "Projectile network owner verification"
      };
      _thread.Start();
    }

    public bool IsOwnerThread => Environment.CurrentManagedThreadId == _ownerThreadId;

    public ValueTask<PacketHandlingResult> ExecuteAsync(
      Func<PacketHandlingResult> operation,
      CancellationToken cancellationToken)
    {
      ArgumentNullException.ThrowIfNull(operation);
      cancellationToken.ThrowIfCancellationRequested();
      var completion = new TaskCompletionSource<PacketHandlingResult>(
        TaskCreationOptions.RunContinuationsAsynchronously);
      _work.Add(() =>
      {
        if (cancellationToken.IsCancellationRequested)
        {
          completion.TrySetCanceled(cancellationToken);
          return;
        }

        try
        {
          EnsureOwnerThread();
          cancellationToken.ThrowIfCancellationRequested();
          completion.TrySetResult(operation());
        }
        catch (OperationCanceledException exception)
        {
          completion.TrySetCanceled(exception.CancellationToken);
        }
        catch (Exception exception)
        {
          completion.TrySetException(exception);
        }
      });
      return new ValueTask<PacketHandlingResult>(completion.Task);
    }

    public Task<T> InvokeOwnerAsync<T>(Func<T> operation)
    {
      ArgumentNullException.ThrowIfNull(operation);
      var completion = new TaskCompletionSource<T>(
        TaskCreationOptions.RunContinuationsAsynchronously);
      _work.Add(() =>
      {
        try
        {
          EnsureOwnerThread();
          completion.TrySetResult(operation());
        }
        catch (Exception exception)
        {
          completion.TrySetException(exception);
        }
      });
      return completion.Task;
    }

    public Task InvokeOwnerAsync(Action operation)
    {
      ArgumentNullException.ThrowIfNull(operation);
      return InvokeOwnerAsync(() =>
      {
        operation();
        return true;
      });
    }

    public void Pause()
    {
      lock (_pauseGate)
      {
        if (!_executionGate.IsSet)
        {
          throw new InvalidOperationException("The owner scheduler is already paused.");
        }

        _blockedWork = new TaskCompletionSource(
          TaskCreationOptions.RunContinuationsAsynchronously);
        _executionGate.Reset();
      }
    }

    public Task WaitForBlockedWorkAsync()
    {
      lock (_pauseGate)
      {
        return _blockedWork?.Task ?? throw new InvalidOperationException(
          "Pause the owner scheduler before waiting for a blocked item.");
      }
    }

    public Task ResumeBeforeNextWorkAsync(Action operation)
    {
      ArgumentNullException.ThrowIfNull(operation);
      var completion = new TaskCompletionSource(
        TaskCreationOptions.RunContinuationsAsynchronously);
      lock (_pauseGate)
      {
        if (_executionGate.IsSet || _blockedWork?.Task.IsCompleted != true ||
            _beforeNextWork is not null)
        {
          throw new InvalidOperationException(
            "The owner scheduler must be paused at a blocked item before installing " +
            "a pre-work action.");
        }

        _beforeNextWork = () =>
        {
          try
          {
            operation();
            completion.TrySetResult();
          }
          catch (Exception exception)
          {
            completion.TrySetException(exception);
          }
        };
        _executionGate.Set();
      }

      return completion.Task;
    }

    public void Resume()
    {
      lock (_pauseGate)
      {
        _executionGate.Set();
      }
    }

    public void Dispose()
    {
      Resume();
      _work.CompleteAdding();
      _thread.Join();
      _executionGate.Dispose();
      _work.Dispose();
    }

    public void EnsureOwnerThread()
    {
      if (!IsOwnerThread)
      {
        throw new InvalidOperationException(
          "Projectile lifecycle work escaped the EntityRuntime owner thread.");
      }
    }

    private void Run()
    {
      Volatile.Write(ref _ownerThreadId, Environment.CurrentManagedThreadId);
      foreach (Action work in _work.GetConsumingEnumerable())
      {
        TaskCompletionSource? blockedWork = null;
        lock (_pauseGate)
        {
          if (!_executionGate.IsSet)
          {
            blockedWork = _blockedWork;
          }
        }

        blockedWork?.TrySetResult();
        _executionGate.Wait();
        Action? beforeNextWork;
        lock (_pauseGate)
        {
          beforeNextWork = _beforeNextWork;
          _beforeNextWork = null;
        }

        beforeNextWork?.Invoke();
        work();
      }
    }
  }

  private sealed class CurrentSessionQuery : IProjectileNetworkCommandSessionQuery
  {
    private readonly object _senderGate = new();
    private readonly OwnerThreadScheduler _scheduler;
    private readonly IProjectileDefinitionQuery _definitions;
    private readonly ProjectileDefinitionHydrationContext _hydrationContext;
    private LoadedWorldSession? _currentWorld;
    private ConnectionIdentity? _connection;
    private SenderBinding? _actor;
    private string? _profileKey;
    private NetworkSessionContext? _lastActiveSender;
    private int _resolveCount;

    public CurrentSessionQuery(
      OwnerThreadScheduler scheduler,
      IProjectileDefinitionQuery definitions,
      ProjectileDefinitionHydrationContext hydrationContext)
    {
      _scheduler = scheduler;
      _definitions = definitions;
      _hydrationContext = hydrationContext;
    }

    public int ResolveCount => Volatile.Read(ref _resolveCount);

    public NetworkSessionContext LastActiveSender =>
      Volatile.Read(ref _lastActiveSender) ?? throw new InvalidOperationException(
        "The gateway has not submitted an active projectile command yet.");

    public EntityRuntimeId? CaptureWorldRuntimeId()
    {
      LoadedWorldSession? currentWorld = Volatile.Read(ref _currentWorld);
      return currentWorld is { IsDisposed: false } ? currentWorld.WorldRuntimeId : null;
    }

    public void CaptureAuthenticatedSender(NetworkSessionContext sender)
    {
      lock (_senderGate)
      {
        _connection = sender.Connection;
        _actor = sender.Actor;
        _profileKey = sender.ProfileKey;
      }
    }

    public void SetCurrentWorld(LoadedWorldSession world)
    {
      _scheduler.EnsureOwnerThread();
      ArgumentNullException.ThrowIfNull(world);
      if (!world.IsComplete || !world.IsPublished || world.IsPublicationUncertain ||
          world.IsDisposed)
      {
        throw new InvalidOperationException(
          "Only a complete, published world session may become current in the fixture.");
      }

      Volatile.Write(ref _currentWorld, world);
    }

    public bool TryResolveCurrent(
      NetworkSessionContext sender,
      out ProjectileNetworkCommandSession? session)
    {
      _scheduler.EnsureOwnerThread();
      ConnectionIdentity? currentConnection;
      SenderBinding? currentActor;
      string? currentProfileKey;
      lock (_senderGate)
      {
        currentConnection = _connection;
        currentActor = _actor;
        currentProfileKey = _profileKey;
      }

      LoadedWorldSession? currentWorld = Volatile.Read(ref _currentWorld);
      Interlocked.Increment(ref _resolveCount);
      if (sender.Stage != NetworkSessionStage.Active ||
          currentConnection != sender.Connection ||
          currentActor != sender.Actor ||
          currentProfileKey != sender.ProfileKey ||
          !sender.WorldRuntimeId.HasValue ||
          currentWorld is null ||
          currentWorld.IsDisposed ||
          !currentWorld.IsComplete ||
          !currentWorld.IsPublished ||
          currentWorld.IsPublicationUncertain ||
          sender.WorldRuntimeId.Value != currentWorld.WorldRuntimeId)
      {
        session = null;
        return false;
      }

      session = new ProjectileNetworkCommandSession(
        sender,
        currentWorld,
        _definitions,
        _hydrationContext);
      Volatile.Write(ref _lastActiveSender, sender);
      return true;
    }
  }

  private sealed class FixtureSessionSection
  {
  }

  private sealed class SessionCompletionApiCatalog : IWorldLoadApiCatalog
  {
    public IReadOnlyList<WorldLoadApiDescriptor> Descriptors { get; } = Array.AsReadOnly(
      new[]
      {
        new WorldLoadApiDescriptor(
          "fixture.session.complete",
          LoadedWorldSession.OwnerContextId,
          "fixture.session-complete",
          minimumFormatVersion: 1,
          maximumFormatVersion: 1,
          requirement: WorldLoadSectionRequirement.Required)
      });

    public WorldLoadApiExecutionResult Execute(WorldLoadApiBindings bindings)
    {
      if (!bindings.TryGetOwnerContext(
            LoadedWorldSession.OwnerContextId,
            out LoadedWorldSession session) ||
          !bindings.TryGetSection<FixtureSessionSection>(
            "fixture.session-complete",
            out WorldLoadSection<FixtureSessionSection> section) ||
          !section.IsPresent)
      {
        throw new InvalidOperationException(
          "The fixture world-load API did not receive its required owner and section.");
      }

      var footer = WorldLoadSection<WorldFileFooterSection>.Present(
        new WorldFileFooterSection(true, session.World.Descriptor.Name,
          session.World.Descriptor.WorldId));
      var footerApi = new WorldFooterLoadApi();
      footerApi.PrepareLoad(session, footer);
      footerApi.CommitLoad(session, in footer);
      return WorldLoadApiExecutionResult.Completed();
    }
  }

  private sealed class FixtureDocumentDecoder(WorldPersistenceDocument document)
    : IWorldPersistenceDocumentDecoder
  {
    public WorldPersistenceDecodeResult Decode(ReadOnlyMemory<byte> fileBytes)
    {
      if (fileBytes.IsEmpty)
      {
        return WorldPersistenceDecodeResult.Failed(
          WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData));
      }

      return WorldPersistenceDecodeResult.Decoded(document);
    }
  }

  private sealed class FixtureDocumentValidator : IWorldPersistenceDocumentValidator
  {
    public WorldStorageFailure Validate(WorldPersistenceDocument document)
    {
      return document.FormatVersion == 1 &&
          document.TryGetSection<FixtureSessionSection>(
            "fixture.session-complete",
            out WorldLoadSection<FixtureSessionSection> section) &&
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

    public WorldStorageOperationResult WriteAllBytes(string path, ReadOnlyMemory<byte> data)
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

  private sealed class FixtureLifecycleProjection : IWorldLoadLifecycleProjection
  {
    public WorldStorageOperationResult Project(
      LoadedWorldSession session,
      bool isGeneratingOrLoadingWorld,
      bool loadFailed,
      bool worldBackup,
      bool worldCleared)
    {
      return WorldStorageOperationResult.Success;
    }
  }

  private sealed class FixtureRecoveryEffects : IWorldLoadRecoveryEffects
  {
    public WorldStorageOperationResult PublishLoadedWorld(
      LoadedWorldSession session,
      CancellationToken cancellationToken)
    {
      cancellationToken.ThrowIfCancellationRequested();
      return WorldStorageOperationResult.Success;
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
      return WorldStorageOperationResult.Success;
    }
  }

  public static async Task RunAsync()
  {
    using var scheduler = new OwnerThreadScheduler();
    ProjectileDefinitionCatalog definitions = CreateDefinitions();
    var hydrationContext = new ProjectileDefinitionHydrationContext(
      catalogRevision: 9,
      npcCapacity: 64,
      playerCapacity: 8);
    var sessions = new CurrentSessionQuery(scheduler, definitions, hydrationContext);
    LoadedWorldSession world = await scheduler.InvokeOwnerAsync(CreatePublishedWorldSession);
    await scheduler.InvokeOwnerAsync(() => sessions.SetCurrentWorld(world));

    var owner = new ProjectileNetworkCommandOwner(sessions, scheduler);
    var authority = new RecordingAuthority();
    ProtocolProfile profile = TerrariaProtocolProfile.Create(
      GeneratedProfileVerification.CreateFacts());
    await using var gateway = new PacketGateway(
      profile,
      authority,
      worldRuntimeIdProvider: sessions.CaptureWorldRuntimeId);
    RegisterProgression(gateway, sessions);
    ProjectilePacketGatewayRegistration.Register(
      gateway,
      owner,
      new PacketPolicy(27, NetworkSessionStage.Active),
      new PacketPolicy(29, NetworkSessionStage.Active));

    try
    {
      await using var peer = new GatewayPeer(gateway, profile);
      await peer.JoinAsync();
      Verify.That(peer.Session.Binding?.PlayerSlot == 0,
        "The real gateway must bind the projectile sender through its authority.");

      var firstCommand = CreateCommand(17, FirstProjectileType);
      SendProjectile(peer, firstCommand, forgedOwner: 231);
      ProjectileSnapshot first = await WaitForProjectileAsync(
        scheduler,
        world,
        slot: 0,
        snapshot => snapshot.Exists && snapshot.Identity == 17,
        "Packet 27 did not create a projectile through the Application owner.");
      Verify.That(first.OwnerSlot == 0 && first.ProjectileType == FirstProjectileType &&
          first.SlotIndex == 0 && first.IndexedHandle == first.Handle,
        "Packet 27 must replace the forged owner and publish the authenticated identity index.");

      int resolveCount = sessions.ResolveCount;
      SendProjectile(peer, CreateCommand(17, FirstProjectileType, x: 22f), forgedOwner: 232);
      ProjectileSnapshot updated = await WaitForProjectileAsync(
        scheduler,
        world,
        slot: 0,
        snapshot => sessions.ResolveCount > resolveCount && snapshot.Exists &&
          snapshot.Handle == first.Handle && snapshot.EntityReference == first.EntityReference &&
          snapshot.ProjectileType == FirstProjectileType,
        "A same-type packet 27 did not preserve the projectile generation and root.");
      Verify.That(updated.Handle == first.Handle && updated.EntityReference == first.EntityReference,
        "Same-type updates must retain their runtime handle and entity root.");

      resolveCount = sessions.ResolveCount;
      SendProjectile(peer, CreateCommand(17, ReplacementProjectileType, x: 35f), forgedOwner: 233);
      ProjectileSnapshot replacement = await WaitForProjectileAsync(
        scheduler,
        world,
        slot: 0,
        snapshot => sessions.ResolveCount > resolveCount && snapshot.Exists &&
          snapshot.ProjectileType == ReplacementProjectileType,
        "A changed projectile type did not replace the active generation.");
      Verify.That(replacement.Handle.Slot == first.Handle.Slot &&
          replacement.Handle.Generation == first.Handle.Generation + 1 &&
          replacement.RuntimeHandle != first.RuntimeHandle &&
          replacement.EntityReference != first.EntityReference &&
          replacement.IndexedHandle == replacement.Handle && replacement.SlotIndex == 0,
        "Type replacement must advance the generation and update both ECS root and protocol index.");

      await VerifyNoOpPacketAsync(
        peer,
        scheduler,
        sessions,
        world,
        CreateCommand(77, HostileProjectileType),
        "Hostile non-949 packet 27 must be silently accepted without allocating a root.");
      await VerifyNoOpPacketAsync(
        peer,
        scheduler,
        sessions,
        world,
        CreateCommand(78, projectileType: 499),
        "An unknown projectile type must be silently accepted without allocating a root.");

      await VerifyApplicationRejectionsAndFailuresAsync(
        owner,
        scheduler,
        sessions,
        world,
        replacement);

      int beforeServerProjectile = sessions.ResolveCount;
      SendProjectile(peer, CreateCommand(33, ServerProjectileType), forgedOwner: 0);
      ProjectileSnapshot serverProjectile = await WaitForProjectileAsync(
        scheduler,
        world,
        slot: 1,
        snapshot => sessions.ResolveCount > beforeServerProjectile && snapshot.Exists &&
          snapshot.Identity == 33,
        "The Version4 type-949 packet exception did not use the server owner slot.");
      Verify.That(serverProjectile.OwnerSlot == byte.MaxValue &&
          serverProjectile.ProjectileType == ServerProjectileType,
        "Type 949 must be stored under the server projectile owner slot.");
      bool serverProjectileTerminated = await scheduler.InvokeOwnerAsync(() =>
        new ProjectileLifecycleSystem(world.Storage).TryTerminateNetwork(
          new ProjectileNetworkTerminateCommand(byte.MaxValue, 33)));
      Verify.That(serverProjectileTerminated,
        "The fixture must clean up its server-owned type-949 projectile through lifecycle.");

      int beforeMissingTermination = sessions.ResolveCount;
      peer.Receive(new KillProjectilePacket { ProjectileIdentity = 999, Owner = 220 });
      await WaitForResolveAsync(scheduler, sessions, beforeMissingTermination);
      Verify.That(peer.Session.Stage == NetworkSessionStage.Active,
        "A missing packet-29 identity must remain an accepted no-op.");

      int beforeTermination = sessions.ResolveCount;
      peer.Receive(new KillProjectilePacket { ProjectileIdentity = 17, Owner = 220 });
      await WaitForEmptyWorldAsync(
        scheduler,
        sessions,
        world,
        beforeTermination,
        "Packet 29 did not remove the projectile root, slot, and identity index.");
      Verify.That(peer.Session.Stage == NetworkSessionStage.Active,
        "Authenticated packet-29 termination must not reject the connection.");

      int beforeRepeatedTermination = sessions.ResolveCount;
      peer.Receive(new KillProjectilePacket { ProjectileIdentity = 17, Owner = 220 });
      await WaitForResolveAsync(scheduler, sessions, beforeRepeatedTermination);
      await AssertEmptyWorldAsync(scheduler, world,
        "Repeated packet-29 termination must not recreate or retain projectile state.");
    }
    finally
    {
      await scheduler.InvokeOwnerAsync(() =>
      {
        if (!world.IsDisposed)
        {
          world.Dispose();
        }
      });
    }
  }

  private static void RegisterProgression(
    PacketGateway gateway,
    CurrentSessionQuery sessions)
  {
    gateway.Register(new PacketPolicy(6, NetworkSessionStage.AwaitPlayerData),
      new RecordingHandler<RequestWorldDataPacket>((sender, _, _) =>
        ValueTask.FromResult(new PacketHandlingResult(true,
          nextStage: NetworkSessionStage.AwaitSectionRequest))));
    gateway.Register(new PacketPolicy(8, NetworkSessionStage.AwaitSectionRequest),
      new RecordingHandler<SpawnTileDataPacket>((sender, _, _) =>
        ValueTask.FromResult(new PacketHandlingResult(true,
          nextStage: NetworkSessionStage.Synchronizing))));
    gateway.Register(new PacketPolicy(12, NetworkSessionStage.Synchronizing),
      new RecordingHandler<PlayerSpawnPacket>((sender, _, _) =>
      {
        sessions.CaptureAuthenticatedSender(sender);
        return ValueTask.FromResult(new PacketHandlingResult(true,
          nextStage: NetworkSessionStage.Active));
      }));
  }

  private static async Task VerifyApplicationRejectionsAndFailuresAsync(
    ProjectileNetworkCommandOwner owner,
    OwnerThreadScheduler scheduler,
    CurrentSessionQuery sessions,
    LoadedWorldSession world,
    ProjectileSnapshot existing)
  {
    NetworkSessionContext sender = sessions.LastActiveSender;
    WorldCounts before = await ReadWorldCountsAsync(scheduler, world);

    PacketHandlingResult wrongOwner = await owner.ApplyProjectileAsync(
      sender,
      CreateCommand(88, FirstProjectileType, ownerSlot: 1),
      CancellationToken.None);
    Verify.That(!wrongOwner.Accepted && wrongOwner.RejectionCode == "ProjectileOwnerMismatch",
      "The Application owner must reject a command whose owner is not the authenticated actor.");

    NetworkSessionContext oldEpoch = sender with
    {
      Connection = sender.Connection with { Epoch = sender.Connection.Epoch + 1 }
    };
    PacketHandlingResult staleEpoch = await owner.ApplyProjectileAsync(
      oldEpoch,
      CreateCommand(89, FirstProjectileType),
      CancellationToken.None);
    Verify.That(!staleEpoch.Accepted && staleEpoch.RejectionCode == "StaleProjectileSession",
      "The Application session query must reject a replaced connection epoch.");

    NetworkSessionContext wrongActor = sender with
    {
      Actor = sender.Actor with { GameSessionKey = Guid.NewGuid() }
    };
    PacketHandlingResult staleActor = await owner.ApplyProjectileAsync(
      wrongActor,
      CreateCommand(90, FirstProjectileType),
      CancellationToken.None);
    Verify.That(!staleActor.Accepted && staleActor.RejectionCode == "StaleProjectileSession",
      "The Application session query must reject an old actor binding.");

    NetworkSessionContext wrongRuntime = sender with
    {
      WorldRuntimeId = new EntityRuntimeId(Guid.NewGuid())
    };
    PacketHandlingResult staleRuntime = await owner.ApplyProjectileAsync(
      wrongRuntime,
      CreateCommand(91, FirstProjectileType),
      CancellationToken.None);
    Verify.That(!staleRuntime.Accepted && staleRuntime.RejectionCode == "StaleProjectileSession",
      "The Application session query must reject a mismatched world runtime token.");

    LoadedWorldSession replacementWorld =
      await scheduler.InvokeOwnerAsync(CreatePublishedWorldSession);
    try
    {
      await scheduler.InvokeOwnerAsync(() => sessions.SetCurrentWorld(replacementWorld));
      PacketHandlingResult oldWorld = await owner.ApplyProjectileAsync(
        sender,
        CreateCommand(92, FirstProjectileType),
        CancellationToken.None);
      Verify.That(!oldWorld.Accepted && oldWorld.RejectionCode == "StaleProjectileSession",
        "The Application session query must reject a command captured before world replacement.");
    }
    finally
    {
      await scheduler.InvokeOwnerAsync(() =>
      {
        sessions.SetCurrentWorld(world);
        replacementWorld.Dispose();
      });
    }

    await VerifyQueuedSessionInvalidationsAsync(owner, scheduler, sessions, world, existing);

    scheduler.Pause();
    using var cancellation = new CancellationTokenSource();
    Task<PacketHandlingResult> canceledWork = owner.ApplyProjectileAsync(
      sender,
      CreateCommand(93, FirstProjectileType),
      cancellation.Token).AsTask();
    try
    {
      await scheduler.WaitForBlockedWorkAsync();
      cancellation.Cancel();
    }
    finally
    {
      scheduler.Resume();
    }
    await Verify.ThrowsAsync<OperationCanceledException>(() => canceledWork);

    PacketHandlingResult failedHydration = await owner.ApplyProjectileAsync(
      sender,
      CreateCommand(94, InvalidHydrationProjectileType, projectileUuid: 8),
      CancellationToken.None);
    Verify.That(failedHydration.Accepted && failedHydration.Outbound.Count == 0,
      "A rejected hydration must retain source accepted-no-op behavior without dispatch.");

    var staleHandle = new ProjectileHandle(new ProjectileSlot(91), Generation: 1);
    bool registeredStaleIdentity = await scheduler.InvokeOwnerAsync(() =>
      world.Storage.ProjectileIdentities.TryRegister(
        new OwnerProjectileIdentity(new PlayerSlot(0), 95),
        staleHandle));
    Verify.That(registeredStaleIdentity,
      "The fixture must install the stale protocol index used to exercise lifecycle rejection.");
    await Verify.ThrowsAsync<InvalidOperationException>(() => owner.ApplyProjectileAsync(
      sender,
      CreateCommand(95, FirstProjectileType),
      CancellationToken.None).AsTask());
    WorldCounts failedIndexCounts = await ReadWorldCountsAsync(scheduler, world);
    Verify.That(failedIndexCounts.Slots == before.Slots &&
        failedIndexCounts.RuntimeEntities == before.RuntimeEntities &&
        failedIndexCounts.Identities == before.Identities + 1,
      "An inconsistent protocol index must not allocate a projectile slot or runtime root.");
    await scheduler.InvokeOwnerAsync(() =>
    {
      if (!world.Storage.ProjectileIdentities.TryUnregister(staleHandle, out _))
      {
        throw new InvalidOperationException("The fixture could not remove its injected stale index.");
      }
    });
    WorldCounts cleanedIndexCounts = await ReadWorldCountsAsync(scheduler, world);
    Verify.That(cleanedIndexCounts == before,
      "Removing the fixture's stale index must restore the pre-failure world counts.");

    ProjectileSnapshot stillActive = await ReadProjectileAsync(scheduler, world, slot: 0);
    Verify.That(stillActive.Exists && stillActive.Handle == existing.Handle &&
        stillActive.EntityReference == existing.EntityReference,
      "Rejected, stale, canceled, and failed commands must preserve the active projectile root.");
  }

  private static async Task VerifyQueuedSessionInvalidationsAsync(
    ProjectileNetworkCommandOwner owner,
    OwnerThreadScheduler scheduler,
    CurrentSessionQuery sessions,
    LoadedWorldSession world,
    ProjectileSnapshot existing)
  {
    NetworkSessionContext sender = sessions.LastActiveSender;
    WorldCounts before = await ReadWorldCountsAsync(scheduler, world);
    LoadedWorldSession replacementWorld =
      await scheduler.InvokeOwnerAsync(CreatePublishedWorldSession);
    try
    {
      scheduler.Pause();
      Task<PacketHandlingResult> queuedWorldCommand = owner.ApplyProjectileAsync(
        sender,
        CreateCommand(96, FirstProjectileType),
        CancellationToken.None).AsTask();
      try
      {
        await scheduler.WaitForBlockedWorkAsync();
        await scheduler.ResumeBeforeNextWorkAsync(
          () => sessions.SetCurrentWorld(replacementWorld));
      }
      finally
      {
        scheduler.Resume();
      }

      PacketHandlingResult staleWorld = await queuedWorldCommand;
      Verify.That(!staleWorld.Accepted && staleWorld.RejectionCode == "StaleProjectileSession",
        "A packet-27 command queued before world replacement must re-resolve and reject " +
        "its old world token.");
      WorldCounts oldWorldCounts = await ReadWorldCountsAsync(scheduler, world);
      WorldCounts replacementWorldCounts = await ReadWorldCountsAsync(scheduler, replacementWorld);
      Verify.That(oldWorldCounts == before && replacementWorldCounts == default,
        "A queued stale-world command must preserve the old world's state and leave the " +
        "replacement world empty.");
      ProjectileSnapshot retained = await ReadProjectileAsync(scheduler, world, slot: 0);
      Verify.That(retained.Exists && retained.Handle == existing.Handle &&
          retained.EntityReference == existing.EntityReference,
        "A queued stale-world command must preserve the old projectile root and handle.");
    }
    finally
    {
      scheduler.Resume();
      await scheduler.InvokeOwnerAsync(() =>
      {
        sessions.SetCurrentWorld(world);
        replacementWorld.Dispose();
      });
    }

    NetworkSessionContext replacedEpoch = sender with
    {
      Connection = sender.Connection with { Epoch = sender.Connection.Epoch + 1 }
    };
    scheduler.Pause();
    Task<PacketHandlingResult> queuedEpochCommand = owner.ApplyProjectileAsync(
      sender,
      CreateCommand(97, FirstProjectileType),
      CancellationToken.None).AsTask();
    try
    {
      await scheduler.WaitForBlockedWorkAsync();
      sessions.CaptureAuthenticatedSender(replacedEpoch);
    }
    finally
    {
      scheduler.Resume();
    }

    PacketHandlingResult staleEpoch = await queuedEpochCommand;
    sessions.CaptureAuthenticatedSender(sender);
    Verify.That(!staleEpoch.Accepted && staleEpoch.RejectionCode == "StaleProjectileSession",
      "A packet-27 command queued before connection epoch replacement must be rejected " +
      "on the owner thread.");
    WorldCounts unchangedWorldCounts = await ReadWorldCountsAsync(scheduler, world);
    ProjectileSnapshot unchangedRoot = await ReadProjectileAsync(scheduler, world, slot: 0);
    Verify.That(unchangedWorldCounts == before && unchangedRoot.Exists &&
        unchangedRoot.Handle == existing.Handle &&
        unchangedRoot.EntityReference == existing.EntityReference,
      "A queued stale-epoch command must preserve the current world's slots, index, " +
      "runtime count, and projectile root.");
  }

  private static async Task VerifyNoOpPacketAsync(
    GatewayPeer peer,
    OwnerThreadScheduler scheduler,
    CurrentSessionQuery sessions,
    LoadedWorldSession world,
    ProjectileNetworkApplyCommand command,
    string message)
  {
    int resolveCount = sessions.ResolveCount;
    SendProjectile(peer, command, forgedOwner: 230);
    await WaitForResolveAsync(scheduler, sessions, resolveCount);
    WorldCounts counts = await ReadWorldCountsAsync(scheduler, world);
    Verify.That(counts.Slots == 1 && counts.Identities == 1 && counts.RuntimeEntities == 1,
      message);
  }

  private static async Task WaitForEmptyWorldAsync(
    OwnerThreadScheduler scheduler,
    CurrentSessionQuery sessions,
    LoadedWorldSession world,
    int previousResolveCount,
    string message)
  {
    await WaitForResolveAsync(scheduler, sessions, previousResolveCount);
    await EventuallyAsync(async () =>
    {
      WorldCounts counts = await ReadWorldCountsAsync(scheduler, world);
      return counts == default;
    }, message);
  }

  private static async Task WaitForResolveAsync(
    OwnerThreadScheduler scheduler,
    CurrentSessionQuery sessions,
    int previousResolveCount)
  {
    await EventuallyAsync(
      () => Task.FromResult(sessions.ResolveCount > previousResolveCount),
      "The gateway did not submit the projectile command to its Application owner.");
    await scheduler.InvokeOwnerAsync(static () => true);
  }

  private static async Task<ProjectileSnapshot> WaitForProjectileAsync(
    OwnerThreadScheduler scheduler,
    LoadedWorldSession world,
    int slot,
    Func<ProjectileSnapshot, bool> predicate,
    string message)
  {
    ProjectileSnapshot result = default;
    await EventuallyAsync(async () =>
    {
      result = await ReadProjectileAsync(scheduler, world, slot);
      return predicate(result);
    }, message);
    return result;
  }

  private static async Task<ProjectileSnapshot> ReadProjectileAsync(
    OwnerThreadScheduler scheduler,
    LoadedWorldSession world,
    int slot)
  {
    return await scheduler.InvokeOwnerAsync(() => ReadProjectile(world, slot));
  }

  private static ProjectileSnapshot ReadProjectile(LoadedWorldSession world, int slot)
  {
    var lifecycle = new ProjectileLifecycleSystem(world.Storage);
    if (!lifecycle.TryGetRuntimeHandleAtSlot(
          slot,
          out ProjectileHandle handle,
          out RuntimeEntityHandle runtimeHandle))
    {
      return default;
    }

    if (!lifecycle.TryGetEntityReference(handle, out EntityReference entityReference) ||
        !lifecycle.TryGetIdentity(handle, out ProjectileIdentityComponent identity) ||
        !lifecycle.TryGetOwnerIdentity(handle, out OwnerProjectileIdentity ownerIdentity))
    {
      throw new InvalidOperationException(
        "The runtime projectile root lost its identity or entity reference.");
    }

    int projectileType = -1;
    bool definitionFound = world.EntityRuntime.TryInspect<ProjectileDefinitionComponent>(
      runtimeHandle,
      (in ProjectileDefinitionComponent definitionComponent) =>
      {
        projectileType = definitionComponent.ProjectileType;
      });
    if (!definitionFound ||
        !world.Storage.ProjectileIdentities.TryGetHandle(
          ownerIdentity,
          out ProjectileHandle indexedHandle))
    {
      throw new InvalidOperationException(
        "The runtime projectile root is missing its definition or protocol identity index.");
    }

    return new ProjectileSnapshot(
      true,
      handle,
      runtimeHandle,
      entityReference,
      projectileType,
      identity.OwnerSlot,
      identity.Identity,
      identity.SlotIndex,
      indexedHandle);
  }

  private static async Task<WorldCounts> ReadWorldCountsAsync(
    OwnerThreadScheduler scheduler,
    LoadedWorldSession world)
  {
    return await scheduler.InvokeOwnerAsync(() => new WorldCounts(
      world.Storage.Projectiles.ActiveCount,
      world.Storage.ProjectileIdentities.Count,
      world.EntityRuntime.EntityCount));
  }

  private static async Task AssertEmptyWorldAsync(
    OwnerThreadScheduler scheduler,
    LoadedWorldSession world,
    string message)
  {
    WorldCounts counts = await ReadWorldCountsAsync(scheduler, world);
    Verify.That(counts == default, message);
  }

  private static async Task EventuallyAsync(
    Func<Task<bool>> condition,
    string message)
  {
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    while (!await condition())
    {
      try
      {
        await Task.Delay(5, timeout.Token);
      }
      catch (OperationCanceledException)
      {
        throw new InvalidOperationException(message);
      }
    }
  }

  private static LoadedWorldSession CreatePublishedWorldSession()
  {
    var document = new WorldPersistenceDocument(
      formatVersion: 1,
      sections: new[]
      {
        WorldPersistenceSection.Create(
          "fixture.session-complete",
          new FixtureSessionSection())
      });
    var fileStore = new FixtureWorldFileStore();
    var apiCatalog = new SessionCompletionApiCatalog();
    var loadCoordinator = new WorldLoadCoordinator(
      fileStore,
      new FixtureDocumentDecoder(document),
      new FixtureDocumentValidator(),
      apiCatalog);
    var recoveryCoordinator = new WorldLoadRecoveryCoordinator(
      fileStore,
      loadCoordinator,
      new FixtureLifecycleProjection());
    var session = new LoadedWorldSession();
    WorldLoadApiRuntimeBindings bindings = new WorldLoadApiRuntimeBindingsBuilder()
      .AddOwnerContext(LoadedWorldSession.OwnerContextId, session)
      .Build();
    WorldLoadRecoveryResult recovery = recoveryCoordinator.Recover(
      session,
      "projectile-network-fixture.wld",
      bindings,
      new FixtureRecoveryEffects());
    if (!recovery.Succeeded || !session.IsComplete || !session.IsPublished ||
        session.IsPublicationUncertain)
    {
      session.Dispose();
      throw new InvalidOperationException(
        "The real Application world-load recovery path did not publish its fixture session.");
    }

    return session;
  }

  private static ProjectileDefinitionCatalog CreateDefinitions()
  {
    return new ProjectileDefinitionCatalog(new[]
    {
      CreateDefinition(FirstProjectileType, hostile: false),
      CreateDefinition(ReplacementProjectileType, hostile: false),
      CreateDefinition(HostileProjectileType, hostile: true),
      CreateDefinition(InvalidHydrationProjectileType, hostile: false),
      CreateDefinition(ServerProjectileType, hostile: false)
    });
  }

  private static ProjectileDefinition CreateDefinition(int typeId, bool hostile)
  {
    return new ProjectileDefinition(
      new ProjectileIdentityDefinition(typeId, null, NeedsUuid: false),
      new ProjectileGeometryDefinition(8, 8, 1f, true, false),
      new ProjectileBehaviorDefinition(1, 0, 120),
      new ProjectileCombatDefinition(0, 0f, Friendly: !hostile, Hostile: hostile),
      new ProjectilePenetrationDefinition(1, 1, true),
      new ProjectileCapabilitiesDefinition(false, false, false, false),
      new ProjectilePresentationDefinition(1, 0f, false));
  }

  private static ProjectileNetworkApplyCommand CreateCommand(
    int identity,
    int projectileType,
    float x = 12f,
    int ownerSlot = 0,
    int projectileUuid = -1)
  {
    return new ProjectileNetworkApplyCommand(
      ownerSlot,
      identity,
      projectileType,
      new Vector2(x, 24f),
      new Vector2(2f, -3f),
      damage: 18,
      originalDamage: 21,
      knockback: 1.5f,
      ai0: 4f,
      ai1: 5f,
      ai2: 6f,
      projectileUuid: projectileUuid);
  }

  private static void SendProjectile(
    GatewayPeer peer,
    ProjectileNetworkApplyCommand command,
    byte forgedOwner)
  {
    var codec = new ProjectilePacket27Codec();
    if (!codec.TryMapToPacket(command, out SyncProjectilePacket packet))
    {
      throw new InvalidOperationException("The packet-27 fixture command could not be mapped.");
    }

    packet.Owner = forgedOwner;
    peer.Receive(packet);
  }
}
