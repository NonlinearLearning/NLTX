using System.Collections.ObjectModel;

namespace Terraria.WorldGeneration.Dungeon.Catalogs;

public sealed class DungeonObjectStyleCatalog
{
  private readonly IReadOnlyDictionary<DungeonObjectStyleKey, int> _entries;

  public DungeonObjectStyleCatalog(
    IEnumerable<KeyValuePair<DungeonObjectStyleKey, int>> entries)
  {
    ArgumentNullException.ThrowIfNull(entries);
    var dictionary = new Dictionary<DungeonObjectStyleKey, int>();
    foreach ((DungeonObjectStyleKey key, int value) in entries)
    {
      if (value < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(entries));
      }

      if (!dictionary.TryAdd(key, value))
      {
        throw new ArgumentException(
          $"Dungeon object style '{key}' is registered more than once.",
          nameof(entries));
      }
    }

    if (dictionary.Count == 0)
    {
      throw new ArgumentException(
        "At least one dungeon object style is required.",
        nameof(entries));
    }

    _entries = new ReadOnlyDictionary<DungeonObjectStyleKey, int>(dictionary);
  }

  public IReadOnlyDictionary<DungeonObjectStyleKey, int> Entries => _entries;

  public bool TryGet(DungeonObjectStyleKey key, out int style)
  {
    return _entries.TryGetValue(key, out style);
  }

  public static DungeonObjectStyleCatalog CreateDefault()
  {
    return new DungeonObjectStyleCatalog(
    [
      Pair(DungeonObjectStyleKey.DoorWooden, 13),
      Pair(DungeonObjectStyleKey.DoorBlueBrick, 16),
      Pair(DungeonObjectStyleKey.DoorGreenBrick, 17),
      Pair(DungeonObjectStyleKey.DoorPinkBrick, 18),
      Pair(DungeonObjectStyleKey.PotNormal1, 0),
      Pair(DungeonObjectStyleKey.PotNormal2, 1),
      Pair(DungeonObjectStyleKey.PotNormal3, 2),
      Pair(DungeonObjectStyleKey.PotNormal4, 3),
      Pair(DungeonObjectStyleKey.PotSkull1, 10),
      Pair(DungeonObjectStyleKey.PotSkull2, 11),
      Pair(DungeonObjectStyleKey.PotSkull3, 12),
      Pair(DungeonObjectStyleKey.ChandelierBlueBrick, 27),
      Pair(DungeonObjectStyleKey.ChandelierGreenBrick, 28),
      Pair(DungeonObjectStyleKey.ChandelierPinkBrick, 29),
      Pair(DungeonObjectStyleKey.PlatformBlueBrick, 6),
      Pair(DungeonObjectStyleKey.PlatformGreenBrick, 8),
      Pair(DungeonObjectStyleKey.PlatformPinkBrick, 7),
      Pair(DungeonObjectStyleKey.PlatformMetalShelf, 9),
      Pair(DungeonObjectStyleKey.PlatformBrassShelf, 10),
      Pair(DungeonObjectStyleKey.PlatformWoodShelf, 11),
      Pair(DungeonObjectStyleKey.PlatformDungeonShelf, 12)
    ]);
  }

  private static KeyValuePair<DungeonObjectStyleKey, int> Pair(
    DungeonObjectStyleKey key,
    int value)
  {
    return new KeyValuePair<DungeonObjectStyleKey, int>(key, value);
  }
}
