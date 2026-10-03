using System;
using System.Collections.Generic;
using System.Threading;
using Terraria.WorldStorage;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldLoadCoordinator
{
  private readonly IWorldFileStore _fileStore;
  private readonly IWorldPersistenceDocumentDecoder _documentDecoder;
  private readonly IWorldPersistenceDocumentValidator _documentValidator;
  private readonly IWorldLoadApiCatalog _apiCatalog;
  private readonly object _loadLock = new();

  public WorldLoadCoordinator(
    IWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder documentDecoder,
    IWorldPersistenceDocumentValidator documentValidator,
    IWorldLoadApiCatalog apiCatalog)
  {
    _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    _documentDecoder = documentDecoder ??
      throw new ArgumentNullException(nameof(documentDecoder));
    _documentValidator = documentValidator ??
      throw new ArgumentNullException(nameof(documentValidator));
    _apiCatalog = apiCatalog ?? throw new ArgumentNullException(nameof(apiCatalog));
  }

  /// <summary>
  /// Loads the file at path once. Retry and backup selection belong to the lifecycle caller.
  /// </summary>
  public WorldRecoveryOutcome Load(
    string path,
    WorldLoadApiRuntimeBindings runtimeBindings)
  {
    return Load(
      path,
      runtimeBindings,
      WorldRecoveryStatus.Loaded,
      CancellationToken.None);
  }

  /// <summary>
  /// Loads the file at path once and labels a successful attempt with its lifecycle source.
  /// Retry and backup selection belong to the lifecycle caller.
  /// </summary>
  public WorldRecoveryOutcome Load(
    string path,
    WorldLoadApiRuntimeBindings runtimeBindings,
    WorldRecoveryStatus successStatus)
  {
    return Load(path, runtimeBindings, successStatus, CancellationToken.None);
  }

  /// <summary>
  /// Loads the file at path once, labels a successful attempt, and observes cancellation at each
  /// application boundary. File ports and owner APIs remain synchronous and may report their own
  /// cancellation exceptions.
  /// </summary>
  public WorldRecoveryOutcome Load(
    string path,
    WorldLoadApiRuntimeBindings runtimeBindings,
    WorldRecoveryStatus successStatus,
    CancellationToken cancellationToken)
  {
    return Load(path, runtimeBindings, successStatus, cancellationToken, null);
  }

  /// <summary>
  /// Loads the file at path once and reports its read and validation boundaries to the caller.
  /// Retry and backup selection belong to the lifecycle caller.
  /// </summary>
  public WorldRecoveryOutcome Load(
    string path,
    WorldLoadApiRuntimeBindings runtimeBindings,
    WorldRecoveryStatus successStatus,
    CancellationToken cancellationToken,
    IWorldLoadAttemptObserver? attemptObserver)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return Failed(0, WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    if (runtimeBindings is null)
    {
      return Failed(
        0,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "World load API runtime bindings were not supplied."));
    }

    if (successStatus is not (
        WorldRecoveryStatus.Loaded or
        WorldRecoveryStatus.RecoveredFromBackup))
    {
      return Failed(
        0,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "A load attempt must use Loaded or RecoveredFromBackup as its success status."));
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled(0, CancellationFailure());
    }

    lock (_loadLock)
    {
      return LoadUnderLock(
        path,
        runtimeBindings,
        successStatus,
        cancellationToken,
        attemptObserver);
    }
  }

  private WorldRecoveryOutcome LoadUnderLock(
    string path,
    WorldLoadApiRuntimeBindings runtimeBindings,
    WorldRecoveryStatus successStatus,
    CancellationToken cancellationToken,
    IWorldLoadAttemptObserver? attemptObserver)
  {
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled(0, CancellationFailure());
    }

    WorldStorageReadResult read = ReadAllBytes(path);

    if (!read.Succeeded || read.Data is null ||
        read.Failure.Kind != WorldStorageFailureKind.None)
    {
      WorldStorageFailure failure = ReadFailure(read);
      return failure.Kind == WorldStorageFailureKind.Missing
        ? new WorldRecoveryOutcome(WorldRecoveryStatus.Missing, 1, failure)
        : failure.Kind == WorldStorageFailureKind.Canceled
          ? Canceled(1, failure)
          : Failed(1, failure);
    }

    if (!TryNotifyFileOpened(attemptObserver, out WorldStorageFailure observerFailure))
    {
      return observerFailure.Kind == WorldStorageFailureKind.Canceled
        ? Canceled(1, observerFailure)
        : Failed(1, observerFailure);
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled(1, CancellationFailure());
    }

    if (!TryDecodeAndValidate(
          read.Data,
          out WorldPersistenceDocument document,
          out WorldStorageFailure validationFailure,
          cancellationToken))
    {
      return validationFailure.Kind == WorldStorageFailureKind.Canceled
        ? Canceled(1, validationFailure)
        : Failed(1, validationFailure);
    }

    if (!TryNotifyDocumentValidated(attemptObserver, out observerFailure))
    {
      return observerFailure.Kind == WorldStorageFailureKind.Canceled
        ? Canceled(1, observerFailure)
        : Failed(1, observerFailure);
    }

    return ExecuteLoad(document, runtimeBindings, successStatus, 1, cancellationToken);
  }

  private static bool TryNotifyFileOpened(
    IWorldLoadAttemptObserver? attemptObserver,
    out WorldStorageFailure failure)
  {
    return TryNotify(attemptObserver?.OnFileOpened, out failure);
  }

  private static bool TryNotifyDocumentValidated(
    IWorldLoadAttemptObserver? attemptObserver,
    out WorldStorageFailure failure)
  {
    return TryNotify(attemptObserver?.OnDocumentValidated, out failure);
  }

  private static bool TryNotify(Action? observerAction, out WorldStorageFailure failure)
  {
    if (observerAction is null)
    {
      failure = WorldStorageFailure.None;
      return true;
    }

    try
    {
      observerAction.Invoke();
      failure = WorldStorageFailure.None;
      return true;
    }
    catch (Exception exception)
    {
      failure = WorldStorageFailure.Create(
        WorldStorageExceptionClassifier.ContainsCancellation(exception)
          ? WorldStorageFailureKind.Canceled
          : WorldStorageFailureKind.Unknown,
        exception.Message);
      return false;
    }
  }

  private bool TryDecodeAndValidate(
    ReadOnlyMemory<byte> fileBytes,
    out WorldPersistenceDocument document,
    out WorldStorageFailure failure,
    CancellationToken cancellationToken)
  {
    document = null!;
    try
    {
      cancellationToken.ThrowIfCancellationRequested();
      WorldPersistenceDecodeResult? decodeResult = _documentDecoder.Decode(fileBytes);
      cancellationToken.ThrowIfCancellationRequested();
      if (decodeResult is null || !decodeResult.Succeeded || decodeResult.Document is null)
      {
        failure = decodeResult?.Failure.Kind == WorldStorageFailureKind.None ||
          decodeResult is null
          ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
          : decodeResult.Failure;
        return false;
      }

      WorldStorageFailure validationFailure = _documentValidator.Validate(decodeResult.Document);
      if (validationFailure.Kind != WorldStorageFailureKind.None)
      {
        failure = validationFailure;
        return false;
      }

      cancellationToken.ThrowIfCancellationRequested();
      document = decodeResult.Document;
      failure = WorldStorageFailure.None;
      return true;
    }
    catch (OperationCanceledException exception)
    {
      failure = WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message);
      return false;
    }
    catch (Exception exception)
    {
      failure = WorldStorageFailure.Create(
        WorldStorageExceptionClassifier.ContainsCancellation(exception)
          ? WorldStorageFailureKind.Canceled
          : WorldStorageFailureKind.InvalidData,
        exception.Message);
      return false;
    }
  }

  private WorldStorageReadResult ReadAllBytes(string path)
  {
    try
    {
      WorldStorageReadResult result = _fileStore.ReadAllBytes(path);
      if (result.Succeeded && result.Failure.Kind != WorldStorageFailureKind.None)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world file store returned success with a failure."));
      }

      if (result.Succeeded && result.Data is null)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world file store returned success without file data."));
      }

      if (!result.Succeeded && result.Data is not null)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world file store returned failure with file data."));
      }

      if (!result.Succeeded && result.Failure.Kind == WorldStorageFailureKind.None)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.IoFailure,
            "The world file store returned failure without an error."));
      }

      return result;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (Exception exception)
    {
      return WorldStorageReadResult.Failed(
        WorldStorageExceptionClassifier.ClassifyExternalFailure(
          exception,
          WorldStorageFailureKind.Unknown));
    }
  }

  private WorldRecoveryOutcome ExecuteLoad(
    WorldPersistenceDocument document,
    WorldLoadApiRuntimeBindings runtimeBindings,
    WorldRecoveryStatus successStatus,
    int attempts,
    CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
    {
      return Canceled(attempts, CancellationFailure());
    }

    WorldLoadApiExecutionResult execution;
    IReadOnlyList<WorldLoadApiDescriptor>? descriptors;
    int descriptorCount;
    try
    {
      IReadOnlyList<WorldLoadApiDescriptor>? catalogDescriptors = _apiCatalog.Descriptors;
      if (catalogDescriptors is null)
      {
        descriptors = null;
        descriptorCount = -1;
      }
      else
      {
        var descriptorSnapshot = new List<WorldLoadApiDescriptor>(catalogDescriptors.Count);
        for (int index = 0; index < catalogDescriptors.Count; index++)
        {
          // Preserve null entries for the validation boundary to diagnose explicitly.
          descriptorSnapshot.Add(catalogDescriptors[index]!);
        }

        descriptors = descriptorSnapshot;
        descriptorCount = descriptorSnapshot.Count;
      }

      if (cancellationToken.IsCancellationRequested)
      {
        return Canceled(attempts, CancellationFailure());
      }
    }
    catch (Exception exception)
    {
      execution = WorldLoadApiExecutionResult.Failed(
        WorldLoadApiStage.Binding,
        null,
        null,
        WorldLoadApiFailure.Create(
          "CatalogDescriptorException",
          "The world load API catalog failed while exposing its descriptors."),
        exception);
      return FailedExecution(attempts, execution);
    }

    if (descriptors is null)
    {
      execution = WorldLoadApiExecutionResult.Failed(
        WorldLoadApiStage.Binding,
        null,
        null,
        WorldLoadApiFailure.Create(
          "InvalidApiCatalog",
          "The world load API catalog returned a null descriptor list."));
    }
    else if (descriptorCount == 0)
    {
      execution = WorldLoadApiExecutionResult.Failed(
        WorldLoadApiStage.Binding,
        null,
        null,
        WorldLoadApiFailure.Create(
            "MissingApiDeclarations",
            "No world load API declarations were found in the generated catalog."));
    }
    else if (descriptorCount < 0)
    {
      execution = WorldLoadApiExecutionResult.Failed(
        WorldLoadApiStage.Binding,
        null,
        null,
        WorldLoadApiFailure.Create(
          "InvalidApiCatalog",
          "The world load API catalog returned a negative descriptor count."));
    }
    else
    {
      try
      {
        if (!WorldLoadApiCatalogValidation.TryValidate(
              descriptors,
              document.FormatVersion,
              document.SectionIds,
              out WorldLoadApiFailure catalogFailure,
              out string? catalogApiId,
              out string? catalogOwnerId))
        {
          execution = WorldLoadApiExecutionResult.Failed(
            WorldLoadApiStage.Binding,
            catalogApiId,
            catalogOwnerId,
            catalogFailure);
        }
        else
        {
          if (cancellationToken.IsCancellationRequested)
          {
            return Canceled(attempts, CancellationFailure());
          }

          var bindings = new WorldPersistenceApiBindings(
            document,
            runtimeBindings,
            cancellationToken);
          try
          {
            WorldLoadApiExecutionResult? catalogExecution = _apiCatalog.Execute(bindings);
            if (catalogExecution is null)
            {
              execution = WorldLoadApiExecutionResult.Failed(
                WorldLoadApiStage.Commit,
                null,
                null,
                WorldLoadApiFailure.Create(
                  "InvalidApiCatalogResult",
                  "The world load API catalog returned no execution result."));
            }
            else if (!catalogExecution.Succeeded &&
                     catalogExecution.Stage is not (
                       WorldLoadApiStage.Binding or
                       WorldLoadApiStage.Preparation or
                       WorldLoadApiStage.Commit))
            {
              execution = WorldLoadApiExecutionResult.Failed(
                WorldLoadApiStage.Commit,
                null,
                null,
                WorldLoadApiFailure.Create(
                  "InvalidApiCatalogResult",
                  "The world load API catalog returned a failure without a valid execution stage."),
                catalogExecution.Exception,
                catalogExecution.CleanupException);
            }
            else
            {
              execution = catalogExecution;
              if (execution.Succeeded &&
                  !TryConfirmDescriptorSnapshot(descriptors, out Exception? snapshotException))
              {
                execution = WorldLoadApiExecutionResult.Failed(
                  WorldLoadApiStage.Commit,
                  null,
                  null,
                  WorldLoadApiFailure.Create(
                    "CatalogChangedDuringLoad",
                    "The world load API catalog descriptors changed during execution."),
                  snapshotException);
              }
            }
          }
          catch (Exception exception)
          {
            // The catalog may have committed earlier APIs before an unexpected exception escaped.
            execution = WorldLoadApiExecutionResult.Failed(
              WorldLoadApiStage.Commit,
              null,
              null,
              WorldLoadApiFailure.Create(
                "CatalogExecutionException",
                "The world load API catalog failed during execution; owner state may be " +
                "partially written."),
              exception);
          }
        }
      }
      catch (Exception exception)
      {
        execution = WorldLoadApiExecutionResult.Failed(
          WorldLoadApiStage.Binding,
          null,
          null,
          WorldLoadApiFailure.Create(
            "CatalogValidationException",
            "The world load API catalog failed while validating its descriptors."),
          exception);
      }
    }

    if (!execution.Succeeded)
    {
      return FailedExecution(attempts, execution);
    }

    return new WorldRecoveryOutcome(
      successStatus,
      attempts,
      WorldStorageFailure.None,
      execution);
  }

  private static WorldRecoveryOutcome FailedExecution(
    int attempts,
    WorldLoadApiExecutionResult execution)
  {
    WorldStorageFailure failure = ToStorageFailure(execution);
    if (failure.Kind == WorldStorageFailureKind.Canceled)
    {
      return new WorldRecoveryOutcome(
        WorldRecoveryStatus.Canceled,
        attempts,
        failure,
        execution);
    }

    return new WorldRecoveryOutcome(
      WorldRecoveryStatus.Failed,
      attempts,
      failure,
      execution);
  }

  private static WorldStorageFailure ReadFailure(WorldStorageReadResult result)
  {
    return result.Failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : result.Failure;
  }

  private bool TryConfirmDescriptorSnapshot(
    IReadOnlyList<WorldLoadApiDescriptor> snapshot,
    out Exception? exception)
  {
    try
    {
      IReadOnlyList<WorldLoadApiDescriptor>? current = _apiCatalog.Descriptors;
      if (current is null || current.Count != snapshot.Count)
      {
        exception = null;
        return false;
      }

      for (int index = 0; index < snapshot.Count; index++)
      {
        if (!AreDescriptorsEquivalent(snapshot[index], current[index]))
        {
          exception = null;
          return false;
        }
      }

      exception = null;
      return true;
    }
    catch (Exception caughtException)
    {
      exception = caughtException;
      return false;
    }
  }

  private static bool AreDescriptorsEquivalent(
    WorldLoadApiDescriptor? left,
    WorldLoadApiDescriptor? right)
  {
    if (left is null || right is null)
    {
      return left is null && right is null;
    }

    if (!string.Equals(left.ApiId, right.ApiId, StringComparison.Ordinal) ||
        !string.Equals(left.OwnerId, right.OwnerId, StringComparison.Ordinal) ||
        !string.Equals(left.SectionId, right.SectionId, StringComparison.Ordinal) ||
        left.MinimumFormatVersion != right.MinimumFormatVersion ||
        left.MaximumFormatVersion != right.MaximumFormatVersion ||
        left.Requirement != right.Requirement ||
        left.CommitAfter.Count != right.CommitAfter.Count)
    {
      return false;
    }

    for (int index = 0; index < left.CommitAfter.Count; index++)
    {
      if (!string.Equals(
            left.CommitAfter[index],
            right.CommitAfter[index],
            StringComparison.Ordinal))
      {
        return false;
      }
    }

    return true;
  }

  private static WorldStorageFailure CancellationFailure()
  {
    return WorldStorageFailure.Create(
      WorldStorageFailureKind.Canceled,
      "The world load operation was canceled.");
  }

  private static WorldStorageFailure ToStorageFailure(WorldLoadApiExecutionResult execution)
  {
    WorldLoadApiFailure failure = execution.Failure ??
      WorldLoadApiFailure.Create("ApiExecutionFailed", "World load API execution failed.");
    WorldStorageFailureKind kind = WorldStorageExceptionClassifier.ContainsCancellation(
        execution.Exception) ||
      WorldStorageExceptionClassifier.ContainsCancellation(execution.CleanupException) ||
      string.Equals(failure.Code, "Canceled", StringComparison.Ordinal)
      ? WorldStorageFailureKind.Canceled
      : execution.Stage == WorldLoadApiStage.Commit
        ? WorldStorageFailureKind.Unknown
        : WorldStorageFailureKind.InvalidData;
    string detail = $"{execution.Stage} failed for API '{execution.ApiId}' " +
      $"and owner '{execution.OwnerId}': {failure.Code}: {failure.Message}";
    if (execution.CleanupException is not null)
    {
      detail += $" Cleanup also failed: {execution.CleanupException.Message}";
    }

    return WorldStorageFailure.Create(kind, detail);
  }

  private static WorldRecoveryOutcome Failed(int attempts, WorldStorageFailure failure)
  {
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
      : failure;
    WorldRecoveryStatus status = actualFailure.Kind == WorldStorageFailureKind.Canceled
      ? WorldRecoveryStatus.Canceled
      : WorldRecoveryStatus.Failed;
    return new WorldRecoveryOutcome(status, attempts, actualFailure);
  }

  private static WorldRecoveryOutcome Canceled(
    int attempts,
    WorldStorageFailure failure)
  {
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.Canceled
      ? failure
      : WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, failure.Detail);
    return new WorldRecoveryOutcome(
      WorldRecoveryStatus.Canceled,
      attempts,
      actualFailure);
  }
}
