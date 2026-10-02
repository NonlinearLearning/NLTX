using System.Collections.ObjectModel;

namespace Terraria.NpcTownBestiary;

public sealed class BestiaryKillCountStateComponent
{
  private readonly Dictionary<BestiaryCreditId, int> _countsByCredit = new();

  public IReadOnlyDictionary<BestiaryCreditId, int> CountsByCredit =>
    new ReadOnlyDictionary<BestiaryCreditId, int>(_countsByCredit);

  public int GetCount(BestiaryCreditId creditId)
  {
    return _countsByCredit.TryGetValue(creditId, out int count) ? count : 0;
  }

  internal int Add(BestiaryCreditId creditId, int amount)
  {
    int current = GetCount(creditId);
    long candidate = (long)current + amount;
    int next = candidate <= 0
      ? 0
      : candidate >= BestiaryKillCountPolicy.PositiveCap
        ? BestiaryKillCountPolicy.PositiveCap
        : (int)candidate;
    _countsByCredit[creditId] = next;
    return next;
  }

  internal bool SetDirect(BestiaryCreditId creditId, int count)
  {
    int next = BestiaryKillCountPolicy.Clamp(count);
    int previous = GetCount(creditId);
    _countsByCredit[creditId] = next;
    return previous != next;
  }

  internal void Restore(BestiaryCreditId creditId, int count)
  {
    _countsByCredit[creditId] = BestiaryKillCountPolicy.Clamp(count);
  }

  internal void Clear()
  {
    _countsByCredit.Clear();
  }
}
