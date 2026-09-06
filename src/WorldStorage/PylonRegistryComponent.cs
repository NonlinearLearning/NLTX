using System.Collections.ObjectModel;

namespace Terraria.WorldStorage;

public sealed class PylonRegistryComponent
{
  private readonly List<PylonRegistryEntry> _currentPylons = new();
  private readonly List<PylonRegistryEntry> _previousPylons = new();
  private readonly ReadOnlyCollection<PylonRegistryEntry> _currentPylonsView;
  private readonly ReadOnlyCollection<PylonRegistryEntry> _previousPylonsView;

  public PylonRegistryComponent()
  {
    _currentPylonsView = _currentPylons.AsReadOnly();
    _previousPylonsView = _previousPylons.AsReadOnly();
  }

  public IReadOnlyList<PylonRegistryEntry> CurrentPylons => _currentPylonsView;

  public IReadOnlyList<PylonRegistryEntry> PreviousPylons => _previousPylonsView;

  public int Count => _currentPylons.Count;

  public bool HasPendingRefresh => RefreshCooldownTicksRemaining == 0;

  public int RefreshCooldownTicksRemaining { get; internal set; }

  public uint Revision { get; internal set; }
}
