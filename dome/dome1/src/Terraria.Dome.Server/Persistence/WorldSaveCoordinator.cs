using System;
using System.IO;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Persistence;

public sealed class WorldSaveCoordinator
{
  public const int DefaultRollingBackupsCountToKeep = 2;
  private const int MaximumRollingBackupsCountToKeep = 32;

  public void Save(string path, WorldGridSnapshot snapshot)
  {
    Save(path, snapshot, DefaultRollingBackupsCountToKeep);
  }

  public void Save(string path, WorldGridSnapshot snapshot, int rollingBackupsCountToKeep)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(snapshot);
    if (rollingBackupsCountToKeep < 0 ||
        rollingBackupsCountToKeep > MaximumRollingBackupsCountToKeep)
    {
      throw new ArgumentOutOfRangeException(nameof(rollingBackupsCountToKeep));
    }

    string fullPath = Path.GetFullPath(path);
    string? directory = Path.GetDirectoryName(fullPath);
    if (string.IsNullOrEmpty(directory))
    {
      throw new InvalidOperationException("The world save path requires a parent directory.");
    }

    Directory.CreateDirectory(directory);
    string temporaryName = $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp";
    string temporaryPath = Path.Combine(directory, temporaryName);
    try
    {
      using (FileStream output = new(
        temporaryPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None,
        bufferSize: 4096,
        FileOptions.WriteThrough))
      {
        WorldPersistenceFormat.Write(output, snapshot);
        output.Flush(flushToDisk: true);
      }

      if (File.Exists(fullPath))
      {
        RotateBackups(fullPath, rollingBackupsCountToKeep);
        File.Replace(temporaryPath, fullPath, destinationBackupFileName: null);
      }
      else
      {
        File.Move(temporaryPath, fullPath);
      }
    }
    finally
    {
      if (File.Exists(temporaryPath))
      {
        File.Delete(temporaryPath);
      }
    }
  }

  private static void RotateBackups(string fullPath, int countToKeep)
  {
    if (countToKeep == 0)
    {
      return;
    }

    for (int index = countToKeep; index >= 2; index--)
    {
      string sourcePath = GetBackupPath(fullPath, index - 1);
      string destinationPath = GetBackupPath(fullPath, index);
      if (File.Exists(sourcePath))
      {
        File.Move(sourcePath, destinationPath, overwrite: true);
      }
    }

    File.Copy(fullPath, GetBackupPath(fullPath, 1), overwrite: true);
  }

  private static string GetBackupPath(string fullPath, int index)
  {
    return index == 1 ? fullPath + ".bak" : $"{fullPath}.bak{index}";
  }

  public WorldRecoveryResult TryLoad(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    try
    {
      using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      return WorldRecoveryResult.Success(WorldPersistenceFormat.Read(input));
    }
    catch (Exception exception) when (exception is IOException ||
                                     exception is InvalidDataException ||
                                     exception is OverflowException ||
                                     exception is ArgumentException)
    {
      return WorldRecoveryResult.Failure(exception.Message);
    }
  }
}
