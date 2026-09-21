using System;
using System.Collections.Generic;

namespace Terraria.Npc.Queries;

public static class NpcActivePresenceScanSystem
{
  public static void Rebuild(
    NpcActivePresenceCache cache,
    long scanRevision,
    IReadOnlyList<NpcActivePresenceScanEntry> entries)
  {
    ArgumentNullException.ThrowIfNull(cache);
    ArgumentNullException.ThrowIfNull(entries);

    cache.BeginScan(scanRevision);
    for (var index = 0; index < entries.Count; index++)
    {
      NpcActivePresenceScanEntry entry = entries[index];
      if (!entry.IsActive)
      {
        continue;
      }

      cache.TryMarkActive(scanRevision, entry.NpcType);
    }
  }
}
