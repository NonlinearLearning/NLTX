namespace Terraria.NonAuthoritative.Platform;

public sealed class FilePlatformAdapter
{
  private readonly ICloudFileStore? _cloudFileStore;

  public FilePlatformAdapter(ICloudFileStore? cloudFileStore = null)
  {
    _cloudFileStore = cloudFileStore;
  }

  public FileExistenceResult Exists(string path, bool isCloud)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return FileExistenceResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FileExistenceResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        return _cloudFileStore.Exists(path)
          ? FileExistenceResult.Present
          : FileExistenceResult.Absent;
      }
      catch (Exception exception)
      {
        return FileExistenceResult.Failed(Classify(exception, provider: true));
      }
    }

    try
    {
      return File.Exists(path) ? FileExistenceResult.Present : FileExistenceResult.Absent;
    }
    catch (Exception exception)
    {
      return FileExistenceResult.Failed(Classify(exception, provider: false));
    }
  }

  public FileReadResult ReadAllBytes(string path, bool isCloud)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return FileReadResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FileReadResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        byte[] bytes = _cloudFileStore.ReadAllBytes(path);
        return FileReadResult.FromBytes(bytes);
      }
      catch (Exception exception)
      {
        return FileReadResult.Failed(Classify(exception, provider: true));
      }
    }

    try
    {
      return FileReadResult.FromBytes(File.ReadAllBytes(path));
    }
    catch (Exception exception)
    {
      return FileReadResult.Failed(Classify(exception, provider: false));
    }
  }

  public FilePlatformOperationResult WriteAllBytes(
    string path,
    ReadOnlyMemory<byte> data,
    bool isCloud)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        return _cloudFileStore.WriteAllBytes(path, data)
          ? FilePlatformOperationResult.Success
          : FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderRejected));
      }
      catch (Exception exception)
      {
        return FilePlatformOperationResult.Failed(Classify(exception, provider: true));
      }
    }

    try
    {
      string parentPath = FilePathParsingAdapter.GetParentFolderPath(path);
      if (!string.IsNullOrEmpty(parentPath))
      {
        Directory.CreateDirectory(parentPath);
      }

      RemoveReadOnlyAttribute(path);
      File.WriteAllBytes(path, data.ToArray());
      return FilePlatformOperationResult.Success;
    }
    catch (Exception exception)
    {
      return FilePlatformOperationResult.Failed(Classify(exception, provider: false));
    }
  }

  public FilePlatformOperationResult Copy(
    string sourcePath,
    string destinationPath,
    bool isCloud)
  {
    if (!ArePathsValid(sourcePath, destinationPath))
    {
      return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        return _cloudFileStore.Copy(sourcePath, destinationPath)
          ? FilePlatformOperationResult.Success
          : FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderRejected));
      }
      catch (Exception exception)
      {
        return FilePlatformOperationResult.Failed(Classify(exception, provider: true));
      }
    }

    try
    {
      string parentPath = FilePathParsingAdapter.GetParentFolderPath(destinationPath);
      if (!string.IsNullOrEmpty(parentPath))
      {
        Directory.CreateDirectory(parentPath);
      }

      File.Copy(sourcePath, destinationPath, overwrite: true);
      return FilePlatformOperationResult.Success;
    }
    catch (Exception exception)
    {
      return FilePlatformOperationResult.Failed(Classify(exception, provider: false));
    }
  }

  public FilePlatformOperationResult Move(
    string sourcePath,
    string destinationPath,
    bool isCloud)
  {
    if (!ArePathsValid(sourcePath, destinationPath))
    {
      return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        return _cloudFileStore.Move(sourcePath, destinationPath)
          ? FilePlatformOperationResult.Success
          : FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderRejected));
      }
      catch (Exception exception)
      {
        return FilePlatformOperationResult.Failed(Classify(exception, provider: true));
      }
    }

    try
    {
      string parentPath = FilePathParsingAdapter.GetParentFolderPath(destinationPath);
      if (!string.IsNullOrEmpty(parentPath))
      {
        Directory.CreateDirectory(parentPath);
      }

      if (File.Exists(destinationPath))
      {
        RemoveReadOnlyAttribute(destinationPath);
        File.Delete(destinationPath);
      }

      File.Move(sourcePath, destinationPath);
      return FilePlatformOperationResult.Success;
    }
    catch (IOException)
    {
      FilePlatformOperationResult copyResult = Copy(sourcePath, destinationPath, isCloud: false);
      return copyResult.Succeeded
        ? Delete(sourcePath, isCloud: false, forceDelete: true)
        : copyResult;
    }
    catch (Exception exception)
    {
      return FilePlatformOperationResult.Failed(Classify(exception, provider: false));
    }
  }

  public FilePlatformOperationResult Delete(
    string path,
    bool isCloud,
    bool forceDelete = false)
  {
    if (string.IsNullOrWhiteSpace(path))
    {
      return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.InvalidPath));
    }

    if (isCloud)
    {
      if (_cloudFileStore is null)
      {
        return FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderUnavailable));
      }

      try
      {
        return _cloudFileStore.Delete(path)
          ? FilePlatformOperationResult.Success
          : FilePlatformOperationResult.Failed(Failure(FilePlatformFailureKind.ProviderRejected));
      }
      catch (Exception exception)
      {
        return FilePlatformOperationResult.Failed(Classify(exception, provider: true));
      }
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

      return FilePlatformOperationResult.Success;
    }
    catch (Exception exception)
    {
      return FilePlatformOperationResult.Failed(Classify(exception, provider: false));
    }
  }

  public static string GetFullPath(string path, bool isCloud)
  {
    return FilePathParsingAdapter.GetFullPath(path, isCloud);
  }

  private static bool ArePathsValid(string sourcePath, string destinationPath)
  {
    return !string.IsNullOrWhiteSpace(sourcePath) && !string.IsNullOrWhiteSpace(destinationPath);
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

  private static FilePlatformFailure Failure(FilePlatformFailureKind kind)
  {
    return FilePlatformFailure.Create(kind);
  }

  private static FilePlatformFailure Classify(Exception exception, bool provider)
  {
    FilePlatformFailureKind kind = exception switch
    {
      OperationCanceledException => FilePlatformFailureKind.Canceled,
      UnauthorizedAccessException => FilePlatformFailureKind.PermissionDenied,
      FileNotFoundException or DirectoryNotFoundException => FilePlatformFailureKind.Missing,
      PathTooLongException or ArgumentException or NotSupportedException => FilePlatformFailureKind.InvalidPath,
      IOException => provider
        ? FilePlatformFailureKind.ResultUnknown
        : FilePlatformFailureKind.IoFailure,
      _ => provider
        ? FilePlatformFailureKind.ResultUnknown
        : FilePlatformFailureKind.IoFailure
    };
    return FilePlatformFailure.Create(kind, exception.Message);
  }
}
