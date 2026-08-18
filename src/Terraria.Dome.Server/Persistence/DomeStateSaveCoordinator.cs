using System;
using System.IO;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Persistence;

public sealed class DomeStateSaveCoordinator
{
  public void Save(string path, DomeSimulationSnapshot snapshot)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(snapshot);
    string fullPath = Path.GetFullPath(path);
    string? directory = Path.GetDirectoryName(fullPath);
    if (string.IsNullOrEmpty(directory))
    {
      throw new InvalidOperationException("The Dome state path requires a parent directory.");
    }

    Directory.CreateDirectory(directory);
    string temporaryPath = Path.Combine(
      directory,
      $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
    try
    {
      using (FileStream output = new(
        temporaryPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None,
        4096,
        FileOptions.WriteThrough))
      {
        DomeStatePersistenceFormat.Write(output, snapshot);
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

  public DomeStateRecoveryResult TryLoad(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    try
    {
      using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      return DomeStateRecoveryResult.Success(DomeStatePersistenceFormat.Read(input));
    }
    catch (Exception exception) when (exception is IOException ||
                                     exception is InvalidDataException ||
                                     exception is OverflowException ||
                                     exception is ArgumentException)
    {
      return DomeStateRecoveryResult.Failure(exception.Message);
    }
  }
}
