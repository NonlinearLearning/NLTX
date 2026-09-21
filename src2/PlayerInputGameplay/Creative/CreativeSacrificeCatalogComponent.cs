namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativeSacrificeCatalogComponent
{
  private readonly Dictionary<int, int> _requiredByItemId = new();

  public void SetRequiredCount(int itemId, int count)
  {
    if (itemId < 0 || count < 0)
    {
      throw new ArgumentOutOfRangeException(itemId < 0 ? nameof(itemId) : nameof(count));
    }

    _requiredByItemId[itemId] = count;
  }

  public int GetRequiredCount(int itemId)
  {
    return _requiredByItemId.TryGetValue(itemId, out var count) ? count : 0;
  }

  public IReadOnlyDictionary<int, int> Snapshot()
  {
    return new Dictionary<int, int>(_requiredByItemId);
  }
}
