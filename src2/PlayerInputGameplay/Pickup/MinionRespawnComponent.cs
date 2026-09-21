namespace NLTX.PlayerInputGameplay.Pickup;

public readonly record struct MinionSpawnEntry(int ItemType, int ItemPrefix)
{
  public bool Matches(int itemType, int itemPrefix)
  {
    return ItemType == itemType && ItemPrefix == itemPrefix;
  }
}

public sealed class MinionRespawnComponent
{
  private readonly List<MinionSpawnEntry> _minions = new();

  public IReadOnlyList<MinionSpawnEntry> Minions => _minions;

  public void Add(int itemType, int itemPrefix)
  {
    if (itemType < 0 || itemPrefix < 0)
    {
      throw new ArgumentOutOfRangeException(itemType < 0 ? nameof(itemType) : nameof(itemPrefix));
    }

    _minions.Add(new MinionSpawnEntry(itemType, itemPrefix));
  }

  public bool Contains(int itemType, int itemPrefix)
  {
    return _minions.Any(entry => entry.Matches(itemType, itemPrefix));
  }

  public void Clear()
  {
    _minions.Clear();
  }
}
