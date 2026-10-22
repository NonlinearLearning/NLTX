using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;
using ContractWorldFileCommitResult = Terraria.NonAuthoritative.Persistence.WorldFileCommitResult;

namespace MinimalImplementation.Persistence;

/// <summary>Validates, encodes and persists one already-composed world document.</summary>
/// <remarks>
/// This use case does not capture mutable owner state or assemble section DTOs. The caller must
/// provide one immutable document and its commit version, captured behind the server's save
/// barrier. All file access is delegated to the Infrastructure-owned bounded store. One saver
/// instance serializes its save operations so a backup always corresponds to the primary it read.
/// </remarks>
public sealed class WorldDocumentSaver
{
  private static readonly object SaveGatesSync = new();
  private static readonly Dictionary<string, SaveGateEntry> SaveGates = new(
    OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

  private readonly IMinimalWorldFileStore _fileStore;
  private readonly IWorldSaveEncoder _encoder;
  private readonly IWorldPersistenceDocumentDecoder _decoder;
  private readonly IWorldPersistenceDocumentValidator _validator;

  public WorldDocumentSaver(
    IMinimalWorldFileStore fileStore,
    IWorldSaveEncoder encoder,
    IWorldPersistenceDocumentDecoder decoder,
    IWorldPersistenceDocumentValidator validator)
  {
    _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
    _decoder = decoder ?? throw new ArgumentNullException(nameof(decoder));
    _validator = validator ?? throw new ArgumentNullException(nameof(validator));
  }

  public async Task<WorldDocumentSaveResult> SaveAsync(
    WorldDocumentSaveRequest request,
    CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(request);
    WorldDocumentBackupStatus initialBackupStatus =
      request.BackupPolicy == WorldBackupPolicy.Disabled
        ? WorldDocumentBackupStatus.Disabled
        : WorldDocumentBackupStatus.NotAttempted;
    if (cancellationToken.IsCancellationRequested)
    {
      return Rejected(
        request,
        null,
        initialBackupStatus,
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled));
    }

    WorldStorageFailure documentFailure = ValidateDocument(request.Document);
    if (documentFailure.Kind != WorldStorageFailureKind.None)
    {
      return Rejected(request, null, initialBackupStatus, documentFailure);
    }

    WorldSaveEncodeResult encoded = EncodeDocument(request.Document);
    if (!encoded.Succeeded)
    {
      return Rejected(request, null, initialBackupStatus, encoded.Failure);
    }

    byte[] payload = encoded.Bytes.ToArray();
    if (payload.Length == 0)
    {
      return Rejected(
        request,
        null,
        initialBackupStatus,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The WorldFile encoder returned an empty payload."));
    }

    if (payload.Length > request.MaximumFileBytes)
    {
      return Rejected(
        request,
        null,
        initialBackupStatus,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          $"The encoded WorldFile exceeds the {request.MaximumFileBytes}-byte limit."),
        payload.Length);
    }

    WorldStorageFailure encodedValidationFailure = ValidateEncodedPayload(payload);
    if (encodedValidationFailure.Kind != WorldStorageFailureKind.None)
    {
      return Rejected(
        request,
        null,
        initialBackupStatus,
        encodedValidationFailure,
        payload.Length);
    }

    string payloadSha256 = Convert.ToHexString(SHA256.HashData(payload));
    string primaryPath;
    try
    {
      primaryPath = Path.GetFullPath(request.PrimaryPath);
    }
    catch (Exception exception) when (IsPathResolutionFailure(exception))
    {
      return Rejected(
        request,
        null,
        initialBackupStatus,
        CreateFailure(exception),
        payload.Length,
        payloadSha256);
    }

    string? backupPath = request.BackupPolicy == WorldBackupPolicy.Disabled
      ? null
      : primaryPath + ".bak";

    GateLease gate;
    try
    {
      gate = await AcquireSaveGateAsync(primaryPath, cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException exception)
    {
      return Rejected(
        request,
        backupPath,
        initialBackupStatus,
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message),
        payload.Length,
        payloadSha256);
    }

    using (gate)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return Rejected(
          request,
          backupPath,
          initialBackupStatus,
          WorldStorageFailure.Create(WorldStorageFailureKind.Canceled),
          payload.Length,
          payloadSha256);
      }

      BackupPreparation backup = await PrepareBackupAsync(
        request,
        primaryPath,
        backupPath,
        cancellationToken).ConfigureAwait(false);
      if (!backup.CanCommitPrimary)
      {
        return Rejected(
          request,
          backupPath,
          backup.Status,
          backup.Failure,
          payload.Length,
          payloadSha256,
          backup.Failure);
      }

      if (cancellationToken.IsCancellationRequested)
      {
        return Rejected(
          request,
          backupPath,
          backup.Status,
          WorldStorageFailure.Create(WorldStorageFailureKind.Canceled),
          payload.Length,
          payloadSha256,
          backup.Failure);
      }

      ContractWorldFileCommitResult commitResult = await CommitPrimaryAsync(
        request,
        primaryPath,
        payload,
        payloadSha256,
        cancellationToken).ConfigureAwait(false);

      return new WorldDocumentSaveResult(
        primaryPath,
        backupPath,
        request.CapturedVersion,
        request.Document.FormatVersion,
        payload.Length,
        payloadSha256,
        backup.Status,
        backup.Failure,
        commitResult);
    }
  }

  private WorldStorageFailure ValidateDocument(WorldPersistenceDocument document)
  {
    try
    {
      return _validator.Validate(document);
    }
    catch (InvalidDataException exception)
    {
      return WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message);
    }
  }

  private WorldSaveEncodeResult EncodeDocument(WorldPersistenceDocument document)
  {
    try
    {
      return _encoder.Encode(document);
    }
    catch (InvalidDataException exception)
    {
      return WorldSaveEncodeResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message));
    }
  }

  private WorldStorageFailure ValidateEncodedPayload(byte[] payload)
  {
    WorldPersistenceDecodeResult decoded;
    try
    {
      decoded = _decoder.Decode(payload);
    }
    catch (InvalidDataException exception)
    {
      return WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message);
    }

    if (!decoded.Succeeded || decoded.Document is null)
    {
      return decoded.Failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
        : decoded.Failure;
    }

    return ValidateDocument(decoded.Document);
  }

  private async Task<BackupPreparation> PrepareBackupAsync(
    WorldDocumentSaveRequest request,
    string primaryPath,
    string? backupPath,
    CancellationToken cancellationToken)
  {
    if (request.BackupPolicy == WorldBackupPolicy.Disabled)
    {
      return BackupPreparation.Ready(WorldDocumentBackupStatus.Disabled);
    }

    WorldStorageReadResult primary;
    try
    {
      primary = await _fileStore.ReadBoundedAsync(
        primaryPath,
        request.MaximumFileBytes,
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      return BackupPreparation.Stopped(
        WorldDocumentBackupStatus.Rejected,
        CreateFailure(exception));
    }

    if (!primary.Succeeded)
    {
      if (primary.Failure.Kind == WorldStorageFailureKind.Missing)
      {
        return BackupPreparation.Ready(WorldDocumentBackupStatus.NoPreviousPrimary);
      }

      return BackupPreparation.Stopped(
        WorldDocumentBackupStatus.Rejected,
        primary.Failure);
    }

    if (primary.Data is null)
    {
      return BackupPreparation.Ready(
        WorldDocumentBackupStatus.SkippedInvalidPreviousPrimary,
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData));
    }

    if (!IsValidWorld(primary.Data, out WorldStorageFailure validationFailure))
    {
      return BackupPreparation.Ready(
        WorldDocumentBackupStatus.SkippedInvalidPreviousPrimary,
        validationFailure);
    }

    if (backupPath is null)
    {
      return BackupPreparation.Stopped(
        WorldDocumentBackupStatus.Rejected,
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData));
    }

    string previousSha256 = Convert.ToHexString(SHA256.HashData(primary.Data));
    ContractWorldFileCommitResult backupCommit;
    try
    {
      backupCommit = await _fileStore.CommitAtomicallyAsync(
        backupPath,
        primary.Data,
        request.MaximumFileBytes,
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      backupCommit = ContractWorldFileCommitResult.Unknown(CreateFailure(exception));
    }

    if (backupCommit.Status == WorldFileCommitStatus.Rejected)
    {
      return BackupPreparation.Stopped(
        WorldDocumentBackupStatus.Rejected,
        backupCommit.Failure);
    }

    WorldStorageReadResult backupBytes;
    try
    {
      backupBytes = await _fileStore.ReadBoundedAsync(
        backupPath,
        request.MaximumFileBytes,
        CancellationToken.None).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      backupBytes = WorldStorageReadResult.Failed(CreateFailure(exception));
    }

    if (!backupBytes.Succeeded || backupBytes.Data is null)
    {
      WorldStorageFailure failure = backupBytes.Failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
        : backupBytes.Failure;
      return BackupPreparation.Stopped(WorldDocumentBackupStatus.OutcomeUnknown, failure);
    }

    string backupSha256 = Convert.ToHexString(SHA256.HashData(backupBytes.Data));
    if (!string.Equals(previousSha256, backupSha256, StringComparison.Ordinal))
    {
      return BackupPreparation.Stopped(
        WorldDocumentBackupStatus.OutcomeUnknown,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The backup file does not match the previous valid primary after commit."));
    }

    return BackupPreparation.Ready(WorldDocumentBackupStatus.Updated);
  }

  private bool IsValidWorld(byte[] data, out WorldStorageFailure failure)
  {
    WorldPersistenceDecodeResult decoded;
    try
    {
      decoded = _decoder.Decode(data);
    }
    catch (InvalidDataException exception)
    {
      failure = WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, exception.Message);
      return false;
    }

    if (!decoded.Succeeded || decoded.Document is null)
    {
      failure = decoded.Failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
        : decoded.Failure;
      return false;
    }

    failure = ValidateDocument(decoded.Document);
    return failure.Kind == WorldStorageFailureKind.None;
  }

  private async Task<ContractWorldFileCommitResult> CommitPrimaryAsync(
    WorldDocumentSaveRequest request,
    string primaryPath,
    byte[] payload,
    string payloadSha256,
    CancellationToken cancellationToken)
  {
    ContractWorldFileCommitResult commitResult;
    try
    {
      commitResult = await _fileStore.CommitAtomicallyAsync(
        primaryPath,
        payload,
        request.MaximumFileBytes,
        cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      commitResult = ContractWorldFileCommitResult.Unknown(CreateFailure(exception));
    }

    if (commitResult.Status == WorldFileCommitStatus.Rejected)
    {
      return commitResult;
    }

    WorldStorageReadResult committedBytes;
    try
    {
      committedBytes = await _fileStore.ReadBoundedAsync(
        primaryPath,
        request.MaximumFileBytes,
        CancellationToken.None).ConfigureAwait(false);
    }
    catch (Exception exception) when (IsStorageBoundaryFailure(exception))
    {
      committedBytes = WorldStorageReadResult.Failed(CreateFailure(exception));
    }

    if (!committedBytes.Succeeded || committedBytes.Data is null)
    {
      return ContractWorldFileCommitResult.Unknown(
        committedBytes.Failure.Kind == WorldStorageFailureKind.None
          ? WorldStorageFailure.Create(WorldStorageFailureKind.Unknown)
          : committedBytes.Failure);
    }

    string committedSha256 = Convert.ToHexString(SHA256.HashData(committedBytes.Data));
    if (!string.Equals(payloadSha256, committedSha256, StringComparison.Ordinal))
    {
      return ContractWorldFileCommitResult.Unknown(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.Unknown,
          "The primary WorldFile does not match the encoded document after commit."));
    }

    return ContractWorldFileCommitResult.Committed;
  }

  private static WorldDocumentSaveResult Rejected(
    WorldDocumentSaveRequest request,
    string? backupPath,
    WorldDocumentBackupStatus backupStatus,
    WorldStorageFailure failure,
    int payloadLength = 0,
    string payloadSha256 = "",
    WorldStorageFailure backupFailure = default)
  {
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData)
      : failure;
    ContractWorldFileCommitResult commitResult =
      ContractWorldFileCommitResult.Rejected(actualFailure);
    return new WorldDocumentSaveResult(
      request.PrimaryPath,
      backupPath,
      request.CapturedVersion,
      request.Document.FormatVersion,
      payloadLength,
      payloadSha256,
      backupStatus,
      backupFailure,
      commitResult);
  }

  private static WorldStorageFailure CreateFailure(Exception exception)
  {
    WorldStorageFailureKind kind = exception switch
    {
      OperationCanceledException => WorldStorageFailureKind.Canceled,
      UnauthorizedAccessException or System.Security.SecurityException =>
        WorldStorageFailureKind.PermissionDenied,
      PathTooLongException or ArgumentException or NotSupportedException =>
        WorldStorageFailureKind.InvalidPath,
      InvalidDataException => WorldStorageFailureKind.InvalidData,
      IOException => WorldStorageFailureKind.IoFailure,
      _ => WorldStorageFailureKind.Unknown
    };
    return WorldStorageFailure.Create(kind, exception.Message);
  }

  private static bool IsStorageBoundaryFailure(Exception exception) => exception is
    IOException or UnauthorizedAccessException or ArgumentException or
    NotSupportedException or OperationCanceledException or System.Security.SecurityException;

  private static bool IsPathResolutionFailure(Exception exception) => exception is
    ArgumentException or NotSupportedException or PathTooLongException or
    System.Security.SecurityException;

  private static async ValueTask<GateLease> AcquireSaveGateAsync(
    string primaryPath,
    CancellationToken cancellationToken)
  {
    SaveGateEntry entry;
    lock (SaveGatesSync)
    {
      if (!SaveGates.TryGetValue(primaryPath, out entry!))
      {
        entry = new SaveGateEntry();
        SaveGates.Add(primaryPath, entry);
      }

      entry.ReferenceCount++;
    }

    try
    {
      await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
      return new GateLease(primaryPath, entry);
    }
    catch
    {
      ReleaseSaveGateReference(primaryPath, entry, releaseSemaphore: false);
      throw;
    }
  }

  private static void ReleaseSaveGateReference(
    string primaryPath,
    SaveGateEntry entry,
    bool releaseSemaphore)
  {
    if (releaseSemaphore)
    {
      entry.Semaphore.Release();
    }

    lock (SaveGatesSync)
    {
      entry.ReferenceCount--;
      if (entry.ReferenceCount == 0)
      {
        SaveGates.Remove(primaryPath);
        entry.Semaphore.Dispose();
      }
    }
  }

  private sealed class SaveGateEntry
  {
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public int ReferenceCount { get; set; }
  }

  private sealed class GateLease(string primaryPath, SaveGateEntry entry) : IDisposable
  {
    private bool _disposed;

    public void Dispose()
    {
      if (_disposed)
      {
        return;
      }

      _disposed = true;
      ReleaseSaveGateReference(primaryPath, entry, releaseSemaphore: true);
    }
  }

  private readonly record struct BackupPreparation(
    bool CanCommitPrimary,
    WorldDocumentBackupStatus Status,
    WorldStorageFailure Failure)
  {
    public static BackupPreparation Ready(
      WorldDocumentBackupStatus status,
      WorldStorageFailure failure = default)
    {
      return new(true, status, failure);
    }

    public static BackupPreparation Stopped(
      WorldDocumentBackupStatus status,
      WorldStorageFailure failure)
    {
      return new(false, status, failure);
    }
  }
}
