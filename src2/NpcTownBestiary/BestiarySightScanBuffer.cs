namespace Terraria.NpcTownBestiary;

public sealed class BestiarySightScanBuffer
{
  private readonly HashSet<NpcNetId> _seenNpcNetIds = new();
  private readonly List<BestiaryPlayerBounds> _playerBounds = new();

  public IReadOnlyCollection<NpcNetId> SeenNpcNetIds => _seenNpcNetIds;

  public IReadOnlyCollection<BestiaryPlayerBounds> PlayerBounds => _playerBounds;

  public void ReplacePlayerBounds(IEnumerable<BestiaryPlayerBounds> bounds)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    _playerBounds.Clear();
    _playerBounds.AddRange(bounds);
  }

  public void BeginScan(IEnumerable<BestiaryPlayerBounds> bounds)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    Reset();
    ReplacePlayerBounds(bounds);
  }

  public bool TryMarkSeen(NpcNetId npcNetId)
  {
    return _seenNpcNetIds.Add(npcNetId);
  }

  public void Reset()
  {
    _seenNpcNetIds.Clear();
    _playerBounds.Clear();
  }
}
