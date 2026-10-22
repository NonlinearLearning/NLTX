using System;
using MinimalImplementation.Contracts;
using Terraria.NonAuthoritative.Persistence;

namespace MinimalImplementation.Persistence;

/// <summary>Captures a complete immutable document selected for persistence.</summary>
public sealed class WorldDocumentSaveRequest
{
  public WorldDocumentSaveRequest(
    string primaryPath,
    CommitVersion capturedVersion,
    WorldPersistenceDocument document,
    int maximumFileBytes,
    WorldBackupPolicy backupPolicy = WorldBackupPolicy.KeepPreviousPrimary)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(primaryPath);
    ArgumentNullException.ThrowIfNull(document);
    if (!capturedVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(capturedVersion));
    }

    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFileBytes);
    if (!Enum.IsDefined(backupPolicy))
    {
      throw new ArgumentOutOfRangeException(nameof(backupPolicy));
    }

    PrimaryPath = primaryPath;
    CapturedVersion = capturedVersion;
    Document = document;
    MaximumFileBytes = maximumFileBytes;
    BackupPolicy = backupPolicy;
  }

  public string PrimaryPath { get; }

  public CommitVersion CapturedVersion { get; }

  public WorldPersistenceDocument Document { get; }

  public int MaximumFileBytes { get; }

  public WorldBackupPolicy BackupPolicy { get; }
}
