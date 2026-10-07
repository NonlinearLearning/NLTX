using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.Platform;
using Terraria.WorldGeneration.Components;
using Terraria.WorldSession.Components;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.WorldStorage;

public static class WorldStorageCoordinatorFactory
{
  private sealed record PendingWorldSessionRetirement(
    LoadedWorldSession Candidate,
    LoadedWorldSession? Previous);

  private static LoadedWorldSession? _activeGeneratedWorldSession;
  private static PendingWorldSessionRetirement? _pendingWorldSessionRetirement;
  [ThreadStatic]
  private static bool _deferWorldSessionDisposalUntilHostCleanup;
  [ThreadStatic]
  private static LoadedWorldSession? _deferredWorldSessionDisposal;
  [ThreadStatic]
  private static LoadedWorldSession? _worldSessionBeingReset;

  /// <summary>
  /// The complete generated-world owner retained after publication, including storage and
  /// session-scoped components that are not projected into legacy Main fields.
  /// </summary>
  public static LoadedWorldSession? ActiveGeneratedWorldSession =>
    Volatile.Read(ref _activeGeneratedWorldSession);

  /// <summary>
  /// Connects the legacy WorldFile entry point to generated section APIs and per-load session
  /// recovery. The gate must also be shared with transform and snapshot-save coordinators.
  /// </summary>
  /// <param name="resultObserver">
  /// Observes a terminal recovery result after recovery and I/O-gate release. Observer failures are
  /// reported diagnostically and do not change the completed load outcome; host state cleanup belongs
  /// in the reset effect so it runs before the world load gate is released.
  /// </param>
  /// <param name="exceptionObserver">
  /// Observes load and result-observer failures. Exceptions thrown by this observer are reported
  /// diagnostically and do not replace the load outcome or original exception.
  /// </param>
  public static void RegisterGeneratedWorldLoadHandler(
    IWorldStorageIoGate ioGate,
    Func<LoadedWorldSession, IWorldLoadRecoveryEffects> recoveryEffectsFactory,
    Action<WorldLoadRecoveryResult>? resultObserver = null,
    Action<Exception>? exceptionObserver = null,
    Func<CancellationToken>? cancellationTokenProvider = null,
    Func<LoadedWorldSession>? sessionFactory = null)
  {
    RegisterGeneratedWorldLoadHandler(
      ioGate,
      new GeneratedWorldLoadApiCatalog(),
      recoveryEffectsFactory,
      resultObserver,
      exceptionObserver,
      cancellationTokenProvider,
      sessionFactory);
  }

  /// <summary>
  /// Registers the legacy load handler with a host-provided catalog. Candidate creation, recovery,
  /// and failed-candidate disposal use the same path as the generated-catalog overload.
  /// </summary>
  public static void RegisterGeneratedWorldLoadHandler(
    IWorldStorageIoGate ioGate,
    IWorldLoadApiCatalog apiCatalog,
    Func<LoadedWorldSession, IWorldLoadRecoveryEffects> recoveryEffectsFactory,
    Action<WorldLoadRecoveryResult>? resultObserver = null,
    Action<Exception>? exceptionObserver = null,
    Func<CancellationToken>? cancellationTokenProvider = null,
    Func<LoadedWorldSession>? sessionFactory = null)
  {
    ArgumentNullException.ThrowIfNull(apiCatalog);
    RegisterGeneratedWorldLoadHandlerCore(
      ioGate,
      CreateLoadRecoveryCoordinator(apiCatalog),
      recoveryEffectsFactory,
      resultObserver,
      exceptionObserver,
      cancellationTokenProvider,
      sessionFactory);
  }

  /// <summary>
  /// Registers the legacy load handler with injected persistence ports and catalog while keeping
  /// the factory-owned candidate creation and disposal path.
  /// </summary>
  public static void RegisterGeneratedWorldLoadHandler(
    IWorldStorageIoGate ioGate,
    IWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog,
    Func<LoadedWorldSession, IWorldLoadRecoveryEffects> recoveryEffectsFactory,
    Action<WorldLoadRecoveryResult>? resultObserver = null,
    Action<Exception>? exceptionObserver = null,
    Func<CancellationToken>? cancellationTokenProvider = null,
    Func<LoadedWorldSession>? sessionFactory = null)
  {
    RegisterGeneratedWorldLoadHandlerCore(
      ioGate,
      CreateLoadRecoveryCoordinator(
        fileStore,
        documentDecoder,
        documentValidator,
        apiCatalog),
      recoveryEffectsFactory,
      resultObserver,
      exceptionObserver,
      cancellationTokenProvider,
      sessionFactory);
  }

  private static void RegisterGeneratedWorldLoadHandlerCore(
    IWorldStorageIoGate ioGate,
    WorldLoadRecoveryCoordinator recoveryCoordinator,
    Func<LoadedWorldSession, IWorldLoadRecoveryEffects> recoveryEffectsFactory,
    Action<WorldLoadRecoveryResult>? resultObserver,
    Action<Exception>? exceptionObserver,
    Func<CancellationToken>? cancellationTokenProvider,
    Func<LoadedWorldSession>? sessionFactory)
  {
    ArgumentNullException.ThrowIfNull(ioGate);
    ArgumentNullException.ThrowIfNull(recoveryCoordinator);
    ArgumentNullException.ThrowIfNull(recoveryEffectsFactory);
    var recoveryLock = new object();

    NSSLC.WorldGeneration.Main.RegisterWorldLoadHandler(
      () =>
      {
        WorldLoadRecoveryResult? result = null;
        Exception? operationException = null;
        bool runGeneratedWorldLoadReturned = false;
        // Keep terminal host cleanup ordered with the next load after releasing the I/O gate.
        lock (recoveryLock)
        {
          try
          {
            CancellationToken cancellationToken = cancellationTokenProvider?.Invoke() ??
              CancellationToken.None;
            using (ioGate.Enter(cancellationToken))
            {
              result = RunGeneratedWorldLoad(
                recoveryCoordinator,
                recoveryEffectsFactory,
                sessionFactory ?? (static () => new LoadedWorldSession()),
                cancellationToken,
                out operationException);
              runGeneratedWorldLoadReturned = true;
            }
          }
          catch (Exception exception)
          {
            // Setup can fail before this load projects the legacy lifecycle flags.
            NSSLC.WorldGeneration.WorldGen.loadFailed = true;
            if (!runGeneratedWorldLoadReturned)
            {
              NSSLC.WorldGeneration.WorldGen.worldBackup = false;
              NSSLC.WorldGeneration.WorldGen.worldCleared = false;
            }

            if (exceptionObserver is null)
            {
              throw;
            }

            NotifyExceptionObserver(exceptionObserver, exception);
            return;
          }

          if (operationException is not null)
          {
            if (exceptionObserver is null)
            {
              ExceptionDispatchInfo.Capture(operationException).Throw();
            }

            NotifyExceptionObserver(exceptionObserver, operationException);
          }
          else if (result is not null)
          {
            NotifyResultObserver(resultObserver, exceptionObserver, result);
          }
        }
      },
      ClearActiveGeneratedWorldSession);
  }

  private static void ClearActiveGeneratedWorldSession()
  {
    LoadedWorldSession? resettingSession = _worldSessionBeingReset;
    LoadedWorldSession? session = Volatile.Read(ref _activeGeneratedWorldSession);
    if (resettingSession is not null && !ReferenceEquals(session, resettingSession))
    {
      return;
    }

    while (session is not null)
    {
      session.EntityRuntime.EnsureCanDispose();
      LoadedWorldSession? exchanged = Interlocked.CompareExchange(
        ref _activeGeneratedWorldSession,
        null,
        session);
      if (ReferenceEquals(exchanged, session))
      {
        break;
      }

      if (resettingSession is not null)
      {
        return;
      }

      session = Volatile.Read(ref _activeGeneratedWorldSession);
    }

    if (session is null)
    {
      return;
    }

    if (!_deferWorldSessionDisposalUntilHostCleanup)
    {
      session?.Dispose();
    }
    else
    {
      _deferredWorldSessionDisposal = session;
    }
  }

  private static void NotifyResultObserver(
    Action<WorldLoadRecoveryResult>? resultObserver,
    Action<Exception>? exceptionObserver,
    WorldLoadRecoveryResult result)
  {
    if (resultObserver is null)
    {
      return;
    }

    try
    {
      resultObserver(result);
    }
    catch (Exception observerException)
    {
      if (exceptionObserver is not null)
      {
        NotifyExceptionObserver(exceptionObserver, observerException);
        return;
      }

      WriteObserverFailure(observerException);
    }
  }

  private static void NotifyExceptionObserver(
    Action<Exception> exceptionObserver,
    Exception observedException)
  {
    try
    {
      exceptionObserver(observedException);
    }
    catch (Exception observerException)
    {
      WriteObserverFailure(new AggregateException(
        "World-load exception observation failed.",
        observedException,
        observerException));
    }
  }

  private static void WriteObserverFailure(Exception exception)
  {
    try
    {
      Trace.WriteLine(exception);
    }
    catch (Exception)
    {
      // A diagnostic sink cannot change an already completed world-load outcome.
    }
  }

  /// <summary>
  /// Registers generated loading with the legacy world reset already bound to clearWorld.
  /// The host remains responsible for publishing session state and host-specific pre-release
  /// cleanup. The standard legacy NPC/world initialization runs before the supplied finalization
  /// callback, which should only perform additional host-owned work.
  /// </summary>
  /// <param name="preparePreReleaseWeather">
  /// Performs host-specific cleanup after the adapter resets the legacy weather counter and clouds.
  /// </param>
  /// <param name="finalizeLoadedWorld">
  /// Performs additional host-owned finalization after the standard legacy NPC/world initialization.
  /// </param>
  /// <param name="resetHostWorldState">
  /// Clears host-owned world state after the legacy world reset and before the load gate is released.
  /// A failed result keeps the world gated and the reset incomplete.
  /// </param>
  /// <param name="resultObserver">
  /// Observes the terminal result after the I/O gate is released; use resetHostWorldState for cleanup
  /// that must complete before the world load gate is lowered.
  /// </param>
  public static void RegisterGeneratedWorldLoadHandler(
    IWorldStorageIoGate ioGate,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult> publishLoadedWorld,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      preparePreReleaseWeather,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      finalizeLoadedWorld,
    Action<WorldLoadRecoveryResult>? resultObserver = null,
    Action<Exception>? exceptionObserver = null,
    Func<CancellationToken>? cancellationTokenProvider = null,
    Func<LoadedWorldSession, WorldStorageOperationResult>? resetHostWorldState = null,
    Func<LoadedWorldSession>? sessionFactory = null)
  {
    ArgumentNullException.ThrowIfNull(publishLoadedWorld);
    ArgumentNullException.ThrowIfNull(preparePreReleaseWeather);
    ArgumentNullException.ThrowIfNull(finalizeLoadedWorld);
    RegisterGeneratedWorldLoadHandler(
      ioGate,
      _ => new LegacyWorldLoadRecoveryEffects(
        publishLoadedWorld,
        preparePreReleaseWeather,
        finalizeLoadedWorld,
        session => ResetPublishedWorld(session, resetHostWorldState)),
      resultObserver,
      exceptionObserver,
      cancellationTokenProvider,
      sessionFactory);
  }

  /// <summary>
  /// Registers loading with the legacy tile map projected before the host commits its remaining
  /// world state.
  /// </summary>
  /// <param name="preparePreReleaseWeather">
  /// Performs host-specific cleanup after the adapter resets the legacy weather counter and clouds.
  /// </param>
  /// <param name="finalizeLoadedWorld">
  /// Performs additional host-owned finalization after the standard legacy NPC/world initialization.
  /// </param>
  /// <param name="resetHostWorldState">
  /// Clears host-owned world state after the legacy world reset and before the load gate is released.
  /// A failed result keeps the world gated and the reset incomplete.
  /// </param>
  /// <param name="resultObserver">
  /// Observes the terminal result after the I/O gate is released; use resetHostWorldState for cleanup
  /// that must complete before the world load gate is lowered.
  /// </param>
  /// <param name="reprojectPreviousWorldState">
  /// Restores host-owned projections from the already-published previous session after a failed
  /// candidate reset. The callback must preserve existing roots and report a failure if their
  /// runtime references or projections no longer match.
  /// </param>
  public static void RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap(
    IWorldStorageIoGate ioGate,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      publishRemainingWorldState,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      preparePreReleaseWeather,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      finalizeLoadedWorld,
    Action<WorldLoadRecoveryResult>? resultObserver = null,
    Action<Exception>? exceptionObserver = null,
    Func<CancellationToken>? cancellationTokenProvider = null,
    Func<LoadedWorldSession, WorldStorageOperationResult>? resetHostWorldState = null,
    Func<LoadedWorldSession>? sessionFactory = null,
    Func<LoadedWorldSession, WorldStorageOperationResult>? reprojectPreviousWorldState = null)
  {
    ArgumentNullException.ThrowIfNull(publishRemainingWorldState);
    ArgumentNullException.ThrowIfNull(preparePreReleaseWeather);
    ArgumentNullException.ThrowIfNull(finalizeLoadedWorld);
    WorldLoadRecoveryCoordinator recoveryCoordinator = CreateLoadRecoveryCoordinator(
      new WorldFileStoreAdapter(),
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator(),
      new GeneratedWorldLoadApiCatalog(),
      previousSession => ReprojectPreviousRuntimeWorldState(
        previousSession,
        reprojectPreviousWorldState));
    RegisterGeneratedWorldLoadHandlerCore(
      ioGate,
      recoveryCoordinator,
      _ => new LegacyWorldLoadRecoveryEffects(
        (session, cancellationToken) => PublishRuntimeWorldState(
          session,
          cancellationToken,
          publishRemainingWorldState),
        preparePreReleaseWeather,
        finalizeLoadedWorld,
        session => ResetPublishedWorld(session, resetHostWorldState)),
      resultObserver,
      exceptionObserver,
      cancellationTokenProvider,
      sessionFactory);
  }

  private static WorldLoadRecoveryResult? RunGeneratedWorldLoad(
    WorldLoadRecoveryCoordinator recoveryCoordinator,
    Func<LoadedWorldSession, IWorldLoadRecoveryEffects> recoveryEffectsFactory,
    Func<LoadedWorldSession> sessionFactory,
    CancellationToken cancellationToken,
    out Exception? operationException)
  {
    operationException = null;
    bool preservePreexistingLoadGate =
      NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld;
    LoadedWorldSession loadedSession = sessionFactory() ??
      throw new InvalidOperationException("The session factory returned null.");
    if (!loadedSession.IsFresh)
    {
      throw new InvalidOperationException("The session factory must return a fresh world session.");
    }

    WorldLoadLifecycleComponent lifecycle = loadedSession.Lifecycle;
    IWorldLoadRecoveryEffects? recoveryEffects = null;
    WorldLoadRecoveryResult? result = null;
    try
    {
      WorldLoadApiRuntimeBindings bindings = CreateGeneratedWorldLoadBindings(loadedSession);
      recoveryEffects =
        recoveryEffectsFactory(loadedSession) ??
        throw new InvalidOperationException("The host did not provide world-load recovery effects.");
      result = recoveryCoordinator.Recover(
        loadedSession,
        NSSLC.WorldGeneration.Main.worldPathName,
        bindings,
        recoveryEffects,
        cancellationToken,
        preservePreexistingLoadGate);
      NSSLC.WorldGeneration.WorldGen.worldBackup = lifecycle.WorldBackup;
    }
    catch (Exception exception)
    {
      NSSLC.WorldGeneration.WorldGen.loadFailed = true;
      NSSLC.WorldGeneration.WorldGen.worldBackup = lifecycle.WorldBackup;
      NSSLC.WorldGeneration.WorldGen.worldCleared = lifecycle.WorldCleared;
      operationException = recoveryEffects is null
        ? exception
        : recoveryCoordinator.RecoverUnexpectedFailure(
          loadedSession,
          recoveryEffects,
          exception);
      return null;
    }
    finally
    {
      bool candidatePublished = result?.Succeeded == true &&
        loadedSession.IsPublished &&
        ReferenceEquals(Volatile.Read(ref _activeGeneratedWorldSession), loadedSession);
      if (!candidatePublished)
      {
        loadedSession.Dispose();
      }
    }

    NSSLC.WorldGeneration.WorldGen.loadFailed = !result!.Succeeded;
    return result;
  }

  private static WorldStorageOperationResult ResetLegacyWorld(LoadedWorldSession session)
  {
    ArgumentNullException.ThrowIfNull(session);
    NSSLC.WorldGeneration.WorldGen.clearWorld();
    return NSSLC.WorldGeneration.WorldGen.worldCleared
      ? WorldStorageOperationResult.Success
      : WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The legacy world reset did not mark the world cleared."));
  }

  private static WorldStorageOperationResult ResetPublishedWorld(
    LoadedWorldSession session,
    Func<LoadedWorldSession, WorldStorageOperationResult>? resetHostWorldState)
  {
    LoadedWorldSession? previousResetSession = _worldSessionBeingReset;
    _worldSessionBeingReset = session;
    _deferWorldSessionDisposalUntilHostCleanup = true;
    _deferredWorldSessionDisposal = null;
    try
    {
      WorldStorageOperationResult legacyReset = ResetLegacyWorld(session);
      if (!legacyReset.Succeeded ||
          legacyReset.Failure.Kind != WorldStorageFailureKind.None)
      {
        return legacyReset;
      }

      if (resetHostWorldState is null)
      {
        return legacyReset;
      }

      return resetHostWorldState(session);
    }
    finally
    {
      _deferWorldSessionDisposalUntilHostCleanup = false;
      LoadedWorldSession? retiredSession = _deferredWorldSessionDisposal;
      _deferredWorldSessionDisposal = null;
      try
      {
        // Recovery projects the terminal lifecycle and previous world after this reset returns.
        // Keep the candidate readable until RunGeneratedWorldLoad's finally disposes it.
        if (!ReferenceEquals(retiredSession, session))
        {
          retiredSession?.Dispose();
        }
      }
      finally
      {
        _worldSessionBeingReset = previousResetSession;
      }
    }
  }

  private static WorldStorageOperationResult PublishRuntimeWorldState(
    LoadedWorldSession session,
    CancellationToken cancellationToken,
    Func<LoadedWorldSession, CancellationToken, WorldStorageOperationResult>
      publishRemainingWorldState)
  {
    WorldStorageOperationResult tileMapResult =
      LegacyWorldTileMapProjection.PublishRuntimeTileMap(session, cancellationToken);
    if (!tileMapResult.Succeeded ||
        tileMapResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return tileMapResult;
    }

    WorldStorageOperationResult tileEntityResult =
      LegacyWorldTileEntityProjection.Publish(session, cancellationToken);
    if (!tileEntityResult.Succeeded ||
        tileEntityResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return tileEntityResult;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
    }

    WorldStorageOperationResult coreStateResult =
      LegacyWorldSessionProjection.PublishCoreState(session);
    if (!coreStateResult.Succeeded ||
        coreStateResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return coreStateResult;
    }

    WorldStorageOperationResult chestAndSignResult =
      LegacyWorldChestAndSignProjection.Publish(session);
    if (!chestAndSignResult.Succeeded ||
        chestAndSignResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return chestAndSignResult;
    }

    WorldStorageOperationResult pressurePlateResult =
      LegacyWorldPressurePlateProjection.Publish(session);
    if (!pressurePlateResult.Succeeded ||
        pressurePlateResult.Failure.Kind != WorldStorageFailureKind.None)
    {
      return pressurePlateResult;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
    }

    return publishRemainingWorldState(session, cancellationToken);
  }

  private static WorldStorageOperationResult ReprojectPreviousRuntimeWorldState(
    LoadedWorldSession previousSession,
    Func<LoadedWorldSession, WorldStorageOperationResult>? reprojectPreviousWorldState)
  {
    ArgumentNullException.ThrowIfNull(previousSession);

    WorldStorageOperationResult result =
      LegacyWorldTileMapProjection.ReprojectPublishedRuntimeTileMap(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    result = LegacyWorldTileEntityProjection.ReprojectPublished(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    result = LegacyWorldSessionProjection.ReprojectPublishedCoreState(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    result = LegacyWorldChestAndSignProjection.ReprojectPublished(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    result = LegacyWorldPressurePlateProjection.ReprojectPublished(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    result = LegacyWorldTreeTopsProjection.Publish(previousSession);
    if (!result.Succeeded || result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    return reprojectPreviousWorldState?.Invoke(previousSession) ??
      WorldStorageOperationResult.Success;
  }

  public static Terraria.WorldStorage.WorldLoadApiRuntimeBindings CreateGeneratedWorldLoadBindings(
    LoadedWorldSession target)
  {
    ArgumentNullException.ThrowIfNull(target);
    if (!target.IsFresh)
    {
      throw new ArgumentException("A fresh unpublished world session is required.", nameof(target));
    }
    var builder = new WorldLoadApiRuntimeBindingsBuilder();
    builder.AddOwnerContext(LoadedWorldSession.OwnerContextId, target);
    builder.AddApi(new WorldBackgroundLoadApi());
    builder.AddOwnerContext(WorldBackgroundLoadApi.OwnerId, target);
    builder.AddApi(new WorldBannerLoadApi());
    builder.AddOwnerContext(WorldBannerLoadApi.OwnerId, target);
    builder.AddApi(new WorldBestiaryLoadApi());
    builder.AddOwnerContext(WorldBestiaryLoadApi.OwnerId, target);
    builder.AddApi(new WorldBossProgressionLoadApi());
    builder.AddOwnerContext(WorldBossProgressionLoadApi.OwnerId, target);
    builder.AddApi(new WorldChestLoadApi());
    builder.AddOwnerContext(WorldChestLoadApi.OwnerId, target);
    builder.AddApi(new WorldCreativePowersLoadApi());
    builder.AddOwnerContext(WorldCreativePowersLoadApi.OwnerId, target);
    builder.AddApi(new WorldDefenderEventLoadApi());
    builder.AddOwnerContext(WorldDefenderEventLoadApi.OwnerId, target);
    builder.AddApi(new WorldEnvironmentLoadApi());
    builder.AddOwnerContext(WorldEnvironmentLoadApi.OwnerId, target);
    builder.AddApi(new WorldEventLoadApi());
    builder.AddOwnerContext(WorldEventLoadApi.OwnerId, target);
    builder.AddApi(new WorldFooterLoadApi());
    builder.AddOwnerContext(WorldFooterLoadApi.OwnerId, target);
    builder.AddApi(new WorldHeaderLoadApi());
    builder.AddOwnerContext(WorldHeaderLoadApi.OwnerId, target);
    builder.AddApi(new WorldMetadataLoadApi());
    builder.AddOwnerContext(WorldMetadataLoadApi.OwnerId, target);
    builder.AddApi(new WorldNpcLoadApi());
    builder.AddOwnerContext(WorldNpcLoadApi.OwnerId, target);
    builder.AddApi(new WorldNpcUnlockLoadApi());
    builder.AddOwnerContext(WorldNpcUnlockLoadApi.OwnerId, target);
    builder.AddApi(new WorldPartyLoadApi());
    builder.AddOwnerContext(WorldPartyLoadApi.OwnerId, target);
    builder.AddApi(new WorldPressurePlateLoadApi());
    builder.AddOwnerContext(WorldPressurePlateLoadApi.OwnerId, target);
    builder.AddApi(new WorldProgressionLoadApi());
    builder.AddOwnerContext(WorldProgressionLoadApi.OwnerId, target);
    builder.AddApi(new WorldQuestLoadApi());
    builder.AddOwnerContext(WorldQuestLoadApi.OwnerId, target);
    builder.AddApi(new WorldSandstormLoadApi());
    builder.AddOwnerContext(WorldSandstormLoadApi.OwnerId, target);
    builder.AddApi(new WorldSeasonalLoadApi());
    builder.AddOwnerContext(WorldSeasonalLoadApi.OwnerId, target);
    builder.AddApi(new WorldSignLoadApi());
    builder.AddOwnerContext(WorldSignLoadApi.OwnerId, target);
    builder.AddApi(new WorldSpawnLoadApi());
    builder.AddOwnerContext(WorldSpawnLoadApi.OwnerId, target);
    builder.AddApi(new WorldTileEntityLoadApi(
      new WorldFileTileEntityCodec(),
      new WorldFileTileEntityAnchorValidator()));
    builder.AddOwnerContext(WorldTileEntityLoadApi.OwnerId, target);
    builder.AddApi(new WorldTilePayloadLoadApi(new WorldFileTileCodec()));
    builder.AddOwnerContext(WorldTilePayloadLoadApi.OwnerId, target);
    builder.AddApi(new WorldTimePolicyLoadApi());
    builder.AddOwnerContext(WorldTimePolicyLoadApi.OwnerId, target);
    builder.AddApi(new WorldTownManagerLoadApi());
    builder.AddOwnerContext(WorldTownManagerLoadApi.OwnerId, target);
    builder.AddApi(new WorldTreeTopsLoadApi());
    builder.AddOwnerContext(WorldTreeTopsLoadApi.OwnerId, target.TreeTops);
    return builder.Build();
  }

  /// <summary>
  /// Creates the per-world gate that must be shared by transform and snapshot-save coordinators.
  /// </summary>
  public static WorldStorageIoGate CreateWorldStorageIoGate()
  {
    return new WorldStorageIoGate();
  }

  /// <summary>
  /// Creates a gate backed by the host's legacy world-file lock.
  /// </summary>
  public static WorldStorageIoGate CreateWorldStorageIoGate(object legacyWorldFileIoLock)
  {
    ArgumentNullException.ThrowIfNull(legacyWorldFileIoLock);
    return new WorldStorageIoGate(legacyWorldFileIoLock);
  }

  public static IWorldTransformationScheduler CreateTransformationScheduler(
    Action<Action> queueMainThreadAction)
  {
    return new WorldTransformationSchedulerAdapter(queueMainThreadAction);
  }

  /// <summary>
  /// Registers the legacy WorldGen transform entry with the transaction coordinator. The supplied
  /// I/O gate must also be passed to snapshot saves for the same world.
  /// </summary>
  public static WorldTransformWorkCoordinator RegisterWorldTransformationHandler(
    WorldTransformTransactionComponent transactions,
    IWorldStorageIoGate ioGate,
    IWorldTransformationScheduler scheduler)
  {
    ArgumentNullException.ThrowIfNull(transactions);
    ArgumentNullException.ThrowIfNull(ioGate);
    ArgumentNullException.ThrowIfNull(scheduler);
    var coordinator = new WorldTransformWorkCoordinator(transactions, ioGate, scheduler);
    NSSLC.WorldGeneration.WorldGen.RegisterWorldTransformationHandler(
      (transform, mainThreadFollowup, transactionCompleted) =>
        coordinator.Start(transform, mainThreadFollowup, transactionCompleted));
    return coordinator;
  }

  public static WorldSaveCoordinator CreateSaveCoordinator()
  {
    return CreateSaveCoordinator(new WorldFileStoreAdapter());
  }

  public static WorldSaveCoordinator CreateSaveCoordinator(IWorldFileStore fileStore)
  {
    ArgumentNullException.ThrowIfNull(fileStore);
    return new WorldSaveCoordinator(fileStore);
  }

  public static IWorldSaveEncoder CreateSaveEncoder()
  {
    return new WorldFileDocumentEncoder();
  }

  public static WorldSaveValidationQuery CreateSaveValidationQuery()
  {
    return CreateSaveValidationQuery(
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator());
  }

  public static WorldSaveValidationQuery CreateSaveValidationQuery(
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator)
  {
    ArgumentNullException.ThrowIfNull(documentDecoder);
    ArgumentNullException.ThrowIfNull(documentValidator);
    return new WorldSaveValidationQuery(bytes =>
    {
      WorldPersistenceDecodeResult decoded = documentDecoder.Decode(bytes);
      return decoded.Succeeded &&
        decoded.Document is not null &&
        documentValidator.Validate(decoded.Document).Kind == WorldStorageFailureKind.None;
    });
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldLoadApiCatalog apiCatalog)
  {
    return CreateLoadCoordinator(
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator(),
      apiCatalog);
  }

  public static WorldLoadRecoveryCoordinator CreateLoadRecoveryCoordinator(
    IWorldLoadApiCatalog apiCatalog)
  {
    return CreateLoadRecoveryCoordinator(
      new WorldFileStoreAdapter(),
      new WorldFileDocumentDecoder(),
      new WorldFileDocumentValidator(),
      apiCatalog);
  }

  public static WorldLoadRecoveryCoordinator CreateLoadRecoveryCoordinator(
    IWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog,
    Func<LoadedWorldSession, WorldStorageOperationResult>? reprojectPreviousWorldState = null)
  {
    ArgumentNullException.ThrowIfNull(fileStore);
    ArgumentNullException.ThrowIfNull(documentDecoder);
    ArgumentNullException.ThrowIfNull(documentValidator);
    ArgumentNullException.ThrowIfNull(apiCatalog);
    var loadCoordinator = new WorldLoadCoordinator(
      fileStore,
      documentDecoder,
      documentValidator,
      apiCatalog);
    return new WorldLoadRecoveryCoordinator(
      fileStore,
      loadCoordinator,
      new RuntimeWorldLoadLifecycleProjection(reprojectPreviousWorldState));
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog)
  {
    return CreateLoadCoordinator(
      new WorldFileStoreAdapter(),
      documentDecoder,
      documentValidator,
      apiCatalog);
  }

  public static WorldLoadCoordinator CreateLoadCoordinator(
    IWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog)
  {
    return new WorldLoadCoordinator(
      fileStore,
      documentDecoder,
      documentValidator,
      apiCatalog);
  }

  private sealed class RuntimeWorldLoadLifecycleProjection : IWorldLoadLifecycleProjection
  {
    private readonly Func<LoadedWorldSession, WorldStorageOperationResult>?
      _reprojectPreviousWorldState;

    public RuntimeWorldLoadLifecycleProjection(
      Func<LoadedWorldSession, WorldStorageOperationResult>? reprojectPreviousWorldState)
    {
      _reprojectPreviousWorldState = reprojectPreviousWorldState;
    }

    public WorldStorageOperationResult CommitPublishedSession(LoadedWorldSession session)
    {
      ArgumentNullException.ThrowIfNull(session);
      if (!session.IsPublished ||
          session.Lifecycle.LoadFailed ||
          session.Lifecycle.RequiresWorldReset ||
          session.Lifecycle.IsGeneratingOrLoadingWorld)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "Only a settled published session can replace the active world session."));
      }

      PendingWorldSessionRetirement? pending =
        Volatile.Read(ref _pendingWorldSessionRetirement);
      LoadedWorldSession? activeSession = Volatile.Read(ref _activeGeneratedWorldSession);
      if (pending is null && ReferenceEquals(activeSession, session))
      {
        return WorldStorageOperationResult.Success;
      }

      if (pending is null || !ReferenceEquals(pending.Candidate, session) ||
          !ReferenceEquals(activeSession, session))
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The candidate was not staged as the active world session."));
      }

      try
      {
        pending.Previous?.EntityRuntime.EnsureCanDispose();
      }
      catch (InvalidOperationException exception)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.Unknown,
            $"The previous world session cannot be retired: {exception.Message}"));
      }

      pending.Previous?.Dispose();
      Interlocked.CompareExchange(ref _pendingWorldSessionRetirement, null, pending);
      return WorldStorageOperationResult.Success;
    }

    public WorldStorageOperationResult Project(
      LoadedWorldSession session,
      bool isGeneratingOrLoadingWorld,
      bool loadFailed,
      bool worldBackup,
      bool worldCleared)
    {
      ArgumentNullException.ThrowIfNull(session);
      bool preservePreexistingGate = session.Lifecycle.PreservePreexistingLoadGate &&
        !session.IsPublished &&
        !session.Lifecycle.RequiresWorldReset &&
        !session.Lifecycle.WorldCleared &&
        !worldCleared;
      if (isGeneratingOrLoadingWorld || preservePreexistingGate)
      {
        // Raise the gate before fallible projection work so world updates cannot race recovery.
        NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld = true;
      }

      NSSLC.WorldGeneration.WorldGen.loadFailed = loadFailed;
      NSSLC.WorldGeneration.WorldGen.worldBackup = worldBackup;
      NSSLC.WorldGeneration.WorldGen.worldCleared = worldCleared;
      // A failed reset must not republish the candidate after the host has begun clearing it.
      bool resetIsPending = session.Lifecycle.RequiresWorldReset;
      if (session.IsPublished && !resetIsPending)
      {
        if (!ReferenceEquals(
              Volatile.Read(ref _activeGeneratedWorldSession),
              session))
        {
          WorldStorageOperationResult treeTopsResult =
            LegacyWorldTreeTopsProjection.Publish(session);
          if (!treeTopsResult.Succeeded ||
              treeTopsResult.Failure.Kind != WorldStorageFailureKind.None)
          {
            return treeTopsResult;
          }
        }

        if (!ReferenceEquals(
              NSSLC.WorldGeneration.Main.ActiveWorldSession,
              session.World))
        {
          NSSLC.WorldGeneration.Main.SetActiveWorldSession(session.World);
          NSSLC.WorldGeneration.WorldGen.ResetWorldTileMetrics();
        }

        WorldStorageOperationResult stagingResult = StagePublishedSession(session);
        if (!stagingResult.Succeeded ||
            stagingResult.Failure.Kind != WorldStorageFailureKind.None)
        {
          return stagingResult;
        }
      }
      else if (resetIsPending || worldCleared)
      {
        WorldStorageOperationResult restoreResult = RestorePreviousSession(session);
        if (!restoreResult.Succeeded ||
            restoreResult.Failure.Kind != WorldStorageFailureKind.None)
        {
          return restoreResult;
        }

        LoadedWorldSession? activeSession = Volatile.Read(
          ref _activeGeneratedWorldSession);
        if (activeSession is not null && !ReferenceEquals(activeSession, session))
        {
          if (!ReferenceEquals(
                NSSLC.WorldGeneration.Main.ActiveWorldSession,
                activeSession.World))
          {
            NSSLC.WorldGeneration.Main.SetActiveWorldSession(activeSession.World);
            NSSLC.WorldGeneration.WorldGen.ResetWorldTileMetrics();
          }

          if (worldCleared && _reprojectPreviousWorldState is not null)
          {
            WorldStorageOperationResult reprojectResult =
              _reprojectPreviousWorldState.Invoke(activeSession);
            if (!reprojectResult.Succeeded ||
                reprojectResult.Failure.Kind != WorldStorageFailureKind.None)
            {
              return reprojectResult;
            }
          }
        }
        else if (ReferenceEquals(
                   NSSLC.WorldGeneration.Main.ActiveWorldSession,
                   session.World))
        {
          NSSLC.WorldGeneration.Main.SetActiveWorldSession(
            new Terraria.WorldSession.Components.WorldSessionRestoreState());
          NSSLC.WorldGeneration.WorldGen.ResetWorldTileMetrics();
        }

        if (ReferenceEquals(activeSession, session))
        {
          ClearActiveGeneratedWorldSession();
        }
      }

      // The legacy volatile load gate is the publication boundary observed by the world loop.
      // A raised gate was published before fallible work above; release it only after the
      // associated flags and active session are visible.
      if (!isGeneratingOrLoadingWorld && !preservePreexistingGate)
      {
        NSSLC.WorldGeneration.WorldGen.isGeneratingOrLoadingWorld = false;
      }

      return WorldStorageOperationResult.Success;
    }

    private static WorldStorageOperationResult StagePublishedSession(
      LoadedWorldSession session)
    {
      LoadedWorldSession? activeSession = Volatile.Read(ref _activeGeneratedWorldSession);
      if (ReferenceEquals(activeSession, session))
      {
        return WorldStorageOperationResult.Success;
      }

      PendingWorldSessionRetirement? pending =
        Volatile.Read(ref _pendingWorldSessionRetirement);
      if (pending is not null)
      {
        return ReferenceEquals(pending.Candidate, session)
          ? WorldStorageOperationResult.Success
          : WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "Another candidate is already staged for active-session replacement."));
      }

      var replacement = new PendingWorldSessionRetirement(session, activeSession);
      PendingWorldSessionRetirement? pendingPrevious = Interlocked.CompareExchange(
        ref _pendingWorldSessionRetirement,
        replacement,
        null);
      if (pendingPrevious is not null)
      {
        return ReferenceEquals(pendingPrevious.Candidate, session)
          ? WorldStorageOperationResult.Success
          : WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "Another candidate is already staged for active-session replacement."));
      }

      LoadedWorldSession? exchangedSession = Interlocked.CompareExchange(
        ref _activeGeneratedWorldSession,
        session,
        activeSession);
      if (ReferenceEquals(exchangedSession, activeSession))
      {
        return WorldStorageOperationResult.Success;
      }

      Interlocked.CompareExchange(ref _pendingWorldSessionRetirement, null, replacement);
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The active world session changed while a candidate was staged."));
    }

    private static WorldStorageOperationResult RestorePreviousSession(
      LoadedWorldSession session)
    {
      PendingWorldSessionRetirement? pending =
        Volatile.Read(ref _pendingWorldSessionRetirement);
      if (pending is null || !ReferenceEquals(pending.Candidate, session))
      {
        return WorldStorageOperationResult.Success;
      }

      LoadedWorldSession? activeSession = Volatile.Read(ref _activeGeneratedWorldSession);
      if (ReferenceEquals(activeSession, session))
      {
        activeSession = Interlocked.CompareExchange(
          ref _activeGeneratedWorldSession,
          pending.Previous,
          session);
        if (!ReferenceEquals(activeSession, session))
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "The staged candidate changed before the previous session could be restored."));
        }

        activeSession = pending.Previous;
      }
      else if (activeSession is null && pending.Previous is not null)
      {
        activeSession = Interlocked.CompareExchange(
          ref _activeGeneratedWorldSession,
          pending.Previous,
          null);
        if (activeSession is not null)
        {
          return WorldStorageOperationResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.Unknown,
              "The active world session changed before the previous session could be restored."));
        }

        activeSession = pending.Previous;
      }
      else if (!ReferenceEquals(activeSession, pending.Previous))
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.Unknown,
            "The active world session does not match the pending replacement."));
      }

      PendingWorldSessionRetirement? clearedPending = Interlocked.CompareExchange(
        ref _pendingWorldSessionRetirement,
        null,
        pending);
      if (!ReferenceEquals(clearedPending, pending))
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.Unknown,
            "The pending world-session replacement changed during rollback."));
      }

      return WorldStorageOperationResult.Success;
    }
  }
}
