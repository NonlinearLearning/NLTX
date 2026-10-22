using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>Repairs a damaged primary from a validated backup without replacing the backup.</summary>
public sealed class WorldDocumentRecoveryCoordinator
{
  private readonly IMinimalWorldFileStore _fileStore;
  private readonly WorldDocumentRecoveryLoader _loader;

  public WorldDocumentRecoveryCoordinator(
    IMinimalWorldFileStore fileStore,
    WorldDocumentRecoveryLoader loader)
  {
    ArgumentNullException.ThrowIfNull(fileStore);
    ArgumentNullException.ThrowIfNull(loader);
    _fileStore = fileStore;
    _loader = loader;
  }

  public async Task<WorldDocumentRecoveryResult> LoadAndRepairAsync(
    string primaryPath,
    int maximumFileBytes,
    CancellationToken cancellationToken = default)
  {
    WorldDocumentRecoveryResult recovery = await _loader.ReadBestAvailableAsync(
      primaryPath,
      maximumFileBytes,
      cancellationToken).ConfigureAwait(false);
    if (!recovery.Succeeded || recovery.Source != WorldDocumentLoadSource.Backup)
    {
      return recovery;
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return recovery with
      {
        BackupFailure = WorldStorageFailure.Create(WorldStorageFailureKind.Canceled,
          "Primary repair was not started because recovery was canceled.")
      };
    }

    WorldFileCommitResult commit;
    try
    {
      commit = await _fileStore.CommitAtomicallyAsync(
        primaryPath,
        recovery.SourceBytes,
        maximumFileBytes,
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      commit = WorldFileCommitResult.Unknown(WorldStorageFailure.Create(
        WorldStorageFailureKind.Unknown,
        exception.Message));
    }

    if (commit.Status != WorldFileCommitStatus.Committed)
    {
      return recovery with
      {
        BackupFailure = commit.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
          : commit.Failure
      };
    }

    WorldStorageReadResult repaired = await _fileStore.ReadBoundedAsync(
      primaryPath,
      maximumFileBytes,
      CancellationToken.None).ConfigureAwait(false);
    if (!repaired.Succeeded || repaired.Data is null)
    {
      return recovery with
      {
        BackupFailure = repaired.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
          : repaired.Failure
      };
    }

    if (!CryptographicOperations.FixedTimeEquals(
          SHA256.HashData(recovery.SourceBytes.Span),
          SHA256.HashData(repaired.Data)))
    {
      return recovery with
      {
        BackupFailure = WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The repaired primary does not match the validated backup bytes.")
      };
    }

    return recovery;
  }

  private static bool IsStorageBoundaryFailure(Exception exception) => exception is
    OperationCanceledException or IOException or UnauthorizedAccessException or
    ArgumentException or NotSupportedException or System.Security.SecurityException;
}
