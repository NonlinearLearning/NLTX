using System;
using System.Collections.Generic;
using System.Linq;

namespace Terraria.WorldStorage;

// Commits a discovered pylon fact set. TileEntity scanning and network effects stay outside.
public sealed class PylonRegistrySystem
{
  private readonly PylonRegistryComponent _registry;

  public PylonRegistrySystem(PylonRegistryComponent registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public PylonRegistrySnapshot CurrentSnapshot =>
    new(_registry.CurrentPylons, _registry.Revision);

  public PylonRegistrySnapshot PreviousSnapshot =>
    new(_registry.PreviousPylons, _registry.Revision == 0 ? 0 : _registry.Revision - 1);

  public bool HasPendingRefresh => _registry.HasPendingRefresh;

  public void AdvanceTick()
  {
    _registry.AdvanceTick();
  }

  public void RequestImmediateRefresh()
  {
    _registry.RefreshCooldownTicksRemaining = 0;
  }

  public PylonRegistryRefreshResult Refresh(
    IReadOnlyList<PylonRegistryEntry> discovered,
    int refreshCooldownTicks,
    uint expectedRevision)
  {
    ArgumentNullException.ThrowIfNull(discovered);
    if (refreshCooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(refreshCooldownTicks));
    }

    uint currentRevision = _registry.Revision;
    if (expectedRevision != currentRevision)
    {
      return PylonRegistryRefreshResult.Stale(currentRevision);
    }

    if (!_registry.HasPendingRefresh)
    {
      return PylonRegistryRefreshResult.Cooldown(currentRevision);
    }

    PylonRegistryEntry[] previous = _registry.CurrentPylons.ToArray();
    _registry.Replace(discovered, refreshCooldownTicks);
    PylonRegistryEntry[] current = _registry.CurrentPylons.ToArray();

    HashSet<PylonRegistryEntry> previousSet = previous.ToHashSet();
    HashSet<PylonRegistryEntry> currentSet = current.ToHashSet();
    PylonRegistryEntry[] added = currentSet
      .Except(previousSet)
      .OrderBy(entry => entry.Position.X)
      .ThenBy(entry => entry.Position.Y)
      .ThenBy(entry => entry.Kind)
      .ThenBy(entry => entry.TileEntityId.Value)
      .ToArray();
    PylonRegistryEntry[] removed = previousSet
      .Except(currentSet)
      .OrderBy(entry => entry.Position.X)
      .ThenBy(entry => entry.Position.Y)
      .ThenBy(entry => entry.Kind)
      .ThenBy(entry => entry.TileEntityId.Value)
      .ToArray();

    return new PylonRegistryRefreshResult(
      Accepted: true,
      Changed: added.Length != 0 || removed.Length != 0,
      RejectedAsStale: false,
      RejectedByCooldown: false,
      PreviousRevision: currentRevision,
      CurrentRevision: _registry.Revision,
      Added: added,
      Removed: removed);
  }

  public void Reset()
  {
    _registry.Reset();
  }
}
