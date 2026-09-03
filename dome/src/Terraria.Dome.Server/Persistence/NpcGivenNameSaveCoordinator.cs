using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Dome.Simulation.Npc.Snapshots;

namespace Terraria.Dome.Server.Persistence;

public sealed class NpcGivenNameSaveCoordinator
{
  public IReadOnlyList<NpcStateSnapshot> Apply(
    IReadOnlyList<NpcStateSnapshot> states,
    IReadOnlyList<NpcGivenNameEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(states);
    ArgumentNullException.ThrowIfNull(entries);
    Dictionary<int, string> names = new(entries.Count);
    for (int index = 0; index < entries.Count; index++)
    {
      NpcGivenNameEntry entry = entries[index];
      if (entry.ReplicationId <= 0 || entry.GivenName is null ||
          !names.TryAdd(entry.ReplicationId, entry.GivenName))
      {
        throw new ArgumentException("NPC given-name entries contain duplicate or invalid state.",
          nameof(entries));
      }
    }

    List<NpcStateSnapshot> result = new(states.Count);
    HashSet<int> stateIds = new();
    for (int index = 0; index < states.Count; index++)
    {
      NpcStateSnapshot state = states[index];
      if (!stateIds.Add(state.Replication.ReplicationId))
      {
        throw new ArgumentException("NPC states contain duplicate replication IDs.", nameof(states));
      }

      if (names.TryGetValue(state.Replication.ReplicationId, out string? givenName))
      {
        result.Add(state with { GivenName = givenName });
      }
      else
      {
        result.Add(state);
      }
    }

    return result;
  }

  public void Save(string path, IReadOnlyList<NpcGivenNameEntry> entries)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(entries);
    string fullPath = Path.GetFullPath(path);
    string temporaryPath = fullPath + ".tmp";
    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
    using (FileStream output = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
      NpcGivenNamePersistenceFormat.Write(output, entries);
      output.Flush(flushToDisk: true);
    }

    File.Move(temporaryPath, fullPath, overwrite: true);
  }

  public IReadOnlyList<NpcGivenNameEntry> Load(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string fullPath = Path.GetFullPath(path);
    string temporaryPath = fullPath + ".tmp";
    if (TryRead(fullPath, out IReadOnlyList<NpcGivenNameEntry>? current))
    {
      if (File.Exists(temporaryPath))
      {
        File.Delete(temporaryPath);
      }

      return current!;
    }

    if (TryRead(temporaryPath, out IReadOnlyList<NpcGivenNameEntry>? recovered))
    {
      if (File.Exists(fullPath))
      {
        File.Replace(temporaryPath, fullPath, destinationBackupFileName: null);
      }
      else
      {
        File.Move(temporaryPath, fullPath);
      }

      return recovered!;
    }

    if (!File.Exists(fullPath) && !File.Exists(temporaryPath))
    {
      return Array.Empty<NpcGivenNameEntry>();
    }

    throw new InvalidDataException("NPC given-name sidecar and recovery candidate are invalid.");
  }

  private static bool TryRead(
    string path,
    out IReadOnlyList<NpcGivenNameEntry>? entries)
  {
    entries = null;
    if (!File.Exists(path))
    {
      return false;
    }

    try
    {
      using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      entries = NpcGivenNamePersistenceFormat.Read(input);
      return true;
    }
    catch (IOException)
    {
      return false;
    }
    catch (InvalidDataException)
    {
      return false;
    }
  }
}
