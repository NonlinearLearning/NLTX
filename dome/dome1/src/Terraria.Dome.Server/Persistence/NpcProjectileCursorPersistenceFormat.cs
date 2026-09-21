using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria.Dome.Server.Replication;

namespace Terraria.Dome.Server.Persistence;

public readonly record struct NpcProjectileCursorAccountState(
  string AccountUuid,
  IReadOnlyList<NpcProjectileCursorEntry> Entries);

public static class NpcProjectileCursorPersistenceFormat
{
  private const int Magic = 0x4E504356;
  private const int CurrentVersion = 1;
  private const int MaximumAccountCount = 1024;
  private const int MaximumEntryCount = 8192;
  private const int MaximumUuidLength = 64;

  public static IReadOnlyList<NpcProjectileCursorAccountState> Read(Stream input)
  {
    ArgumentNullException.ThrowIfNull(input);
    using BinaryReader reader = new(input, Encoding.UTF8, leaveOpen: true);
    if (reader.ReadInt32() != Magic || reader.ReadInt32() != CurrentVersion)
    {
      throw new InvalidDataException("NPC projectile cursor sidecar format is unsupported.");
    }

    int accountCount = reader.ReadInt32();
    if (accountCount < 0 || accountCount > MaximumAccountCount)
    {
      throw new InvalidDataException("NPC projectile cursor account count is invalid.");
    }

    List<NpcProjectileCursorAccountState> accounts = new(accountCount);
    HashSet<string> accountIds = new(StringComparer.Ordinal);
    for (int accountIndex = 0; accountIndex < accountCount; accountIndex++)
    {
      string accountUuid = reader.ReadString();
      if (!Guid.TryParseExact(accountUuid, "D", out _) ||
          accountUuid.Length > MaximumUuidLength || !accountIds.Add(accountUuid))
      {
        throw new InvalidDataException("NPC projectile cursor account UUID is invalid.");
      }

      int entryCount = reader.ReadInt32();
      if (entryCount < 0 || entryCount > MaximumEntryCount)
      {
        throw new InvalidDataException("NPC projectile cursor entry count is invalid.");
      }

      List<NpcProjectileCursorEntry> entries = new(entryCount);
      HashSet<int> replicationIds = new();
      for (int entryIndex = 0; entryIndex < entryCount; entryIndex++)
      {
        int replicationId = reader.ReadInt32();
        long revision = reader.ReadInt64();
        if (replicationId <= 0 || revision < 0 || !replicationIds.Add(replicationId))
        {
          throw new InvalidDataException("NPC projectile cursor entry is invalid.");
        }

        entries.Add(new NpcProjectileCursorEntry(replicationId, revision));
      }

      accounts.Add(new NpcProjectileCursorAccountState(accountUuid, entries));
    }

    if (reader.BaseStream.Position != reader.BaseStream.Length)
    {
      throw new InvalidDataException("NPC projectile cursor sidecar contains trailing data.");
    }

    return accounts;
  }

  public static void Write(
    Stream output,
    IReadOnlyList<NpcProjectileCursorAccountState> accounts)
  {
    ArgumentNullException.ThrowIfNull(output);
    ArgumentNullException.ThrowIfNull(accounts);
    if (accounts.Count > MaximumAccountCount)
    {
      throw new ArgumentOutOfRangeException(nameof(accounts));
    }

    using BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true);
    writer.Write(Magic);
    writer.Write(CurrentVersion);
    writer.Write(accounts.Count);
    HashSet<string> accountIds = new(StringComparer.Ordinal);
    for (int accountIndex = 0; accountIndex < accounts.Count; accountIndex++)
    {
      NpcProjectileCursorAccountState account = accounts[accountIndex];
      if (!Guid.TryParseExact(account.AccountUuid, "D", out _) ||
          account.AccountUuid.Length > MaximumUuidLength || !accountIds.Add(account.AccountUuid) ||
          account.Entries.Count > MaximumEntryCount)
      {
        throw new ArgumentOutOfRangeException(nameof(accounts));
      }

      writer.Write(account.AccountUuid);
      writer.Write(account.Entries.Count);
      HashSet<int> replicationIds = new();
      for (int entryIndex = 0; entryIndex < account.Entries.Count; entryIndex++)
      {
        NpcProjectileCursorEntry entry = account.Entries[entryIndex];
        if (entry.ReplicationId <= 0 || entry.Revision < 0 || !replicationIds.Add(entry.ReplicationId))
        {
          throw new ArgumentOutOfRangeException(nameof(accounts));
        }

        writer.Write(entry.ReplicationId);
        writer.Write(entry.Revision);
      }
    }
  }
}
