using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.WorldStorage;

public sealed class PylonRegistryComponent
{
  private List<PylonRegistryEntry> _currentPylons = new();
  private List<PylonRegistryEntry> _previousPylons = new();
  private ReadOnlyCollection<PylonRegistryEntry> _currentPylonsView;
  private ReadOnlyCollection<PylonRegistryEntry> _previousPylonsView;
  private int _refreshCooldownTicksRemaining;

  public PylonRegistryComponent()
  {
    _currentPylonsView = _currentPylons.AsReadOnly();
    _previousPylonsView = _previousPylons.AsReadOnly();
  }

  public IReadOnlyList<PylonRegistryEntry> CurrentPylons => _currentPylonsView;

  public IReadOnlyList<PylonRegistryEntry> PreviousPylons => _previousPylonsView;

  public int Count => _currentPylons.Count;

  public bool HasPendingRefresh => RefreshCooldownTicksRemaining == 0;

  public int RefreshCooldownTicksRemaining
  {
    get => _refreshCooldownTicksRemaining;
    internal set
    {
      if (value < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      _refreshCooldownTicksRemaining = value;
    }
  }

  public uint Revision { get; internal set; }

  public void Replace(
    IReadOnlyList<PylonRegistryEntry> pylons,
    int refreshCooldownTicks)
  {
    ArgumentNullException.ThrowIfNull(pylons);
    if (refreshCooldownTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(refreshCooldownTicks));
    }

    if (Revision == uint.MaxValue)
    {
      throw new InvalidOperationException(
        "The pylon registry revision is exhausted.");
    }

    List<PylonRegistryEntry> replacementPylons = new(pylons);
    _previousPylons = _currentPylons;
    _previousPylonsView = _previousPylons.AsReadOnly();
    _currentPylons = replacementPylons;
    _currentPylonsView = _currentPylons.AsReadOnly();
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

  public void Reset()
  {
    _currentPylons = new List<PylonRegistryEntry>();
    _previousPylons = new List<PylonRegistryEntry>();
    _currentPylonsView = _currentPylons.AsReadOnly();
    _previousPylonsView = _previousPylons.AsReadOnly();
    RefreshCooldownTicksRemaining = 0;
    Revision = 0;
  }
}
