using System;
using System.IO;
using Terraria.Dome.Simulation.WorldModel;

namespace Terraria.Dome.Server.Persistence;

public sealed class WorldSaveCoordinator
{
  public void Save(string path, WorldGridSnapshot snapshot)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(snapshot);
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
