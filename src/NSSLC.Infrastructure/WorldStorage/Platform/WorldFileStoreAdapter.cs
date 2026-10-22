using Terraria.NonAuthoritative.Persistence;

namespace Terraria.NonAuthoritative.Platform;

public sealed class WorldFileStoreAdapter : IWorldFileStore, IMinimalWorldFileStore
{
  private readonly bool _overwriteExistingFiles;

  public WorldFileStoreAdapter(bool overwriteExistingFiles = true) {
    _overwriteExistingFiles = overwriteExistingFiles;
  }

  public async Task<WorldStorageReadResult> ReadBoundedAsync(
    string path,
    int maximumBytes,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    if (maximumBytes <= 0)
    {
      return WorldStorageReadResult.Failed(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          "The world file read limit must be positive."));
    }

    try
    {
      await using FileStream stream = new(
        path,
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read,
        bufferSize: 81920,
        FileOptions.Asynchronous | FileOptions.SequentialScan);
      if (stream.Length > maximumBytes || stream.Length > int.MaxValue)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            $"The world file exceeds the {maximumBytes}-byte read limit."));
      }

      byte[] data = new byte[(int)stream.Length];
      int offset = 0;
      while (offset < data.Length)
      {
        int read = await stream.ReadAsync(data.AsMemory(offset), cancellationToken);
        if (read == 0)
        {
          return WorldStorageReadResult.Failed(
            WorldStorageFailure.Create(
              WorldStorageFailureKind.InvalidData,
              "The world file changed while it was being read."));
        }

        offset += read;
      }

      byte[] overflowProbe = new byte[1];
      if (await stream.ReadAsync(overflowProbe, cancellationToken) != 0)
      {
        return WorldStorageReadResult.Failed(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            $"The world file exceeds the {maximumBytes}-byte read limit."));
      }

      return WorldStorageReadResult.FromBytes(data);
    }
    catch (Exception exception)
    {
      return WorldStorageReadResult.Failed(Classify(exception));
    }
  }

  public async Task<WorldFileCommitResult> CommitAtomicallyAsync(
    string path,
    ReadOnlyMemory<byte> data,
    int maximumBytes,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return WorldFileCommitResult.Rejected(
        WorldStorageFailure.Create(WorldStorageFailureKind.InvalidPath));
    }

    if (maximumBytes <= 0 || data.Length > maximumBytes)
    {
      return WorldFileCommitResult.Rejected(
        WorldStorageFailure.Create(
          WorldStorageFailureKind.InvalidData,
          $"The world file payload exceeds the {maximumBytes}-byte commit limit."));
    }

    string? stagedPath = null;
    bool commitAttempted = false;
    WorldFileCommitResult result;
    try
    {
      string fullPath = Path.GetFullPath(path);
      string directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
      Directory.CreateDirectory(directory);
      stagedPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
      await using (FileStream stream = new(
                     stagedPath,
                     FileMode.CreateNew,
                     FileAccess.Write,
                     FileShare.None,
                     bufferSize: 81920,
                     FileOptions.Asynchronous | FileOptions.WriteThrough))
      {
        await stream.WriteAsync(data, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        stream.Flush(flushToDisk: true);
      }

      byte[] stagedBytes = await File.ReadAllBytesAsync(stagedPath, cancellationToken);
      if (stagedBytes.Length != data.Length ||
          !stagedBytes.AsSpan().SequenceEqual(data.Span))
      {
        return WorldFileCommitResult.Rejected(
          WorldStorageFailure.Create(
            WorldStorageFailureKind.InvalidData,
            "The staged world file does not match the accepted payload."));
      }

      cancellationToken.ThrowIfCancellationRequested();
      commitAttempted = true;
      File.Move(stagedPath, fullPath, overwrite: _overwriteExistingFiles);
      stagedPath = null;
      result = WorldFileCommitResult.Committed;
    }
    catch (Exception exception)
    {
      WorldStorageFailure failure = Classify(exception);
      result = commitAttempted
        ? WorldFileCommitResult.Unknown(failure)
        : WorldFileCommitResult.Rejected(failure);
    }

    if (stagedPath is not null)
    {
      try
      {
        File.Delete(stagedPath);
      }
      catch (Exception cleanupException)
      {
        WorldStorageFailure cleanupFailure = Classify(cleanupException);
        WorldStorageFailure combinedFailure = WorldStorageFailure.Create(
          result.Failure.Kind == WorldStorageFailureKind.None
            ? cleanupFailure.Kind
            : result.Failure.Kind,
          $"{result.Failure.Detail} Temporary-file cleanup failed: {cleanupFailure.Detail}");
        result = result.Status == WorldFileCommitStatus.OutcomeUnknown
          ? WorldFileCommitResult.Unknown(combinedFailure)
          : WorldFileCommitResult.Rejected(combinedFailure);
      }
    }

    return result;
  }

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
