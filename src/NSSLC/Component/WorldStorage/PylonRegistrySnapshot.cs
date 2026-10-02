using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Terraria.WorldStorage;

public sealed class PylonRegistrySnapshot
{
  public PylonRegistrySnapshot(
    IEnumerable<PylonRegistryEntry> entries,
    uint revision)
  {
    ArgumentNullException.ThrowIfNull(entries);

    PylonRegistryEntry[] orderedEntries = entries
      .OrderBy(entry => entry.Position.X)
      .ThenBy(entry => entry.Position.Y)
      .ThenBy(entry => entry.Kind)
      .ThenBy(entry => entry.TileEntityId.Value)
      .ToArray();
    Entries = new ReadOnlyCollection<PylonRegistryEntry>(orderedEntries);
    Revision = revision;
  }

  public IReadOnlyList<PylonRegistryEntry> Entries { get; }

  public uint Revision { get; }

  public bool HasType(byte kind)
  {
    if (kind == 0)
    {
      return false;
    }

    return Entries.Any(entry => entry.IsValid && entry.Kind == kind);
  }

  public bool HasTeleportableEntry(TileCoordinate position)
  {
    return Entries.Any(entry => entry.CanTeleport && entry.Position == position);
  }
}
