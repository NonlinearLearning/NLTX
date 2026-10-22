using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>Loads a valid primary document or falls back to its validated .bak copy.</summary>
/// <remarks>The loader never mutates either file; publishing or repairing a recovered world is a separate operation.</remarks>
public sealed class WorldDocumentRecoveryLoader
{
  private readonly IMinimalWorldFileStore _fileStore;
  private readonly IWorldPersistenceDocumentDecoder _decoder;
  private readonly IWorldPersistenceDocumentValidator _validator;

  public WorldDocumentRecoveryLoader(
    IMinimalWorldFileStore fileStore,
    IWorldPersistenceDocumentDecoder decoder,
    IWorldPersistenceDocumentValidator validator)
  {
    _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
    _validator = validator ?? throw new ArgumentNullException(nameof(validator));
  }

  public async Task<WorldDocumentRecoveryResult> ReadBestAvailableAsync(
    string primaryPath,
    int maximumFileBytes,
    CancellationToken cancellationToken = default)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(primaryPath);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFileBytes);

    string normalizedPrimaryPath;
    try
    {
      normalizedPrimaryPath = Path.GetFullPath(primaryPath);
    }
    catch (Exception exception) when (IsPathFailure(exception))
    {
      WorldStorageFailure failure = Classify(exception);
      return WorldDocumentRecoveryResult.Rejected(failure, WorldStorageFailure.None);
    }

    DocumentRead primary = await ReadAndValidateAsync(
      normalizedPrimaryPath,
      maximumFileBytes,
      cancellationToken).ConfigureAwait(false);
    if (primary.Document is not null)
    {
      return WorldDocumentRecoveryResult.FromPrimary(primary.Document, primary.Bytes);
    }

    if (primary.Failure.Kind == WorldStorageFailureKind.Canceled)
    {
      return WorldDocumentRecoveryResult.Rejected(
        primary.Failure,
        WorldStorageFailure.None);
    }

    if (cancellationToken.IsCancellationRequested)
    {
      return WorldDocumentRecoveryResult.Rejected(
        primary.Failure,
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled,
          "Backup recovery was not started because the load was canceled."),
        backupAttempted: false);
    }

    string backupPath = normalizedPrimaryPath + ".bak";
    DocumentRead backup = await ReadAndValidateAsync(
      backupPath,
      maximumFileBytes,
      cancellationToken).ConfigureAwait(false);
    if (backup.Document is not null)
    {
      return WorldDocumentRecoveryResult.FromBackup(
        backup.Document,
        primary.Failure,
        backup.Bytes);
    }

    return WorldDocumentRecoveryResult.Rejected(
      primary.Failure,
      backup.Failure,
      backupAttempted: true);
  }

  private async Task<DocumentRead> ReadAndValidateAsync(
    string path,
    int maximumFileBytes,
    CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested)
    {
      return DocumentRead.Failed(WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
    }

    WorldStorageReadResult read;
    try
    {
      read = await _fileStore.ReadBoundedAsync(
        path,
        maximumFileBytes,
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      return DocumentRead.Failed(Classify(exception));
    }

    if (!read.Succeeded || read.Data is null)
    {
      return DocumentRead.Failed(
        read.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.IoFailure)
          : read.Failure);
    }

    WorldPersistenceDecodeResult decoded;
    try
    {
      decoded = _decoder.Decode(read.Data);
    }
    catch (InvalidDataException exception)
    {
      return DocumentRead.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message));
    }

    if (!decoded.Succeeded || decoded.Document is null)
    {
      return DocumentRead.Failed(
        decoded.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
          : decoded.Failure);
    }

    WorldStorageFailure validationFailure;
    try
    {
      validationFailure = _validator.Validate(decoded.Document);
    }
    catch (InvalidDataException exception)
    {
      validationFailure = WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidData,
        exception.Message);
    }

    return validationFailure.Kind == WorldStorageFailureKind.None
      ? DocumentRead.Valid(decoded.Document, read.Data)
      : DocumentRead.Failed(validationFailure);
  }

  private static WorldStorageFailure Classify(Exception exception)
  {
    WorldStorageFailureKind kind = exception switch
    {
      OperationCanceledException => WorldStorageFailureKind.Canceled,
      UnauthorizedAccessException or System.Security.SecurityException =>
        WorldStorageFailureKind.PermissionDenied,
      FileNotFoundException or DirectoryNotFoundException => WorldStorageFailureKind.Missing,
      PathTooLongException or ArgumentException or NotSupportedException =>
        WorldStorageFailureKind.InvalidPath,
      InvalidDataException => WorldStorageFailureKind.InvalidData,
      IOException => WorldStorageFailureKind.IoFailure,
      _ => WorldStorageFailureKind.Unknown
    };
    return WorldStorageFailure.Create(kind, exception.Message);
  }

  private static bool IsPathFailure(Exception exception) => exception is
    ArgumentException or NotSupportedException or PathTooLongException or
    System.Security.SecurityException;

  private static bool IsStorageBoundaryFailure(Exception exception) => exception is
    IOException or UnauthorizedAccessException or ArgumentException or
    NotSupportedException or OperationCanceledException or System.Security.SecurityException;

  private readonly record struct DocumentRead(
    WorldPersistenceDocument? Document,
    ReadOnlyMemory<byte> Bytes,
    WorldStorageFailure Failure)
  {
    public static DocumentRead Valid(
      WorldPersistenceDocument document,
      ReadOnlyMemory<byte> bytes)
    {
      return new(document, bytes.ToArray(), WorldStorageFailure.None);
    }

    public static DocumentRead Failed(WorldStorageFailure failure)
    {
      return new(null, ReadOnlyMemory<byte>.Empty, failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
        : failure);
    }
  }
}
