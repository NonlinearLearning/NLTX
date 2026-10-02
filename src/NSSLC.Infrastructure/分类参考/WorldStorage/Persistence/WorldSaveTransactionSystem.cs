using Terraria.NonAuthoritative.Platform;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldSaveTransactionSystem
{
  private readonly FilePlatformAdapter _platform;
  private readonly object _saveLock = new();

  public WorldSaveTransactionSystem(FilePlatformAdapter platform)
  {
    _platform = platform ?? throw new ArgumentNullException(nameof(platform));
  }

  public WorldSaveProjection Save(
    WorldSaveCommand command,
    IWorldSaveEncoder encoder,
    WorldSaveValidationQuery validation)
  {
    ArgumentNullException.ThrowIfNull(encoder);
    ArgumentNullException.ThrowIfNull(validation);

    lock (_saveLock)
    {
      return SaveUnderLock(command, encoder, validation);
    }
  }

  private WorldSaveProjection SaveUnderLock(
    WorldSaveCommand command,
    IWorldSaveEncoder encoder,
    WorldSaveValidationQuery validation)
  {
    List<WorldSaveStage> stages = new() { WorldSaveStage.Capture, WorldSaveStage.Encode };
    WorldSaveEncodeResult encoded;
    try
    {
      encoded = encoder.Encode(command);
    }
    catch (Exception exception)
    {
      return Reject(stages, FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
    }

    if (!encoded.Succeeded)
    {
      return Reject(stages, encoded.Failure);
    }

    FileReadResult previousPrimary = _platform.ReadAllBytes(command.Path, command.IsCloudSave);
    bool hadPreviousPrimary = previousPrimary.Succeeded && previousPrimary.Data is not null;
    if (!hadPreviousPrimary && previousPrimary.Failure.Kind is not FilePlatformFailureKind.None and not FilePlatformFailureKind.Missing)
    {
      return Reject(stages, previousPrimary.Failure);
    }

    BackupState[] previousBackups = new BackupState[command.BackupPolicy.BackupsToKeep];
    for (int index = 1; index <= previousBackups.Length; index++)
    {
      FileReadResult backup = _platform.ReadAllBytes(GetBackupPath(command.Path, index), command.IsCloudSave);
      if (backup.Succeeded && backup.Data is not null)
      {
        previousBackups[index - 1] = new BackupState(true, backup.Data);
      }
      else if (backup.Failure.Kind is not FilePlatformFailureKind.None and not FilePlatformFailureKind.Missing)
      {
        return Reject(stages, backup.Failure);
      }
    }

    stages.Add(WorldSaveStage.Commit);
    FilePlatformOperationResult write = _platform.WriteAllBytes(command.Path, encoded.Bytes, command.IsCloudSave);
    if (!write.Succeeded)
    {
      FilePlatformOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      return Reject(stages, SelectFailure(write.Failure, restore));
    }

    stages.Add(WorldSaveStage.ReadBack);
    FileReadResult readBack = _platform.ReadAllBytes(command.Path, command.IsCloudSave);
    if (!readBack.Succeeded || readBack.Data is null)
    {
      FilePlatformOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      return Reject(stages, SelectFailure(readBack.Failure, restore));
    }

    stages.Add(WorldSaveStage.Validate);
    bool valid;
    try
    {
      valid = validation.IsValid(readBack.Data);
    }
    catch (Exception exception)
    {
      valid = false;
      readBack = FileReadResult.Failed(
        FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, exception.Message));
    }

    if (!valid)
    {
      FilePlatformOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      FilePlatformFailure failure = readBack.Failure.Kind == FilePlatformFailureKind.None
        ? FilePlatformFailure.Create(FilePlatformFailureKind.InvalidData, "World save validation failed.")
        : readBack.Failure;
      return Reject(stages, SelectFailure(failure, restore));
    }

    stages.Add(WorldSaveStage.Backup);
    FilePlatformOperationResult backupResult = RotateBackups(
      command,
      hadPreviousPrimary,
      previousPrimary.Data,
      previousBackups);
    if (!backupResult.Succeeded)
    {
      FilePlatformOperationResult restorePrimary = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      FilePlatformOperationResult restoreBackups = RestoreBackups(command, previousBackups);
      return Reject(stages, SelectFailure(backupResult.Failure, restorePrimary, restoreBackups));
    }

    stages.Add(WorldSaveStage.Publish);
    return new WorldSaveProjection(true, stages, FilePlatformFailure.None);
  }

  private FilePlatformOperationResult RotateBackups(
    WorldSaveCommand command,
    bool hadPreviousPrimary,
    byte[]? previousPrimary,
    IReadOnlyList<BackupState> previousBackups)
  {
    for (int index = previousBackups.Count; index >= 2; index--)
    {
      FilePlatformOperationResult result = previousBackups[index - 2].Exists
        ? _platform.WriteAllBytes(
          GetBackupPath(command.Path, index),
          previousBackups[index - 2].Bytes,
          command.IsCloudSave)
        : _platform.Delete(GetBackupPath(command.Path, index), command.IsCloudSave, forceDelete: true);
      if (!result.Succeeded)
      {
        return result;
      }
    }

    return hadPreviousPrimary && previousPrimary is not null
      ? _platform.WriteAllBytes(GetBackupPath(command.Path, 1), previousPrimary, command.IsCloudSave)
      : _platform.Delete(GetBackupPath(command.Path, 1), command.IsCloudSave, forceDelete: true);
  }

  private FilePlatformOperationResult RestorePrimary(
    WorldSaveCommand command,
    bool hadPreviousPrimary,
    byte[]? previousPrimary)
  {
    return hadPreviousPrimary && previousPrimary is not null
      ? _platform.WriteAllBytes(command.Path, previousPrimary, command.IsCloudSave)
      : _platform.Delete(command.Path, command.IsCloudSave, forceDelete: true);
  }

  private FilePlatformOperationResult RestoreBackups(
    WorldSaveCommand command,
    IReadOnlyList<BackupState> previousBackups)
  {
    for (int index = 1; index <= previousBackups.Count; index++)
    {
      FilePlatformOperationResult result = previousBackups[index - 1].Exists
        ? _platform.WriteAllBytes(
          GetBackupPath(command.Path, index),
          previousBackups[index - 1].Bytes,
          command.IsCloudSave)
        : _platform.Delete(GetBackupPath(command.Path, index), command.IsCloudSave, forceDelete: true);
      if (!result.Succeeded)
      {
        return result;
      }
    }

    return FilePlatformOperationResult.Success;
  }

  private static FilePlatformFailure SelectFailure(
    FilePlatformFailure primary,
    params FilePlatformOperationResult[] restorations)
  {
    foreach (FilePlatformOperationResult restoration in restorations)
    {
      if (!restoration.Succeeded)
      {
        return restoration.Failure;
      }
    }

    return primary;
  }

  private static WorldSaveProjection Reject(
    IReadOnlyList<WorldSaveStage> stages,
    FilePlatformFailure failure)
  {
    FilePlatformFailure actualFailure = failure.Kind == FilePlatformFailureKind.None
      ? FilePlatformFailure.Create(FilePlatformFailureKind.IoFailure)
      : failure;
    return new WorldSaveProjection(false, stages, actualFailure);
  }

  private static string GetBackupPath(string path, int index)
  {
    return index == 1 ? path + ".bak" : path + ".bak" + index;
  }

  private readonly record struct BackupState
  {
    public bool Exists { get; }

    public byte[] Bytes { get; }

    public BackupState(bool exists, byte[] bytes)
      : this()
    {
      Exists = exists;
      Bytes = bytes;
    }
  }
}
