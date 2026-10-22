namespace MinimalImplementation.Server.World;

public sealed partial class LiquidSimulation {
  private enum PanicSettlePhase {
    Fall,
    Scan,
    Publish
  }

  private sealed class PanicSettleState {
    public TileCoordinate Origin;
    public int X;
    public int Y;
    public int Remaining;
    public LiquidType Type;
    public bool FirstLayer = true;
    public PanicSettlePhase Phase;
    public int Direction = -1;
    public int Distance;
    public int LastEmptyDirection = -1;
    public int LastEmptyDistance;
    public bool LeftBlocked;
    public bool RightBlocked;
    public Dictionary<int, WorldTile> Updates = [];

    public PanicSettleState Clone() {
      var clone = (PanicSettleState)MemberwiseClone();
      clone.Updates = new Dictionary<int, WorldTile>(Updates);
      return clone;
    }

    public bool HasSameState(PanicSettleState other) {
      return Origin == other.Origin && X == other.X && Y == other.Y &&
          Remaining == other.Remaining && Type == other.Type && FirstLayer == other.FirstLayer &&
          Phase == other.Phase && Direction == other.Direction && Distance == other.Distance &&
          LastEmptyDirection == other.LastEmptyDirection &&
          LastEmptyDistance == other.LastEmptyDistance && LeftBlocked == other.LeftBlocked &&
          RightBlocked == other.RightBlocked && Updates.Count == other.Updates.Count &&
          Updates.All(pair => other.Updates.TryGetValue(pair.Key, out WorldTile tile) &&
              tile == pair.Value);
    }
  }

  private PanicSettleState? _panicSettle;
  private int _panicRebuildIndex = -1;

  // The scan cursor still points at the unpublished source, so discarding its private overlay
  // restarts that source without restoring old tiles over a newer external mutation.
  internal void InvalidatePanicContinuation() {
    _panicSettle = null;
    if (_panicRebuildIndex >= 0) {
      _panicRebuildIndex = 0;
    }
  }

  private void AdvancePanicWork(
      IReadOnlyList<WorldTile> tiles, Dictionary<int, WorldTile> stagedTiles,
      WorldGeometry geometry, WorldFacts facts, ISet<TileCoordinate> changed,
      ICollection<LiquidMergeAction> merges) {
    if (_panicRebuildIndex >= 0) {
      int index = _panicRebuildIndex++;
      if (ReadTile(tiles, stagedTiles, index).LiquidAmount != 0) {
        EnqueueAround(GetCoordinate(index, geometry), geometry);
      }
      if (_panicRebuildIndex == tiles.Count) {
        _panicRebuildIndex = -1;
        _panicMode = false;
        _panicCounter = 0;
        _quickSettle = false;
      }
      return;
    }

    if (_panicY < 3 || geometry.Width < 9 || geometry.Height < 6) {
      BeginPanicQueueRebuild();
      return;
    }

    if (_panicSettle is null) {
      var origin = new TileCoordinate(_panicX, _panicY);
      int index = GetIndex(origin, geometry);
      WorldTile source = ReadTile(tiles, stagedTiles, index);
      if (source.LiquidAmount == 0 || (source.Active && source.Type == 379)) {
        AdvancePanicCursor(geometry);
        return;
      }
      _panicSettle = new PanicSettleState {
        Origin = origin,
        X = origin.X,
        Y = origin.Y,
        Remaining = source.LiquidAmount,
        Type = source.LiquidType
      };
      _panicSettle.Updates.Add(index, source.WithLiquid(0, source.LiquidType));
      return;
    }

    PanicSettleState state = _panicSettle;
    if (state.Phase == PanicSettlePhase.Publish) {
      int destinationIndex = state.Y * geometry.Width + state.X;
      WorldTile destination = ReadPanicTile(tiles, stagedTiles, state, destinationIndex);
      state.Updates[destinationIndex] = destination.WithLiquid((byte)state.Remaining, state.Type);
      // A source contains at most 255 units, hence at most 255 partial fills plus its two
      // endpoints. No intermediate removal or fill is visible across a budget boundary.
      foreach (var pair in state.Updates) {
        SetTile(tiles, stagedTiles, geometry, GetCoordinate(pair.Key, geometry), pair.Value, changed);
      }
      if (state.Remaining > 0) {
        var destinationCoordinate = new TileCoordinate(state.X, state.Y);
        AttemptPanicMerge(tiles, stagedTiles, geometry, facts, destinationCoordinate,
            state.Type, LiquidType.Lava, changed, merges);
        AttemptPanicMerge(tiles, stagedTiles, geometry, facts, destinationCoordinate,
            state.Type, LiquidType.Honey, changed, merges);
        AttemptPanicMerge(tiles, stagedTiles, geometry, facts, destinationCoordinate,
            state.Type, LiquidType.Shimmer, changed, merges);
      }
      _panicSettle = null;
      AdvancePanicCursor(geometry);
      return;
    }

    if (state.Phase == PanicSettlePhase.Fall) {
      int belowIndex = (state.Y + 1) * geometry.Width + state.X;
      WorldTile below = ReadPanicTile(tiles, stagedTiles, state, belowIndex);
      if (state.Y < geometry.Height - 5 && below.LiquidAmount == 0 &&
          !BlocksPanicLiquid(below, facts)) {
        state.Y++;
        state.FirstLayer = false;
        return;
      }
      state.Direction = -1;
      state.Distance = 0;
      state.LastEmptyDirection = -1;
      state.LastEmptyDistance = 0;
      state.LeftBlocked = false;
      state.RightBlocked = false;
      state.Phase = PanicSettlePhase.Scan;
      return;
    }

    int scanX = state.X + state.Distance * state.Direction;
    if ((uint)scanX >= (uint)geometry.Width) {
      FinishPanicLayer(state, hasOutlet: false);
      return;
    }

    int scanIndex = state.Y * geometry.Width + scanX;
    if (ReadPanicTile(tiles, stagedTiles, state, scanIndex).LiquidAmount == 0) {
      state.LastEmptyDirection = state.Direction;
      state.LastEmptyDistance = state.Distance;
    }
    if (state.Direction == -1 && scanX < 5) {
      state.LeftBlocked = true;
    } else if (state.Direction == 1 && scanX > geometry.Width - 5) {
      state.RightBlocked = true;
    }

    int targetIndex = (state.Y + 1) * geometry.Width + scanX;
    WorldTile target = ReadPanicTile(tiles, stagedTiles, state, targetIndex);
    if (target.LiquidAmount is > 0 and < byte.MaxValue && target.LiquidType == state.Type) {
      int amount = Math.Min(byte.MaxValue - target.LiquidAmount, state.Remaining);
      target = target.WithLiquid((byte)(target.LiquidAmount + amount), state.Type);
      state.Updates[targetIndex] = target;
      state.Remaining -= amount;
      if (state.Remaining == 0) {
        FinishPanicLayer(state, hasOutlet: false);
        return;
      }
    }
    if (state.Y < geometry.Height - 5 && target.LiquidAmount == 0 &&
        !BlocksPanicLiquid(target, facts)) {
      FinishPanicLayer(state, hasOutlet: true);
      return;
    }

    int nextX = state.X + (state.Distance + 1) * state.Direction;
    bool nextBlocked = (uint)nextX >= (uint)geometry.Width;
    if (!nextBlocked) {
      WorldTile next = ReadPanicTile(
          tiles, stagedTiles, state, state.Y * geometry.Width + nextX);
      nextBlocked = (next.LiquidAmount != 0 &&
          (!state.FirstLayer || state.Direction != 1)) || BlocksPanicLiquid(next, facts);
    }
    if (nextBlocked) {
      if (state.Direction == 1) {
        state.RightBlocked = true;
      } else {
        state.LeftBlocked = true;
      }
    }
    if (state.LeftBlocked && state.RightBlocked) {
      FinishPanicLayer(state, hasOutlet: false);
    } else if (state.RightBlocked) {
      state.Direction = -1;
      state.Distance++;
    } else if (state.LeftBlocked) {
      if (state.Direction == 1) {
        state.Distance++;
      }
      state.Direction = 1;
    } else {
      if (state.Direction == 1) {
        state.Distance++;
      }
      state.Direction = -state.Direction;
    }
  }

  private static void FinishPanicLayer(PanicSettleState state, bool hasOutlet) {
    state.X += state.LastEmptyDistance * state.LastEmptyDirection;
    if (state.Remaining == 0 || !hasOutlet) {
      state.Phase = PanicSettlePhase.Publish;
    } else {
      state.Y++;
      state.Phase = PanicSettlePhase.Fall;
    }
  }

  private void AttemptPanicMerge(
      IReadOnlyList<WorldTile> tiles, Dictionary<int, WorldTile> stagedTiles,
      WorldGeometry geometry, WorldFacts facts, TileCoordinate destination,
      LiquidType originalType, LiquidType checkedType, ISet<TileCoordinate> changed,
      ICollection<LiquidMergeAction> merges) {
    bool originalHasType = originalType == checkedType;
    foreach (TileCoordinate neighbor in GetNeighbors(destination, geometry)) {
      WorldTile tile = ReadTile(tiles, stagedTiles, GetIndex(neighbor, geometry));
      if (tile.LiquidAmount == 0 || (tile.LiquidType == checkedType) == originalHasType) {
        continue;
      }
      TileCoordinate coordinate = originalHasType ? destination : neighbor;
      WorldTile source = ReadTile(tiles, stagedTiles, GetIndex(coordinate, geometry));
      // LiquidCheck rejects SolidTile, unlike the flow scan's nactive/solid predicate.
      if (!BlocksPanicLiquid(source, facts) || source.HalfBlock || source.Slope != 0) {
        CheckLiquidMerge(tiles, stagedTiles, geometry, facts, coordinate, source, changed, merges,
            checkedType);
      }
      return;
    }
  }

  private void AdvancePanicCursor(WorldGeometry geometry) {
    _panicX++;
    if (_panicX > geometry.Width - 5) {
      _panicX = 4;
      _panicY--;
    }
  }

  private void BeginPanicQueueRebuild() {
    _active.Clear();
    _buffered.Clear();
    _queued.Clear();
    _delays.Clear();
    _panicSettle = null;
    _panicRebuildIndex = 0;
  }

  private static bool BlocksPanicLiquid(WorldTile tile, WorldFacts facts) {
    if (!tile.Active || tile.Inactive ||
        tile.Type is 138 or 484 or 546 or 664 or 711 or 712 or 713 or 714 or 715 or 716) {
      return false;
    }
    return tile.Type == 379 ||
        (tile.Type < facts.Solid.Count && facts.Solid[tile.Type] &&
         !facts.SolidTop[tile.Type]);
  }

  private static WorldTile ReadPanicTile(
      IReadOnlyList<WorldTile> tiles, IReadOnlyDictionary<int, WorldTile> stagedTiles,
      PanicSettleState state, int index) {
    return state.Updates.TryGetValue(index, out WorldTile tile)
        ? tile : ReadTile(tiles, stagedTiles, index);
  }
}
