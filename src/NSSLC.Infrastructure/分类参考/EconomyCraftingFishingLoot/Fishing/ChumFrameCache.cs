namespace NLTX.EconomyCraftingFishingLoot.Fishing;

public sealed class ChumFrameCache
{
  private Dictionary<(int X, int Y), int> _pendingCounts = [];

  private Dictionary<(int X, int Y), int> _previousCounts = [];

  public void AddPending(int x, int y, int count)
  {
    if (count < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(count));
    }

    (int X, int Y) key = (x, y);
    _pendingCounts.TryGetValue(key, out int existingCount);
    _pendingCounts[key] = checked(existingCount + count);
  }

  public bool TryGetPrevious(int x, int y, out int count)
  {
    return _previousCounts.TryGetValue((x, y), out count);
  }

  public void AdvanceFrame()
  {
    Dictionary<(int X, int Y), int> previousCounts = _previousCounts;
    _previousCounts = _pendingCounts;
    _pendingCounts = previousCounts;
    _pendingCounts.Clear();
  }
}
