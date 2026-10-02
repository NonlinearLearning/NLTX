using System.Collections.Generic;

namespace Terraria.WorldStorage;

public readonly record struct PylonRegistryRefreshResult(
  bool Accepted,
  bool Changed,
  bool RejectedAsStale,
  bool RejectedByCooldown,
  uint PreviousRevision,
  uint CurrentRevision,
  IReadOnlyList<PylonRegistryEntry> Added,
  IReadOnlyList<PylonRegistryEntry> Removed)
{
  public static PylonRegistryRefreshResult Stale(uint revision)
  {
    return new PylonRegistryRefreshResult(
      Accepted: false,
      Changed: false,
      RejectedAsStale: true,
      RejectedByCooldown: false,
      PreviousRevision: revision,
      CurrentRevision: revision,
      Added: [],
      Removed: []);
  }

  public static PylonRegistryRefreshResult Cooldown(uint revision)
  {
    return new PylonRegistryRefreshResult(
      Accepted: false,
      Changed: false,
      RejectedAsStale: false,
      RejectedByCooldown: true,
      PreviousRevision: revision,
      CurrentRevision: revision,
      Added: [],
      Removed: []);
  }
}
