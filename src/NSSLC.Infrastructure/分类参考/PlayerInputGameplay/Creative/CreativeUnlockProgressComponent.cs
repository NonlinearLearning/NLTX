namespace NLTX.PlayerInputGameplay.Creative;

public sealed class CreativeUnlockProgressComponent
{
  public const int PositiveSacrificeCountCap = 999999;

  private readonly Dictionary<string, int> _sacrificeCountByPersistentId = new(StringComparer.Ordinal);
  private readonly Dictionary<int, int> _sacrificeCountByItemId = new();
  private readonly Dictionary<int, string> _unlockedByTeammate = new();
  private readonly HashSet<int> _newlyUnlocked = new();

  public bool AnyNewUnlocksFromTeammates { get; private set; }

  public int LastEditId { get; private set; }

  public int RecordSacrifice(string itemPersistentId, int itemId, int amount, int requiredCount)
  {
    if (string.IsNullOrWhiteSpace(itemPersistentId))
    {
      throw new ArgumentException("A persistent item ID is required.", nameof(itemPersistentId));
    }

    if (itemId < 0 || amount < 0 || requiredCount < 0)
    {
      throw new ArgumentOutOfRangeException(itemId < 0 ? nameof(itemId) : nameof(amount));
    }

    _sacrificeCountByPersistentId.TryGetValue(itemPersistentId, out var oldCount);
    var count = Math.Min(PositiveSacrificeCountCap, checked(oldCount + amount));
    _sacrificeCountByPersistentId[itemPersistentId] = count;
    _sacrificeCountByItemId[itemId] = count;
    if (requiredCount > 0 && count >= requiredCount)
    {
      _newlyUnlocked.Add(itemId);
    }

    LastEditId = checked(LastEditId + 1);
    return count;
  }

  public int GetSacrificeCount(string itemPersistentId)
  {
    ArgumentNullException.ThrowIfNull(itemPersistentId);
    return _sacrificeCountByPersistentId.TryGetValue(itemPersistentId, out var count) ? count : 0;
  }

  public void RecordTeammateUnlock(int itemId, string teammatePersistentId)
  {
    if (itemId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemId));
    }

    if (string.IsNullOrWhiteSpace(teammatePersistentId))
    {
      throw new ArgumentException("A teammate ID is required.", nameof(teammatePersistentId));
    }

    _unlockedByTeammate[itemId] = teammatePersistentId;
    AnyNewUnlocksFromTeammates = true;
    LastEditId = checked(LastEditId + 1);
  }

  public IReadOnlyList<int> DrainNewlyUnlocked()
  {
    var result = _newlyUnlocked.OrderBy(static itemId => itemId).ToArray();
    _newlyUnlocked.Clear();
    return result;
  }

  public IReadOnlyDictionary<int, string> TeammateUnlocks => new Dictionary<int, string>(_unlockedByTeammate);

  public void ClearTeammateNotification()
  {
    AnyNewUnlocksFromTeammates = false;
  }
}
