using System.Collections.Immutable;

namespace Terraria.Content;

public sealed class TileSolidityOverrideState
{
  private readonly Dictionary<int, TileSolidityOverride> _overridesByTileType = new();

  public long Revision { get; private set; }

  public TileOverridePhase OwnerPhase { get; private set; } = TileOverridePhase.Unknown;

  public ImmutableDictionary<int, TileSolidityOverride> ActiveOverrides =>
    _overridesByTileType.ToImmutableDictionary();

  public long? ExpiresAtTick => _overridesByTileType.Count == 0
    ? null
    : _overridesByTileType.Values.Min(static value => value.ExpiresAtTick);

  public void Set(int tileTypeId, bool solid, long expiresAtTick)
  {
    Set(tileTypeId, solid, expiresAtTick, TileOverrideReason.Unknown, TileOverridePhase.Unknown);
  }

  public void Set(
    int tileTypeId,
    bool solid,
    long expiresAtTick,
    TileOverrideReason reason,
    TileOverridePhase ownerPhase)
  {
    if (tileTypeId < 0)
      throw new ArgumentOutOfRangeException(nameof(tileTypeId));

    _overridesByTileType[tileTypeId] = new TileSolidityOverride(
      tileTypeId,
      solid,
      expiresAtTick,
      reason,
      ownerPhase);
    OwnerPhase = ownerPhase;
    Revision++;
  }

  public void RemoveExpired(long currentTick)
  {
    int[] expiredTypeIds = _overridesByTileType
      .Where(pair => pair.Value.ExpiresAtTick <= currentTick)
      .Select(pair => pair.Key)
      .ToArray();
    foreach (int tileTypeId in expiredTypeIds)
    {
      _overridesByTileType.Remove(tileTypeId);
      Revision++;
    }

    if (_overridesByTileType.Count == 0)
      OwnerPhase = TileOverridePhase.Unknown;
  }

  public bool TryGetActive(int tileTypeId, long currentTick, out TileSolidityOverride value)
  {
    if (_overridesByTileType.TryGetValue(tileTypeId, out value) &&
        value.ExpiresAtTick > currentTick)
    {
      return true;
    }

    value = default;
    return false;
  }
}
