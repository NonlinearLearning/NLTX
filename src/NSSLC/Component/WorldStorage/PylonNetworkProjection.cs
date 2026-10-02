using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

// Builds immutable network intents; transport and client registry ownership stay external.
public static class PylonNetworkProjection
{
  public static IReadOnlyList<PylonProjectionMessage> BuildRefreshMessages(
    in PylonRegistryRefreshResult refresh)
  {
    if (!refresh.Accepted || refresh.CurrentRevision == 0)
    {
      return Array.Empty<PylonProjectionMessage>();
    }

    List<PylonProjectionMessage> messages = new(
      refresh.Added.Count + refresh.Removed.Count);
    AddMessages(
      messages,
      refresh.CurrentRevision,
      PylonProjectionMessageKind.Added,
      refresh.Added);
    AddMessages(
      messages,
      refresh.CurrentRevision,
      PylonProjectionMessageKind.Removed,
      refresh.Removed);
    return messages.AsReadOnly();
  }

  public static IReadOnlyList<PylonProjectionMessage> BuildJoinMessages(
    PylonRegistrySnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    List<PylonProjectionMessage> messages = new(snapshot.Entries.Count);
    for (int index = 0; index < snapshot.Entries.Count; index++)
    {
      PylonRegistryEntry entry = snapshot.Entries[index];
      if (!entry.CanTeleport)
      {
        continue;
      }

      messages.Add(new PylonProjectionMessage(
        snapshot.Revision,
        PylonProjectionMessageKind.Added,
        entry));
    }

    return messages.AsReadOnly();
  }

  private static void AddMessages(
    List<PylonProjectionMessage> messages,
    uint revision,
    PylonProjectionMessageKind kind,
    IReadOnlyList<PylonRegistryEntry> entries)
  {
    for (int index = 0; index < entries.Count; index++)
    {
      PylonRegistryEntry entry = entries[index];
      if (!entry.CanTeleport)
      {
        continue;
      }

      messages.Add(new PylonProjectionMessage(revision, kind, entry));
    }
  }
}
