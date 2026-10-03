using System;
using System.Collections.Generic;

namespace Terraria.NonAuthoritative.Persistence;

public sealed class WorldSaveCoordinator
{
  private readonly IWorldFileStore _fileStore;
  private readonly object _saveLock = new();

  public WorldSaveCoordinator(IWorldFileStore fileStore)
  {
    _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
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
    List<WorldSaveStage> stages = new() { WorldSaveStage.Capture };
    if (string.IsNullOrWhiteSpace(command.Path))
    {
      return Reject(
        stages,
        WorldStorageFailure.Create(
        WorldStorageFailureKind.InvalidPath,
        "A world save path is required."));
    }

    if (command.Document is null)
    {
      return Reject(
        stages,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "A world persistence document is required for saving."));
    }

    stages.Add(WorldSaveStage.Encode);
    WorldSaveEncodeResult encoded;
    try
    {
      encoded = encoder.Encode(command.Document);
    }
    catch (OperationCanceledException exception)
    {
      return Reject(
        stages,
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (Exception exception)
    {
      return Reject(
        stages,
        WorldStorageFailure.Create(
          WorldStorageExceptionClassifier.ContainsCancellation(exception)
            ? WorldStorageFailureKind.Canceled
            : WorldStorageFailureKind.InvalidData,
          exception.Message));
    }

    if (encoded.Succeeded && encoded.Failure.Kind != WorldStorageFailureKind.None)
    {
      return Reject(
        stages,
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world save encoder returned success with a failure."));
    }

    if (!encoded.Succeeded)
    {
      WorldStorageFailure failure = encoded.Failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world save encoder returned failure without an error.")
        : encoded.Failure;
      return Reject(stages, failure);
    }

    WorldStorageReadResult previousPrimary = ReadAllBytes(command.Path);
    bool hadPreviousPrimary = previousPrimary.Succeeded && previousPrimary.Data is not null;
    if (!hadPreviousPrimary && previousPrimary.Failure.Kind is not WorldStorageFailureKind.None and not WorldStorageFailureKind.Missing)
    {
      return Reject(stages, previousPrimary.Failure);
    }

    BackupState[] previousBackups = new BackupState[command.BackupPolicy.BackupsToKeep];
    for (int index = 1; index <= previousBackups.Length; index++)
    {
      WorldStorageReadResult backup = ReadAllBytes(GetBackupPath(command.Path, index));
      if (backup.Succeeded && backup.Data is not null)
      {
        previousBackups[index - 1] = new BackupState(true, backup.Data);
      }
      else if (backup.Failure.Kind is not WorldStorageFailureKind.None and not WorldStorageFailureKind.Missing)
      {
        return Reject(stages, backup.Failure);
      }
    }

    stages.Add(WorldSaveStage.Commit);
    WorldStorageOperationResult write = WriteAllBytes(command.Path, encoded.Bytes);
    if (!write.Succeeded)
    {
      WorldStorageOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      return Reject(stages, SelectFailure(write.Failure, restore));
    }

    stages.Add(WorldSaveStage.ReadBack);
    WorldStorageReadResult readBack = ReadAllBytes(command.Path);
    if (!readBack.Succeeded || readBack.Data is null)
    {
      WorldStorageOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      return Reject(stages, SelectFailure(readBack.Failure, restore));
    }

    stages.Add(WorldSaveStage.Validate);
    bool valid;
    try
    {
      valid = validation.IsValid(readBack.Data);
    }
    catch (OperationCanceledException exception)
    {
      valid = false;
      readBack = WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (Exception exception)
    {
      valid = false;
      readBack = WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageExceptionClassifier.ContainsCancellation(exception)
            ? WorldStorageFailureKind.Canceled
            : WorldStorageFailureKind.InvalidData,
          exception.Message));
    }

    if (!valid)
    {
      WorldStorageOperationResult restore = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      WorldStorageFailure failure = readBack.Failure.Kind == WorldStorageFailureKind.None
        ? WorldStorageFailure.Create(WorldStorageFailureKind.InvalidData, "World save validation failed.")
        : readBack.Failure;
      return Reject(stages, SelectFailure(failure, restore));
    }

    stages.Add(WorldSaveStage.Backup);
    WorldStorageOperationResult backupResult = RotateBackups(
      command,
      hadPreviousPrimary,
      previousPrimary.Data,
      previousBackups);
    if (!backupResult.Succeeded)
    {
      WorldStorageOperationResult restorePrimary = RestorePrimary(command, hadPreviousPrimary, previousPrimary.Data);
      WorldStorageOperationResult restoreBackups = RestoreBackups(command, previousBackups);
      return Reject(stages, SelectFailure(backupResult.Failure, restorePrimary, restoreBackups));
    }

    stages.Add(WorldSaveStage.Publish);
    return new WorldSaveProjection(true, stages, WorldStorageFailure.None);
  }

  private WorldStorageOperationResult RotateBackups(
    WorldSaveCommand command,
    bool hadPreviousPrimary,
    byte[]? previousPrimary,
    IReadOnlyList<BackupState> previousBackups)
  {
    if (command.BackupPolicy.BackupsToKeep == 0)
    {
      return WorldStorageOperationResult.Success;
    }

    for (int index = previousBackups.Count; index >= 2; index--)
    {
      WorldStorageOperationResult result = previousBackups[index - 2].Exists
        ? WriteAllBytes(
          GetBackupPath(command.Path, index),
          previousBackups[index - 2].Bytes)
        : Delete(GetBackupPath(command.Path, index), forceDelete: true);
      if (!result.Succeeded)
      {
        return result;
      }
    }

    return hadPreviousPrimary && previousPrimary is not null
      ? WriteAllBytes(GetBackupPath(command.Path, 1), previousPrimary)
      : Delete(GetBackupPath(command.Path, 1), forceDelete: true);
  }

  private WorldStorageOperationResult RestorePrimary(
    WorldSaveCommand command,
    bool hadPreviousPrimary,
    byte[]? previousPrimary)
  {
    return hadPreviousPrimary && previousPrimary is not null
      ? WriteAllBytes(command.Path, previousPrimary)
      : Delete(command.Path, forceDelete: true);
  }

  private WorldStorageOperationResult RestoreBackups(
    WorldSaveCommand command,
    IReadOnlyList<BackupState> previousBackups)
  {
    for (int index = 1; index <= previousBackups.Count; index++)
    {
      WorldStorageOperationResult result = previousBackups[index - 1].Exists
        ? WriteAllBytes(
          GetBackupPath(command.Path, index),
          previousBackups[index - 1].Bytes)
        : Delete(GetBackupPath(command.Path, index), forceDelete: true);
      if (!result.Succeeded)
      {
        return result;
      }
    }

    return WorldStorageOperationResult.Success;
  }

  private static WorldStorageFailure SelectFailure(
    WorldStorageFailure primary,
    params WorldStorageOperationResult[] restorations)
  {
    foreach (WorldStorageOperationResult restoration in restorations)
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
    WorldStorageFailure failure)
  {
    WorldStorageFailure actualFailure = failure.Kind == WorldStorageFailureKind.None
      ? WorldStorageFailure.Create(WorldStorageFailureKind.IoFailure)
      : failure;
    return new WorldSaveProjection(false, stages, actualFailure);
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

  private WorldStorageOperationResult WriteAllBytes(
    string path,
    ReadOnlyMemory<byte> data)
  {
    try
    {
      WorldStorageOperationResult result = _fileStore.WriteAllBytes(path, data);
      if (result.Succeeded && result.Failure.Kind != WorldStorageFailureKind.None)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world file store returned success with a failure."));
      }

      if (!result.Succeeded && result.Failure.Kind == WorldStorageFailureKind.None)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.IoFailure,
            "The world file store returned failure without an error."));
      }

      return result;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageExceptionClassifier.ClassifyExternalFailure(
          exception,
          WorldStorageFailureKind.Unknown));
    }
  }

  private WorldStorageOperationResult Delete(string path, bool forceDelete)
  {
    try
    {
      WorldStorageOperationResult result = _fileStore.Delete(path, forceDelete);
      if (result.Succeeded && result.Failure.Kind != WorldStorageFailureKind.None)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The world file store returned success with a failure."));
      }

      if (!result.Succeeded && result.Failure.Kind == WorldStorageFailureKind.None)
      {
        return WorldStorageOperationResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.IoFailure,
            "The world file store returned failure without an error."));
      }

      return result;
    }
    catch (OperationCanceledException exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.Canceled, exception.Message));
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageExceptionClassifier.ClassifyExternalFailure(
          exception,
          WorldStorageFailureKind.Unknown));
    }
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
