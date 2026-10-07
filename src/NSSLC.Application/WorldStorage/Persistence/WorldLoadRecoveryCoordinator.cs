using System;
using System.IO;
using System.Threading;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

/// <summary>
/// Runs the world-scoped retry, backup, load-gate, and cleanup lifecycle around single-file loads.
/// </summary>
/// <remarks>
/// The supplied file store must be the same instance used by the load coordinator. The host must
/// provide owner bindings and world finalization/reset effects for the world being loaded, and
/// serialize recovery calls for each world session. Failed commits in an unpublished candidate are
/// discarded; host reset runs only after publication may have changed the active world.
/// </remarks>
public sealed class WorldLoadRecoveryCoordinator
{
  private readonly IWorldFileStore _fileStore;
  private readonly WorldLoadCoordinator _loadCoordinator;
  private readonly IWorldLoadLifecycleProjection _lifecycleProjection;

  public WorldLoadRecoveryCoordinator(
    IWorldFileStore fileStore,
    WorldLoadCoordinator loadCoordinator,
    IWorldLoadLifecycleProjection lifecycleProjection)
  {
    _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    _loadCoordinator = loadCoordinator ??
      throw new ArgumentNullException(nameof(loadCoordinator));
    _lifecycleProjection = lifecycleProjection ??
      throw new ArgumentNullException(nameof(lifecycleProjection));
    if (!_loadCoordinator.UsesFileStore(_fileStore))
    {
      throw new ArgumentException(
        "World recovery and single-load coordinators must share the same file store instance.",
        nameof(loadCoordinator));
    }
  }

  public WorldLoadRecoveryResult Recover(
    LoadedWorldSession loadedSession,
    string worldPath,
    WorldLoadApiRuntimeBindings runtimeBindings,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken = default)
  {
    return Recover(
      loadedSession,
      worldPath,
      runtimeBindings,
      recoveryEffects,
      cancellationToken,
      preservePreexistingLoadGate: false);
  }

  public WorldLoadRecoveryResult Recover(
    LoadedWorldSession loadedSession,
    string worldPath,
    WorldLoadApiRuntimeBindings runtimeBindings,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken,
    bool preservePreexistingLoadGate)
  {
    ArgumentNullException.ThrowIfNull(loadedSession);
    ArgumentNullException.ThrowIfNull(runtimeBindings);
    ArgumentNullException.ThrowIfNull(recoveryEffects);
    WorldLoadLifecycleComponent lifecycle = loadedSession.Lifecycle;
    if (!lifecycle.IsFresh)
    {
      throw new ArgumentException(
        "World-load recovery requires a session whose lifecycle has not started.",
        nameof(loadedSession));
    }

    if (!loadedSession.IsFresh)
    {
      throw new ArgumentException(
        "World-load recovery requires a fresh unpublished session.",
        nameof(loadedSession));
    }

    if (!runtimeBindings.TryGetOwnerContext(
          LoadedWorldSession.OwnerContextId,
          out LoadedWorldSession boundSession) ||
        !ReferenceEquals(loadedSession, boundSession))
    {
      throw new ArgumentException(
        "The runtime bindings must target the session supplied to recovery.",
        nameof(runtimeBindings));
    }

    WorldLoadRecoveryAction action = WorldLoadLifecycleSystem.StartRecovery(
      lifecycle,
      preservePreexistingLoadGate);
    WorldRecoveryOutcome? lastLoadOutcome = null;
    WorldStorageFailure failure = WorldStorageFailure.None;
    WorldStorageFailure cleanupFailure = WorldStorageFailure.None;
    var attemptObserver = new LifecycleAttemptObserver(
      loadedSession,
      lifecycle,
      _lifecycleProjection);

    WorldLoadRecoveryResult FinishTerminalFailure(
      WorldLoadLifecycleComponent recoveryLifecycle,
      WorldLoadRecoveryAction terminalAction,
      WorldRecoveryOutcome? loadOutcome,
      WorldStorageFailure terminalFailure,
      WorldStorageFailure terminalCleanupFailure,
      IWorldLoadRecoveryEffects effects,
      CancellationToken token)
    {
      return this.FinishTerminalFailure(
        loadedSession,
        recoveryLifecycle,
        terminalAction,
        loadOutcome,
        terminalFailure,
        terminalCleanupFailure,
        effects,
        token);
    }

    while (true)
    {
      if (action == WorldLoadRecoveryAction.LoadWorld)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle);
          failure = CanceledFailure();
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        WorldRecoveryStatus successStatus = lifecycle.WorldBackup
          ? WorldRecoveryStatus.RecoveredFromBackup
          : WorldRecoveryStatus.Loaded;
        lastLoadOutcome = _loadCoordinator.Load(
          worldPath,
          runtimeBindings,
          successStatus,
          cancellationToken,
          attemptObserver);

        if (lastLoadOutcome.CanPublishWorldLoaded)
        {
          if (!loadedSession.IsComplete)
          {
            failure = WorldStorageFailure.Create(
              WorldStorageFailureKind.InvalidData,
              "The world-load APIs completed without committing a complete session.");
            action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: true,
              requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          WorldStorageOperationResult loadGateProjection = ProjectLifecycle(
            loadedSession,
            lifecycle,
            _lifecycleProjection);
          if (!loadGateProjection.Succeeded)
          {
            failure = NormalizeOperationResult(
              loadGateProjection,
              "Load-gate activation projection").Failure;
            action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: true,
              requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          WorldStorageOperationResult publishResult = PublishLoadedWorld(
            lifecycle,
            loadedSession,
            recoveryEffects,
            cancellationToken);
          if (!publishResult.Succeeded)
          {
            failure = NormalizeOperationResult(publishResult, "World publication").Failure;
            action = failure.Kind == WorldStorageFailureKind.Canceled ||
                cancellationToken.IsCancellationRequested
              ? WorldLoadLifecycleSystem.CancelRecovery(lifecycle, requiresWorldReset: true)
              : WorldLoadLifecycleSystem.CompleteLoadAttempt(
                lifecycle,
                loadFailed: true,
                requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          loadedSession.MarkPublished();
          WorldStorageOperationResult publicationProjection = ProjectLifecycle(
            loadedSession,
            lifecycle,
            _lifecycleProjection);
          if (!publicationProjection.Succeeded)
          {
            failure = NormalizeOperationResult(
              publicationProjection,
              "Published-session projection").Failure;
            action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: true,
              requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          if (cancellationToken.IsCancellationRequested)
          {
            action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle, requiresWorldReset: true);
            failure = CanceledFailure();
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          WorldStorageOperationResult settleResult = SettleLoadedWorld(
            lifecycle,
            loadedSession,
            recoveryEffects,
            cancellationToken);
          if (!settleResult.Succeeded)
          {
            failure = NormalizeOperationResult(settleResult, "Liquid settle").Failure;
            action = failure.Kind == WorldStorageFailureKind.Canceled ||
                cancellationToken.IsCancellationRequested
              ? WorldLoadLifecycleSystem.CancelRecovery(lifecycle, requiresWorldReset: true)
              : WorldLoadLifecycleSystem.CompleteLoadAttempt(
                lifecycle,
                loadFailed: true,
                requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          if (cancellationToken.IsCancellationRequested)
          {
            action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle, requiresWorldReset: true);
            failure = CanceledFailure();
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          WorldLoadLifecycleSystem.CompleteLiquidSettle(lifecycle);
          WorldStorageOperationResult gateReleaseResult = ProjectLifecycle(
            loadedSession,
            lifecycle,
            _lifecycleProjection);
          if (!gateReleaseResult.Succeeded)
          {
            failure = NormalizeOperationResult(
              gateReleaseResult,
              "Load-gate release projection").Failure;
            action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: true,
              requiresWorldReset: true);
            return FinishTerminalFailure(
              lifecycle,
              action,
              lastLoadOutcome,
              failure,
              cleanupFailure,
              recoveryEffects,
              cancellationToken);
          }

          WorldStorageOperationResult finalizeResult = FinalizeLoadedWorld(
            lifecycle,
            loadedSession,
            recoveryEffects,
            cancellationToken);
          if (finalizeResult.Succeeded)
          {
            WorldStorageOperationResult publicationCommit = CommitPublishedSession(
              loadedSession,
              _lifecycleProjection);
            if (!publicationCommit.Succeeded)
            {
              failure = NormalizeOperationResult(
                publicationCommit,
                "Published-session exchange").Failure;
              action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
                lifecycle,
                loadFailed: true,
                requiresWorldReset: true);
              return FinishTerminalFailure(
                lifecycle,
                action,
                lastLoadOutcome,
                failure,
                cleanupFailure,
                recoveryEffects,
                cancellationToken);
            }

            action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: false);
            return new WorldLoadRecoveryResult(
              action,
              lifecycle.LoadAttemptCount,
              lastLoadOutcome,
              WorldStorageFailure.None,
              WorldStorageFailure.None,
              lifecycle.RequiresWorldReset,
              lifecycle.WorldCleared);
          }

          failure = NormalizeOperationResult(finalizeResult, "World finalization").Failure;
          action = failure.Kind == WorldStorageFailureKind.Canceled ||
              cancellationToken.IsCancellationRequested
            ? WorldLoadLifecycleSystem.CancelRecovery(lifecycle, requiresWorldReset: true)
            : WorldLoadLifecycleSystem.CompleteLoadAttempt(
              lifecycle,
              loadFailed: true,
              requiresWorldReset: true);
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        if (lastLoadOutcome.Status == WorldRecoveryStatus.Canceled ||
            cancellationToken.IsCancellationRequested)
        {
          bool requiresCandidateDiscard = lastLoadOutcome.RequiresCandidateDiscard;
          action = WorldLoadLifecycleSystem.CancelRecovery(
            lifecycle,
            requiresCandidateDiscard);
          failure = lastLoadOutcome.Failure.Kind == WorldStorageFailureKind.None
            ? CanceledFailure()
            : lastLoadOutcome.Failure;
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        failure = lastLoadOutcome.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
          : lastLoadOutcome.Failure;
        action = WorldLoadLifecycleSystem.CompleteLoadAttempt(
          lifecycle,
          loadFailed: true,
          lastLoadOutcome.RequiresCandidateDiscard);
        if (lifecycle.RequiresWorldReset || IsTerminal(action))
        {
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        continue;
      }

      if (action == WorldLoadRecoveryAction.CheckBackup)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle);
          failure = CanceledFailure();
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        WorldStorageOperationResult backupCheck = CheckBackupExists(
          worldPath,
          out bool backupExists);
        if (!backupCheck.Succeeded)
        {
          failure = backupCheck.Failure;
          if (failure.Kind == WorldStorageFailureKind.Canceled)
          {
            action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle);
          }
          else
          {
            action = WorldLoadLifecycleSystem.FailBackupCheck(lifecycle);
          }

          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        action = WorldLoadLifecycleSystem.CompleteBackupCheck(lifecycle, backupExists);
        if (action == WorldLoadRecoveryAction.ReportNoBackupFailure)
        {
          failure = WorldStorageFailure.Create(
            WorldStorageFailureKind.Missing,
            "No world backup file was found.");
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        continue;
      }

      if (action == WorldLoadRecoveryAction.RestoreBackupAndDelete)
      {
        if (cancellationToken.IsCancellationRequested)
        {
          action = WorldLoadLifecycleSystem.CancelRecovery(lifecycle);
          failure = CanceledFailure();
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        WorldStorageOperationResult restoreResult = RestoreBackupAndDelete(worldPath);
        if (!restoreResult.Succeeded)
        {
          failure = restoreResult.Failure;
          action = failure.Kind == WorldStorageFailureKind.Canceled
            ? WorldLoadLifecycleSystem.CancelRecovery(lifecycle)
            : WorldLoadLifecycleSystem.FailBackupRestore(lifecycle);
          return FinishTerminalFailure(
            lifecycle,
            action,
            lastLoadOutcome,
            failure,
            cleanupFailure,
            recoveryEffects,
            cancellationToken);
        }

        action = WorldLoadLifecycleSystem.CompleteBackupRestore(lifecycle);
        continue;
      }

      if (IsTerminal(action))
      {
        if (failure.Kind == WorldStorageFailureKind.None)
        {
          failure = WorldStorageFailure.Create(WorldStorageFailureKind.Unknown);
        }

        return FinishTerminalFailure(
          lifecycle,
          action,
          lastLoadOutcome,
          failure,
          cleanupFailure,
          recoveryEffects,
          cancellationToken);
      }

      throw new InvalidOperationException(
        $"World-load recovery produced unsupported action '{action}'.");
    }
  }

  /// <summary>
  /// Closes the lifecycle after an exception escaped recovery outside its expected error handling.
  /// A candidate is discarded without resetting the active world unless publication had started.
  /// </summary>
  public Exception RecoverUnexpectedFailure(
    LoadedWorldSession loadedSession,
    IWorldLoadRecoveryEffects recoveryEffects,
    Exception operationException)
  {
    ArgumentNullException.ThrowIfNull(loadedSession);
    ArgumentNullException.ThrowIfNull(recoveryEffects);
    ArgumentNullException.ThrowIfNull(operationException);

    WorldLoadLifecycleComponent lifecycle = loadedSession.Lifecycle;
    if (lifecycle.RecoveryPhase == WorldLoadRecoveryPhase.NotStarted)
    {
      return operationException;
    }

    bool requiresWorldReset =
      loadedSession.IsPublished || loadedSession.IsPublicationUncertain;
    bool worldResetCompleted = false;
    WorldStorageFailure cleanupFailure = WorldStorageFailure.None;
    if (requiresWorldReset)
    {
      WorldLoadLifecycleSystem.BeginWorldReset(lifecycle);
      WorldStorageOperationResult gateProjection = ProjectLifecycle(
        loadedSession,
        lifecycle,
        _lifecycleProjection);
      if (!gateProjection.Succeeded)
      {
        cleanupFailure = NormalizeOperationResult(
          gateProjection,
          "Unexpected-failure load-gate projection").Failure;
      }
      else
      {
        WorldStorageOperationResult resetResult = ResetWorld(recoveryEffects, loadedSession);
        if (resetResult.Succeeded)
        {
          worldResetCompleted = true;
          loadedSession.MarkUnpublished();
        }
        else
        {
          cleanupFailure = MergeCleanupFailures(
            cleanupFailure,
            resetResult.Failure,
            "World reset");
        }
      }
    }

    WorldLoadLifecycleSystem.FailRecoveryAfterUnexpectedException(
      lifecycle,
      requiresWorldReset,
      worldResetCompleted);
    WorldStorageOperationResult terminalProjection = ProjectLifecycle(
      loadedSession,
      lifecycle,
      _lifecycleProjection);
    if (!terminalProjection.Succeeded)
    {
      cleanupFailure = MergeCleanupFailures(
        cleanupFailure,
        NormalizeOperationResult(terminalProjection, "Terminal lifecycle projection").Failure,
        "Terminal lifecycle projection");
    }

    return cleanupFailure.Kind == WorldStorageFailureKind.None
      ? operationException
      : new AggregateException(
        "World-load recovery failed and its terminal cleanup was incomplete.",
        operationException,
        new InvalidOperationException(cleanupFailure.Detail));
  }

  private WorldStorageOperationResult CheckBackupExists(
    string worldPath,
    out bool backupExists)
  {
    backupExists = false;
    try
    {
      WorldStorageOperationResult result = _fileStore.CheckBackupExists(
        worldPath,
        out backupExists);
      return NormalizeOperationResult(result, "Backup check");
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private WorldStorageOperationResult RestoreBackupAndDelete(string worldPath)
  {
    try
    {
      WorldStorageOperationResult result = _fileStore.RestoreBackupAndDelete(worldPath);
      return NormalizeOperationResult(result, "Backup restore");
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult PublishLoadedWorld(
    WorldLoadLifecycleComponent lifecycle,
    LoadedWorldSession loadedSession,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken)
  {
    if (!lifecycle.IsGeneratingOrLoadingWorld || !loadedSession.IsComplete ||
        loadedSession.IsPublished || loadedSession.IsPublicationUncertain)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "Only a complete unpublished session can be published while the load gate is raised."));
    }

    try
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return WorldStorageOperationResult.Failed(CanceledFailure());
      }

      loadedSession.MarkPublicationAttempted();
      WorldStorageOperationResult result = recoveryEffects.PublishLoadedWorld(
        loadedSession,
        cancellationToken);
      result = NormalizeOperationResult(result, "World publication");
      if (!result.Succeeded)
      {
        return result;
      }

      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult SettleLoadedWorld(
    WorldLoadLifecycleComponent lifecycle,
    LoadedWorldSession loadedSession,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken)
  {
    if (!lifecycle.IsGeneratingOrLoadingWorld)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The load gate was not raised before owner data was committed."));
    }

    try
    {
      WorldStorageOperationResult result = recoveryEffects.SettleLoadedWorld(
        loadedSession,
        cancellationToken);
      result = NormalizeOperationResult(result, "Liquid settle");
      if (!result.Succeeded)
      {
        return result;
      }

      if (cancellationToken.IsCancellationRequested)
      {
        return WorldStorageOperationResult.Failed(CanceledFailure());
      }

      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult FinalizeLoadedWorld(
    WorldLoadLifecycleComponent lifecycle,
    LoadedWorldSession loadedSession,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken)
  {
    if (lifecycle.IsGeneratingOrLoadingWorld)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The load gate must be released before world-specific finalization."));
    }

    try
    {
      WorldStorageOperationResult result = recoveryEffects.FinalizeLoadedWorld(
        loadedSession,
        cancellationToken);
      result = NormalizeOperationResult(result, "World finalization");
      if (!result.Succeeded)
      {
        return result;
      }

      return cancellationToken.IsCancellationRequested
        ? WorldStorageOperationResult.Failed(CanceledFailure())
        : WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private WorldLoadRecoveryResult FinishTerminalFailure(
    LoadedWorldSession loadedSession,
    WorldLoadLifecycleComponent lifecycle,
    WorldLoadRecoveryAction action,
    WorldRecoveryOutcome? lastLoadOutcome,
    WorldStorageFailure failure,
    WorldStorageFailure cleanupFailure,
    IWorldLoadRecoveryEffects recoveryEffects,
    CancellationToken cancellationToken)
  {
    if (lifecycle.RequiresWorldReset)
    {
      if (loadedSession.IsPublished || loadedSession.IsPublicationUncertain)
      {
        WorldLoadLifecycleSystem.BeginWorldReset(lifecycle);
        WorldStorageOperationResult gateResult = ProjectLifecycle(
          loadedSession,
          lifecycle,
          _lifecycleProjection);
        if (!gateResult.Succeeded)
        {
          cleanupFailure = MergeCleanupFailures(
            cleanupFailure,
            NormalizeOperationResult(
              gateResult,
            "Load-gate activation projection").Failure,
            "Load-gate activation projection");
        }
        else
        {
          WorldStorageOperationResult resetResult = ResetWorld(recoveryEffects, loadedSession);
          if (resetResult.Succeeded)
          {
            WorldLoadLifecycleSystem.CompleteWorldReset(lifecycle);
            loadedSession.MarkUnpublished();
          }
          else
          {
            cleanupFailure = MergeCleanupFailures(
              cleanupFailure,
              resetResult.Failure,
              "World reset");
          }
        }
      }
      else
      {
        WorldLoadLifecycleSystem.CompleteUnpublishedSessionDiscard(lifecycle);
        loadedSession.MarkUnpublished();
      }
    }
    else if (lifecycle.IsGeneratingOrLoadingWorld)
    {
      WorldLoadLifecycleSystem.ReleaseLoadGateAfterFailure(lifecycle);
    }

    WorldStorageOperationResult projectionResult = ProjectLifecycle(
      loadedSession,
      lifecycle,
      _lifecycleProjection);
    if (!projectionResult.Succeeded)
    {
      WorldStorageFailure projectedFailure = NormalizeOperationResult(
        projectionResult,
        "Terminal lifecycle projection").Failure;
      cleanupFailure = MergeCleanupFailures(
        cleanupFailure,
        projectedFailure,
        "Terminal lifecycle projection");
    }

    return new WorldLoadRecoveryResult(
      action,
      lifecycle.LoadAttemptCount,
      lastLoadOutcome,
      failure,
      cleanupFailure,
      lifecycle.RequiresWorldReset,
      lifecycle.WorldCleared);
  }

  private static WorldStorageOperationResult ResetWorld(
    IWorldLoadRecoveryEffects recoveryEffects,
    LoadedWorldSession loadedSession)
  {
    try
    {
      return NormalizeOperationResult(
        recoveryEffects.ResetWorld(loadedSession),
        "World reset");
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult ProjectLifecycle(
    LoadedWorldSession loadedSession,
    WorldLoadLifecycleComponent lifecycle,
    IWorldLoadLifecycleProjection lifecycleProjection)
  {
    try
    {
      return NormalizeOperationResult(
        lifecycleProjection.Project(
          loadedSession,
          lifecycle.IsGeneratingOrLoadingWorld,
          lifecycle.LoadFailed,
          lifecycle.WorldBackup,
          lifecycle.WorldCleared),
        "World lifecycle projection");
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult CommitPublishedSession(
    LoadedWorldSession loadedSession,
    IWorldLoadLifecycleProjection lifecycleProjection)
  {
    try
    {
      return NormalizeOperationResult(
        lifecycleProjection.CommitPublishedSession(loadedSession),
        "Published-session exchange");
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(ClassifyException(exception));
    }
  }

  private static WorldStorageOperationResult NormalizeOperationResult(
    WorldStorageOperationResult result,
    string operation)
  {
    if (result.Succeeded && result.Failure.Kind == WorldStorageFailureKind.None)
    {
      return result;
    }

    if (!result.Succeeded && result.Failure.Kind != WorldStorageFailureKind.None)
    {
      return result;
    }

    WorldStorageFailure failure = WorldStorageFailure.Create(
      result.Succeeded
        ? WorldStorageFailureKind.InvalidData
        : WorldStorageFailureKind.IoFailure,
      $"The {operation} effect returned a contradictory result.");
    return WorldStorageOperationResult.Failed(failure);
  }

  private static WorldStorageFailure MergeCleanupFailures(
    WorldStorageFailure current,
    WorldStorageFailure next,
    string operation)
  {
    if (current.Kind == WorldStorageFailureKind.None)
    {
      return next;
    }

    if (next.Kind == WorldStorageFailureKind.None)
    {
      return current;
    }

    return WorldStorageFailure.Create(
      WorldStorageFailureKind.Unknown,
      $"{current.Detail} {operation} also failed: {next.Detail}");
  }

  private static WorldStorageFailure ClassifyException(Exception exception)
  {
    bool canceled = ContainsCancellation(exception);
    WorldStorageFailureKind kind = canceled
      ? WorldStorageFailureKind.Canceled
      : exception switch
      {
        UnauthorizedAccessException => WorldStorageFailureKind.PermissionDenied,
        FileNotFoundException or DirectoryNotFoundException => WorldStorageFailureKind.Missing,
        PathTooLongException or
          ArgumentException or
          NotSupportedException => WorldStorageFailureKind.InvalidPath,
        IOException => WorldStorageFailureKind.IoFailure,
        _ => WorldStorageFailureKind.Unknown
      };
    return WorldStorageFailure.Create(kind, exception.Message);
  }

  private static bool ContainsCancellation(Exception exception)
  {
    if (exception is OperationCanceledException)
    {
      return true;
    }

    if (exception is AggregateException aggregate)
    {
      foreach (Exception innerException in aggregate.InnerExceptions)
      {
        if (ContainsCancellation(innerException))
        {
          return true;
        }
      }
    }

    return exception.InnerException is not null &&
      ContainsCancellation(exception.InnerException);
  }

  private static bool IsTerminal(WorldLoadRecoveryAction action)
  {
    return action is WorldLoadRecoveryAction.ReportLoadFailure or
      WorldLoadRecoveryAction.ReportNoBackupFailure;
  }

  private static WorldStorageFailure CanceledFailure(string? detail = null)
  {
    return WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, detail);
  }

  private sealed class LifecycleAttemptObserver(
    LoadedWorldSession loadedSession,
    WorldLoadLifecycleComponent lifecycle,
    IWorldLoadLifecycleProjection lifecycleProjection) : IWorldLoadAttemptObserver
  {
    public void OnFileRead()
    {
      WorldLoadLifecycleSystem.MarkWorldFileOpened(lifecycle);
      EnsureProjected();
    }

    public void OnDocumentValidated()
    {
      // Keep candidate application behind a session-local gate. The host gate is raised only
      // after all APIs complete and immediately before the candidate is published.
      WorldLoadLifecycleSystem.BeginDecodedWorldRepair(lifecycle);
    }

    private void EnsureProjected()
    {
      WorldStorageOperationResult result = ProjectLifecycle(
        loadedSession,
        lifecycle,
        lifecycleProjection);
      if (!result.Succeeded)
      {
        throw new InvalidOperationException(
          $"World lifecycle projection failed: {result.Failure.Detail}");
      }
    }
  }
}
