using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldRecoveryTransactionAdapter
{
  private readonly FilePlatformAdapter _platform;
  private readonly Func<ReadOnlyMemory<byte>, bool> _validator;
  private readonly object _loadLock = new();

  public WorldRecoveryTransactionAdapter(
    FilePlatformAdapter platform,
    Func<ReadOnlyMemory<byte>, bool> validator)
  {
    _platform = platform ?? throw new ArgumentNullException(nameof(platform));
    _validator = validator ?? throw new ArgumentNullException(nameof(validator));
  }

  public WorldRecoveryOutcome Load(
    string path,
    bool isCloudSave,
    WorldRecoveryPolicy policy)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return Failed(0, FilePlatformFailure.Create(FilePlatformFailureKind.InvalidPath));
    }

    lock (_loadLock)
    {
      return LoadUnderLock(path, isCloudSave, policy);
    }
  }

  private WorldRecoveryOutcome LoadUnderLock(
    string path,
    bool isCloudSave,
    WorldRecoveryPolicy policy)
  {
    int attempts = 0;
    FilePlatformFailure lastFailure = FilePlatformFailure.Create(FilePlatformFailureKind.Missing);
    byte[]? lastPrimaryBytes = null;

    for (int attempt = 0; attempt < policy.NormalAttempts; attempt++)
    {
      attempts++;
      FileReadResult primary = _platform.ReadAllBytes(path, isCloudSave);
      if (primary.Succeeded && primary.Data is not null)
      {
        lastPrimaryBytes = primary.Data;
        if (IsValid(primary.Data, out FilePlatformFailure validationFailure))
        {
          return new WorldRecoveryOutcome(WorldRecoveryStatus.Loaded, primary.Data, attempts, FilePlatformFailure.None);
        }

        lastFailure = validationFailure;
      }
      else
      {
        lastFailure = primary.Failure;
        if (primary.Failure.Kind == FilePlatformFailureKind.ProviderUnavailable)
        {
          return Failed(attempts, primary.Failure);
        }
      }
    }

    if (policy.BackupAttempts == 0)
    {
      return Failed(attempts, lastFailure);
    }

    string backupPath = path + ".bak";
    for (int attempt = 0; attempt < policy.BackupAttempts; attempt++)
    {
      attempts++;
      FileReadResult backup = _platform.ReadAllBytes(backupPath, isCloudSave);
      if (!backup.Succeeded || backup.Data is null)
      {
        lastFailure = backup.Failure;
        continue;
      }

      if (!IsValid(backup.Data, out FilePlatformFailure backupValidationFailure))
      {
        lastFailure = backupValidationFailure;
        continue;
      }

      FilePlatformOperationResult write = _platform.WriteAllBytes(path, backup.Data, isCloudSave);
      if (!write.Succeeded)
      {
        lastFailure = write.Failure;
        RestorePrimary(path, isCloudSave, lastPrimaryBytes);
        continue;
      }

      FileReadResult readBack = _platform.ReadAllBytes(path, isCloudSave);
      if (readBack.Succeeded && readBack.Data is not null &&
        IsValid(readBack.Data, out _))
      {
        return new WorldRecoveryOutcome(
          WorldRecoveryStatus.RecoveredFromBackup,
          readBack.Data,
          attempts,
          FilePlatformFailure.None);
      }

      lastFailure = readBack.Failure.Kind == FilePlatformFailureKind.None
        ? FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, "Recovered world read-back failed.")
        : readBack.Failure;
      RestorePrimary(path, isCloudSave, lastPrimaryBytes);
    }

    return lastFailure.Kind == FilePlatformFailureKind.Missing
      ? new WorldRecoveryOutcome(WorldRecoveryStatus.Missing, null, attempts, lastFailure)
      : Failed(attempts, lastFailure);
  }

  private bool IsValid(
    ReadOnlyMemory<byte> bytes,
    out FilePlatformFailure failure)
  {
    try
    {
      if (_validator(bytes))
      {
        failure = FilePlatformFailure.None;
        return true;
      }
    }
    catch (Exception exception)
    {
      failure = FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message);
      return false;
    }

    failure = FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, "World validation failed.");
    return false;
  }

  private void RestorePrimary(string path, bool isCloudSave, byte[]? previousBytes)
  {
    if (previousBytes is not null)
    {
      _platform.WriteAllBytes(path, previousBytes, isCloudSave);
    }
    else
    {
      _platform.Delete(path, isCloudSave, forceDelete: true);
    }
  }

  private static WorldRecoveryOutcome Failed(int attempts, FilePlatformFailure failure)
  {
    FilePlatformFailure actualFailure = failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.Unknown)
      : failure;
    return new WorldRecoveryOutcome(WorldRecoveryStatus.Failed, null, attempts, actualFailure);
  }
}
