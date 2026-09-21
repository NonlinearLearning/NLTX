using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryChatDiscoveryStateComponent
{
  private readonly HashSet<BestiaryCreditId> _discoveredCredits = new();

  public IReadOnlySet<BestiaryCreditId> DiscoveredCredits =>
    new ReadOnlySet<BestiaryCreditId>(_discoveredCredits);

  public bool Contains(BestiaryCreditId creditId)
  {
    return _discoveredCredits.Contains(creditId);
  }

  internal bool Add(BestiaryCreditId creditId)
  {
    return _discoveredCredits.Add(creditId);
  }

  internal void Restore(BestiaryCreditId creditId)
  {
    _discoveredCredits.Add(creditId);
  }

  internal void Clear()
  {
    _discoveredCredits.Clear();
  }
}
