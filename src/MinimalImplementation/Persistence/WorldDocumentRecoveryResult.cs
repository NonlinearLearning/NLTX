using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

public sealed record WorldDocumentRecoveryResult(
  WorldPersistenceDocument? Document,
  ReadOnlyMemory<byte> SourceBytes,
  WorldDocumentLoadSource Source,
  WorldStorageFailure PrimaryFailure,
  WorldStorageFailure BackupFailure,
  bool BackupAttempted)
{
  public bool Succeeded => Document is not null && Source != WorldDocumentLoadSource.None;

  public static WorldDocumentRecoveryResult FromPrimary(
    WorldPersistenceDocument document,
    ReadOnlyMemory<byte> sourceBytes)
  {
    ArgumentNullException.ThrowIfNull(document);
    return new(document, sourceBytes.ToArray(), WorldDocumentLoadSource.Primary, WorldStorageFailure.None,
      WorldStorageFailure.None, false);
  }

  public static WorldDocumentRecoveryResult FromBackup(
    WorldPersistenceDocument document,
    WorldStorageFailure primaryFailure,
    ReadOnlyMemory<byte> sourceBytes)
  {
    ArgumentNullException.ThrowIfNull(document);
    ArgumentOutOfRangeException.ThrowIfEqual(
      primaryFailure.Kind,
      WorldStorageFailureKind.None);
    return new(document, sourceBytes.ToArray(), WorldDocumentLoadSource.Backup, primaryFailure, WorldStorageFailure.None,
      true);
  }

  public static WorldDocumentRecoveryResult Rejected(
    WorldStorageFailure primaryFailure,
    WorldStorageFailure backupFailure,
    bool backupAttempted = false)
  {
    ArgumentOutOfRangeException.ThrowIfEqual(
      primaryFailure.Kind,
      WorldStorageFailureKind.None);
    return new(null, ReadOnlyMemory<byte>.Empty, WorldDocumentLoadSource.None, primaryFailure, backupFailure,
      backupAttempted);
  }
}
