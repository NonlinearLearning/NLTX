using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;

namespace Terraria.Dome.Server.Persistence;

public sealed class NpcProjectileCursorSaveCoordinator
{
  public IReadOnlyList<NpcProjectileCursorAccountState> Compact(
    IReadOnlyList<NpcProjectileCursorAccountState> accounts,
    IReadOnlyList<NpcProjectileReplicationSnapshot> authoritative,
    long currentTick)
  {
    ArgumentNullException.ThrowIfNull(accounts);
    ArgumentNullException.ThrowIfNull(authoritative);
    if (currentTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentTick));
    }

    HashSet<int> retained = new();
    for (int index = 0; index < authoritative.Count; index++)
    {
      NpcProjectileReplicationSnapshot snapshot = authoritative[index];
      if (snapshot.IsActive || snapshot.TombstoneRetainedUntilTick <= 0 ||
          currentTick < snapshot.TombstoneRetainedUntilTick)
      {
        retained.Add(snapshot.ReplicationId);
      }
    }

    List<NpcProjectileCursorAccountState> compacted = new(accounts.Count);
    for (int accountIndex = 0; accountIndex < accounts.Count; accountIndex++)
    {
      NpcProjectileCursorAccountState account = accounts[accountIndex];
      List<NpcProjectileCursorEntry> entries = new();
      for (int entryIndex = 0; entryIndex < account.Entries.Count; entryIndex++)
      {
        NpcProjectileCursorEntry entry = account.Entries[entryIndex];
        if (retained.Contains(entry.ReplicationId))
        {
          entries.Add(entry);
        }
      }

      if (entries.Count > 0)
      {
        compacted.Add(new NpcProjectileCursorAccountState(account.AccountUuid, entries));
      }
    }

    return compacted;
  }

  public void Save(string path, IReadOnlyList<NpcProjectileCursorAccountState> accounts)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    ArgumentNullException.ThrowIfNull(accounts);
    string temporaryPath = path + ".tmp";
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
    using (FileStream output = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
    {
      NpcProjectileCursorPersistenceFormat.Write(output, accounts);
      output.Flush(flushToDisk: true);
    }

    File.Move(temporaryPath, path, overwrite: true);
  }

  public IReadOnlyList<NpcProjectileCursorAccountState> Load(string path)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(path);
    string fullPath = Path.GetFullPath(path);
    string temporaryPath = fullPath + ".tmp";
    if (TryRead(fullPath, out IReadOnlyList<NpcProjectileCursorAccountState>? current))
    {
      if (File.Exists(temporaryPath))
      {
        File.Delete(temporaryPath);
      }

      return current!;
    }

    if (TryRead(temporaryPath, out IReadOnlyList<NpcProjectileCursorAccountState>? recovered))
    {
      PromoteTemporary(temporaryPath, fullPath);
      return recovered!;
    }

    if (!File.Exists(fullPath) && !File.Exists(temporaryPath))
    {
      return Array.Empty<NpcProjectileCursorAccountState>();
    }

    throw new InvalidDataException("NPC projectile cursor sidecar and recovery candidate are invalid.");
  }

  private static bool TryRead(
    string path,
    out IReadOnlyList<NpcProjectileCursorAccountState>? accounts)
  {
    accounts = null;
    if (!File.Exists(path))
    {
      return false;
    }

    try
    {
      using FileStream input = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
      accounts = NpcProjectileCursorPersistenceFormat.Read(input);
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

  private static void PromoteTemporary(string temporaryPath, string fullPath)
  {
    if (File.Exists(fullPath))
    {
      File.Replace(temporaryPath, fullPath, destinationBackupFileName: null);
    }
    else
    {
      File.Move(temporaryPath, fullPath);
    }
  }
}
