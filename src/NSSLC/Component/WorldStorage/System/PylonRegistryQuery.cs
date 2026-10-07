using System;
using System.Collections.Generic;

namespace Terraria.WorldStorage;

// Reads only an already committed registry snapshot. TileEntity discovery and refresh stay outside this query.
public static class PylonRegistryQuery
{
  public static bool HasType(
    IReadOnlyList<PylonRegistryEntry> pylons,
    byte kind)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    if (kind == 0)
    {
      return false;
    }

    for (int index = 0; index < pylons.Count; index++)
    {
      PylonRegistryEntry entry = pylons[index];
      if (entry.IsValid && entry.Kind == kind)
      {
        return true;
      }
    }

    return false;
  }

  public static bool HasTeleportableEntry(
    IReadOnlyList<PylonRegistryEntry> pylons,
    TileCoordinate position)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    for (int index = 0; index < pylons.Count; index++)
    {
      PylonRegistryEntry entry = pylons[index];
      if (entry.CanTeleport && entry.Position == position)
      {
        return true;
      }
    }

    return false;
  }
}
