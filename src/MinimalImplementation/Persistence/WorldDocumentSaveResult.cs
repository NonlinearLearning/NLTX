using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;
using ContractWorldFileCommitResult = Terraria.NonAuthoritative.Persistence.WorldFileCommitResult;

namespace MinimalImplementation.Persistence;

/// <summary>
/// Reports the captured document, backup result and observed primary-file commit result.
/// </summary>
public sealed record WorldDocumentSaveResult(
  string PrimaryPath,
  string? BackupPath,
  CommitVersion CapturedVersion,
  int FormatVersion,
  int PayloadLength,
  string PayloadSha256,
  WorldDocumentBackupStatus BackupStatus,
  WorldStorageFailure BackupFailure,
  ContractWorldFileCommitResult CommitResult)
{
  public bool BackupUpdated => BackupStatus == WorldDocumentBackupStatus.Updated;

  public bool IsPrimaryCommitted =>
    CommitResult.Status == Terraria.NonAuthoritative.Persistence.WorldFileCommitStatus.Committed;
}
