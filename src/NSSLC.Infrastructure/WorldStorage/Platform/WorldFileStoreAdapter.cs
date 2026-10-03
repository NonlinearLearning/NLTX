using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.Platform;

public sealed class WorldFileStoreAdapter : IWorldFileStore
{
  public WorldStorageOperationResult CheckBackupExists(
    string worldPath,
    out bool backupExists)
  {
    backupExists = false;
    if (string.IsNullOrWhiteSpace(worldPath))
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    try
    {
      using FileStream backup = new(
        worldPath + ".bak",
        FileMode.Open,
        FileAccess.Read,
        FileShare.ReadWrite | FileShare.Delete);
      backupExists = true;
      return WorldStorageOperationResult.Success;
    }
    catch (FileNotFoundException)
    {
      return WorldStorageOperationResult.Success;
    }
    catch (DirectoryNotFoundException)
    {
      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(Classify(exception));
    }
  }

  public WorldStorageOperationResult RestoreBackupAndDelete(string worldPath)
  {
    if (string.IsNullOrWhiteSpace(worldPath))
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    string backupPath = worldPath + ".bak";
    string temporaryPath = worldPath + $".restore-{Guid.NewGuid():N}.tmp";
    try
    {
      using (FileStream backup = new(
               backupPath,
               FileMode.Open,
               FileAccess.Read,
               FileShare.ReadWrite | FileShare.Delete))
      {
      }

      File.Copy(backupPath, temporaryPath, overwrite: false);
      RemoveReadOnlyAttribute(worldPath);
      File.Move(temporaryPath, worldPath, overwrite: true);
      RemoveReadOnlyAttribute(backupPath);
      File.Delete(backupPath);
      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      WorldStorageFailure failure = Classify(exception);
      try
      {
        File.Delete(temporaryPath);
      }
      catch (Exception cleanupException)
      {
        failure = WorldStorageFailure.Create(
          ContainsCancellation(cleanupException)
            ? WorldStorageFailureKind.Canceled
            : failure.Kind,
          $"{failure.Detail} Temporary-file cleanup failed: {cleanupException.Message}");
      }

      return WorldStorageOperationResult.Failed(failure);
    }
  }

  public WorldStorageReadResult ReadAllBytes(string path)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    try
    {
      return WorldStorageReadResult.FromBytes(File.ReadAllBytes(path));
    }
    catch (Exception exception)
    {
      return WorldStorageReadResult.Failed(Classify(exception));
    }
  }

  public WorldStorageOperationResult WriteAllBytes(
    string path,
    ReadOnlyMemory<byte> data)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    try
    {
      string parentPath = Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty;
      if (!string.IsNullOrEmpty(parentPath))
      {
        Directory.CreateDirectory(parentPath);
      }

      RemoveReadOnlyAttribute(path);
      File.WriteAllBytes(path, data.ToArray());
      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(Classify(exception));
    }
  }

  public WorldStorageOperationResult Delete(string path, bool forceDelete)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return WorldStorageOperationResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    try
    {
      if (File.Exists(path))
      {
        if (forceDelete)
        {
          RemoveReadOnlyAttribute(path);
        }

        File.Delete(path);
      }

      return WorldStorageOperationResult.Success;
    }
    catch (Exception exception)
    {
      return WorldStorageOperationResult.Failed(Classify(exception));
    }
  }

  private static WorldStorageFailure Classify(Exception exception)
  {
    WorldStorageFailureKind kind = ContainsCancellation(exception)
      ? WorldStorageFailureKind.Canceled
      : exception switch
      {
        UnauthorizedAccessException => WorldStorageFailureKind.PermissionDenied,
        FileNotFoundException or DirectoryNotFoundException => WorldStorageFailureKind.Missing,
        PathTooLongException or ArgumentException or NotSupportedException => WorldStorageFailureKind.InvalidPath,
        IOException => WorldStorageFailureKind.IoFailure,
        _ => WorldStorageFailureKind.IoFailure
      };

    return WorldStorageFailure.Create(kind, exception.Message);
  }

  private static bool ContainsCancellation(Exception exception)
  {
    if (exception is OperationCanceledException)
    {
      return true;
    }

    if (exception is not AggregateException aggregate)
    {
      return false;
    }

    foreach (Exception innerException in aggregate.InnerExceptions)
    {
      if (ContainsCancellation(innerException))
      {
        return true;
      }
    }

    return exception.InnerException is not null &&
      ContainsCancellation(exception.InnerException);
  }

  private static void RemoveReadOnlyAttribute(string path)
  {
    if (!File.Exists(path))
    {
      return;
    }

    FileAttributes attributes = File.GetAttributes(path);
    if ((attributes & FileAttributes.ReadOnly) != 0)
    {
      File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
    }
  }
}
