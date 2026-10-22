namespace MinimalImplementation.Server.World;

public sealed partial class LiquidSimulation {
  // Called under the WorldOwner gate after an appearance-only change. Pending panic tiles
  // contain full tile values; preserve their liquid work while rebasing the newer appearance.
  internal void RebasePanicTileAppearance(int index, WorldTile current) {
    if (_panicSettle is not null &&
        _panicSettle.Updates.TryGetValue(index, out WorldTile pending)) {
      _panicSettle.Updates[index] = pending with {
        TileColor = current.TileColor,
        WallColor = current.WallColor,
        TileHeader3 = current.TileHeader3,
        WallHeader = current.WallHeader
      };
    }
  }

  internal LiquidSimulation Clone() {
    var clone = new LiquidSimulation(_options) {
      _queueCapacityExceeded = _queueCapacityExceeded,
      _quickFall = _quickFall,
      _quickSettle = _quickSettle,
      _stuck = _stuck,
      _stuckAmount = _stuckAmount,
      _stuckCount = _stuckCount,
      _panicMode = _panicMode,
      _panicCounter = _panicCounter,
      _panicX = _panicX,
      _panicY = _panicY
    };
    foreach (TileCoordinate coordinate in _active) {
      clone._active.Enqueue(coordinate);
    }
    foreach (TileCoordinate coordinate in _buffered) {
      clone._buffered.Enqueue(coordinate);
    }
    clone._queued.UnionWith(_queued);
    foreach (var pair in _delays) {
      clone._delays.Add(pair.Key, pair.Value);
    }
    clone._panicSettle = _panicSettle?.Clone();
    clone._panicRebuildIndex = _panicRebuildIndex;
    return clone;
  }

  // A queue step can alter delays and recovery cursors without changing the world version.
  internal bool HasSameState(LiquidSimulation other) {
    return ReferenceEquals(_options, other._options) &&
        _queueCapacityExceeded == other._queueCapacityExceeded &&
        _quickFall == other._quickFall && _quickSettle == other._quickSettle &&
        _stuck == other._stuck && _stuckAmount == other._stuckAmount &&
        _stuckCount == other._stuckCount && _panicMode == other._panicMode &&
        _panicCounter == other._panicCounter && _panicX == other._panicX &&
        _panicY == other._panicY && _panicRebuildIndex == other._panicRebuildIndex &&
        (_panicSettle is null
            ? other._panicSettle is null
            : other._panicSettle is not null && _panicSettle.HasSameState(other._panicSettle)) &&
        _active.SequenceEqual(other._active) &&
        _buffered.SequenceEqual(other._buffered) && _queued.SetEquals(other._queued) &&
        _delays.Count == other._delays.Count && _delays.All(pair =>
            other._delays.TryGetValue(pair.Key, out int delay) && delay == pair.Value);
  }
}
