using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.Teleportation;

public sealed class PylonRegistryComponent
{
  private readonly List<PylonRegistryEntry> _currentPylons = new();
  private readonly List<PylonRegistryEntry> _previousPylons = new();

  public IReadOnlyList<PylonRegistryEntry> CurrentPylons => _currentPylons.AsReadOnly();
  public IReadOnlyList<PylonRegistryEntry> PreviousPylons => _previousPylons.AsReadOnly();
  public int RefreshCooldownTicksRemaining { get; private set; }
  public uint Revision { get; private set; }
  public int Count => _currentPylons.Count;
  public bool HasPendingRefresh => RefreshCooldownTicksRemaining == 0;

  public void Replace(IReadOnlyList<PylonRegistryEntry> pylons, int refreshCooldownTicks)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    if (refreshCooldownTicks < 0 || Revision == uint.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(refreshCooldownTicks));
    }

    _previousPylons.Clear();
    _previousPylons.AddRange(_currentPylons);
    _currentPylons.Clear();
    _currentPylons.AddRange(pylons);
    RefreshCooldownTicksRemaining = refreshCooldownTicks;
    Revision++;
  }

  public void AdvanceTick()
  {
    if (RefreshCooldownTicksRemaining > 0)
    {
      RefreshCooldownTicksRemaining--;
    }
  }

  public void Clear()
  {
    _currentPylons.Clear();
    _previousPylons.Clear();
    RefreshCooldownTicksRemaining = 0;
    Revision = 0;
  }
}
