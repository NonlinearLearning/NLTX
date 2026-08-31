using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Terraria.Dome.Server.Persistence;

public readonly record struct NpcGivenNameEntry(int ReplicationId, string GivenName);

public static class NpcGivenNamePersistenceFormat
{
  private const int Magic = 0x4E50474E;
  private const int CurrentVersion = 1;
  private const int MaximumEntryCount = 4096;
  private const int MaximumNameLength = 200;

  public static IReadOnlyList<NpcGivenNameEntry> Read(Stream input)
  {
    ArgumentNullException.ThrowIfNull(input);
    using BinaryReader reader = new(input, Encoding.UTF8, leaveOpen: true);
    if (reader.ReadInt32() != Magic || reader.ReadInt32() != CurrentVersion)
    {
      throw new InvalidDataException("NPC given-name sidecar format is unsupported.");
    }

    int count = reader.ReadInt32();
    if (count < 0 || count > MaximumEntryCount)
    {
      throw new InvalidDataException("NPC given-name entry count is invalid.");
    }

    List<NpcGivenNameEntry> entries = new(count);
    HashSet<int> replicationIds = new();
    for (int index = 0; index < count; index++)
    {
      int replicationId = reader.ReadInt32();
      string givenName = reader.ReadString();
      if (replicationId <= 0 || !replicationIds.Add(replicationId) ||
          givenName.Length > MaximumNameLength)
      {
        throw new InvalidDataException("NPC given-name entry is invalid.");
      }

      entries.Add(new NpcGivenNameEntry(replicationId, givenName));
    }

    if (reader.BaseStream.Position != reader.BaseStream.Length)
    {
      throw new InvalidDataException("NPC given-name sidecar contains trailing data.");
    }

    return entries;
  }

  public static void Write(Stream output, IReadOnlyList<NpcGivenNameEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(output);
    ArgumentNullException.ThrowIfNull(entries);
    if (entries.Count > MaximumEntryCount)
    {
      throw new ArgumentOutOfRangeException(nameof(entries));
    }

    using BinaryWriter writer = new(output, Encoding.UTF8, leaveOpen: true);
    writer.Write(Magic);
    writer.Write(CurrentVersion);
    writer.Write(entries.Count);
    HashSet<int> replicationIds = new();
    for (int index = 0; index < entries.Count; index++)
    {
      NpcGivenNameEntry entry = entries[index];
      if (entry.ReplicationId <= 0 || !replicationIds.Add(entry.ReplicationId) ||
          entry.GivenName is null || entry.GivenName.Length > MaximumNameLength)
      {
        throw new ArgumentOutOfRangeException(nameof(entries));
      }

      writer.Write(entry.ReplicationId);
      writer.Write(entry.GivenName);
    }
  }
}
