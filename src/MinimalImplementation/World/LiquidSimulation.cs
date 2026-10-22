namespace MinimalImplementation.Server.World;

public readonly record struct LiquidUpdate(TileCoordinate Coordinate, byte Amount, LiquidType Type);

public sealed class LiquidStepResult
{
  private readonly IReadOnlyDictionary<int, WorldTile> _tileUpdates;

  public LiquidStepResult(
    IReadOnlyDictionary<int, WorldTile> tileUpdates,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    bool queueCapacityExceeded,
    int operationsUsed)
  {
    ArgumentNullException.ThrowIfNull(tileUpdates);
    ArgumentNullException.ThrowIfNull(changedTiles);
    ArgumentNullException.ThrowIfNull(mergeActions);
    if (operationsUsed < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(operationsUsed));
    }

    _tileUpdates = new System.Collections.ObjectModel.ReadOnlyDictionary<int, WorldTile>(
      tileUpdates.ToDictionary(pair => pair.Key, pair => pair.Value));
    ChangedTiles = Array.AsReadOnly(changedTiles.ToArray());
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    QueueCapacityExceeded = queueCapacityExceeded;
    OperationsUsed = operationsUsed;
  }

  public IReadOnlyCollection<TileCoordinate> ChangedTiles { get; }

  public IReadOnlyCollection<LiquidMergeAction> MergeActions { get; }

  public bool QueueCapacityExceeded { get; }

  public int OperationsUsed { get; }

  public IReadOnlyDictionary<int, WorldTile> TileUpdates => _tileUpdates;
}

public sealed class LiquidQuickSettleResult
{
  private readonly IReadOnlyDictionary<int, WorldTile> _tileUpdates;

  internal LiquidQuickSettleResult(
    IReadOnlyDictionary<int, WorldTile> tileUpdates,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    int operationsUsed,
    bool completed,
    bool queueCapacityExceeded)
  {
    _tileUpdates = new System.Collections.ObjectModel.ReadOnlyDictionary<int, WorldTile>(
      tileUpdates.ToDictionary(pair => pair.Key, pair => pair.Value));
    ChangedTiles = Array.AsReadOnly(changedTiles.ToArray());
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    OperationsUsed = operationsUsed;
    Completed = completed;
    QueueCapacityExceeded = queueCapacityExceeded;
  }

  public IReadOnlyDictionary<int, WorldTile> TileUpdates => _tileUpdates;

  public IReadOnlyCollection<TileCoordinate> ChangedTiles { get; }

  public IReadOnlyCollection<LiquidMergeAction> MergeActions { get; }

  public int OperationsUsed { get; }

  public bool Completed { get; }

  public bool QueueCapacityExceeded { get; }
}

/// <summary>
/// Owns the bounded liquid work queues; random draws are supplied explicitly by the host.
/// </summary>
public sealed partial class LiquidSimulation
{
  public const int MaximumActiveEntries = 25_000;
  public const int MaximumBufferedEntries = 50_000;

  private readonly Queue<TileCoordinate> _active = new();
  private readonly Queue<TileCoordinate> _buffered = new();
  private readonly Dictionary<TileCoordinate, int> _delays = [];
  private readonly HashSet<TileCoordinate> _queued = [];
  private readonly LiquidSimulationOptions _options;
  private bool _queueCapacityExceeded;
  private bool _quickFall;
  private bool _quickSettle;
  private bool _stuck;
  private int _stuckAmount;
  private int _stuckCount;
  private bool _panicMode;
  private int _panicCounter;
  private int _panicX;
  private int _panicY;

  public LiquidSimulation()
    : this(null)
  {
  }

  public LiquidSimulation(LiquidSimulationOptions? options)
  {
    _options = options ?? new LiquidSimulationOptions(int.MaxValue);
  }

  public int PendingCount => _active.Count + _buffered.Count;

  public int ActiveCount => _active.Count;

  public int BufferedCount => _buffered.Count;

  public bool QueueCapacityExceeded => _queueCapacityExceeded;

  public LiquidSimulationOptions Options => _options;

  public LiquidSimulationStatus Status => new(
    PendingCount,
    ActiveCount,
    BufferedCount,
    QueueCapacityExceeded,
    _quickFall,
    _quickSettle,
    _stuck,
    _stuckCount,
    _panicMode,
    _panicCounter,
    _panicY);

  public void Reset()
  {
    _active.Clear();
    _buffered.Clear();
    _queued.Clear();
    _delays.Clear();
    _queueCapacityExceeded = false;
    _quickFall = false;
    _quickSettle = false;
    _stuck = false;
    _stuckAmount = 0;
    _stuckCount = 0;
    _panicMode = false;
    _panicCounter = 0;
    _panicX = 0;
    _panicY = 0;
    _panicSettle = null;
    _panicRebuildIndex = -1;
  }

  /// <summary>Starts bounded row recovery equivalent to Version4 StartPanic.</summary>
  public void StartPanic(WorldGeometry geometry)
  {
    if (geometry.Width <= 0 || geometry.Height <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(geometry));
    }

    _active.Clear();
    _buffered.Clear();
    _queued.Clear();
    _delays.Clear();
    _panicMode = true;
    _panicCounter = 0;
    _panicX = 4;
    _panicY = Math.Max(0, geometry.Height - 3);
    _panicSettle = null;
    _panicRebuildIndex = -1;
    _quickFall = true;
    _quickSettle = true;
  }

  public bool Enqueue(TileCoordinate coordinate)
  {
    if (!_queued.Add(coordinate))
    {
      return true;
    }

    if (_active.Count < MaximumActiveEntries)
    {
      _active.Enqueue(coordinate);
      return true;
    }

    if (_buffered.Count < MaximumBufferedEntries)
    {
      _buffered.Enqueue(coordinate);
      return true;
    }

    _queued.Remove(coordinate);
    _queueCapacityExceeded = true;
    return false;
  }

  public bool EnqueueAround(TileCoordinate coordinate, WorldGeometry geometry)
  {
    bool accepted = Enqueue(coordinate);
    if (coordinate.X > 0)
    {
      accepted &= Enqueue(new TileCoordinate(coordinate.X - 1, coordinate.Y));
    }

    if (coordinate.X + 1 < geometry.Width)
    {
      accepted &= Enqueue(new TileCoordinate(coordinate.X + 1, coordinate.Y));
    }

    if (coordinate.Y > 0)
    {
      accepted &= Enqueue(new TileCoordinate(coordinate.X, coordinate.Y - 1));
    }

    if (coordinate.Y + 1 < geometry.Height)
    {
      accepted &= Enqueue(new TileCoordinate(coordinate.X, coordinate.Y + 1));
    }

    return accepted;
  }

  public bool EnqueueActiveTiles(IReadOnlyList<WorldTile> tiles, WorldGeometry geometry)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    if (tiles.Count != geometry.TileCount)
    {
      throw new ArgumentException("The tile list does not match the world geometry.", nameof(tiles));
    }

    bool accepted = true;
    for (int y = 0; y < geometry.Height; y++)
    {
      int rowOffset = y * geometry.Width;
      for (int x = 0; x < geometry.Width; x++)
      {
        if (tiles[rowOffset + x].LiquidAmount != 0)
        {
          accepted &= EnqueueAround(new TileCoordinate(x, y), geometry);
        }
      }
    }

    return accepted;
  }

  public bool RebuildFromTiles(IReadOnlyList<WorldTile> tiles, WorldGeometry geometry)
  {
    Reset();
    return EnqueueActiveTiles(tiles, geometry);
  }

  /// <summary>Seeds a loaded world with flow and merge candidates, as WorldGen.WaterCheck does.</summary>
  public bool EnqueueUnsettledTiles(
      IReadOnlyList<WorldTile> tiles, WorldGeometry geometry, WorldFacts facts) {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(facts);
    if (tiles.Count != geometry.TileCount) {
      throw new ArgumentException("The tile list does not match the world geometry.", nameof(tiles));
    }

    for (int x = 1; x < geometry.Width - 1; x++) {
      for (int y = geometry.Height - 2; y > 0; y--) {
        var coordinate = new TileCoordinate(x, y);
        WorldTile tile = tiles[GetIndex(coordinate, geometry)];
        if (tile.LiquidAmount == 0 || facts.BlocksLiquid(tile)) {
          continue;
        }

        bool needsUpdate = false;
        foreach (TileCoordinate neighborCoordinate in GetNeighbors(coordinate, geometry)) {
          WorldTile neighbor = tiles[GetIndex(neighborCoordinate, geometry)];
          if (facts.BlocksLiquid(neighbor)) {
            continue;
          }
          if ((neighbor.LiquidAmount > 0 && neighbor.LiquidType != tile.LiquidType) ||
              (neighborCoordinate.Y > y && neighbor.LiquidAmount < byte.MaxValue) ||
              (neighborCoordinate.Y == y && neighbor.LiquidAmount != tile.LiquidAmount)) {
            needsUpdate = true;
            break;
          }
        }
        if (needsUpdate && !Enqueue(coordinate)) {
          return false;
        }
      }
    }
    return true;
  }

  /// <summary>
  /// Processes at most <paramref name="operationBudget"/> queued cells. All writes remain in a
  /// local overlay until the owning world commits the returned result.
  /// </summary>
  public LiquidStepResult Step(
    IReadOnlyList<WorldTile> tiles,
    WorldGeometry geometry,
    WorldFacts facts,
    int operationBudget,
    ILiquidRandomSource? random = null)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(facts);
    if (tiles.Count != geometry.TileCount)
    {
      throw new ArgumentException("The tile list does not match the world geometry.", nameof(tiles));
    }

    if (operationBudget <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(operationBudget));
    }

    if (PendingCount > 0 || _panicMode)
    {
      ArgumentNullException.ThrowIfNull(random);
    }

    PromoteBufferedEntries();
    var stagedTiles = new Dictionary<int, WorldTile>();
    var changed = new HashSet<TileCoordinate>();
    var merges = new List<LiquidMergeAction>();
    int operations = 0;
    int minimumPanicRow = Math.Max(3, _panicY - 4);
    bool recovering = _panicMode;
    _quickFall = _quickSettle || _panicMode;
    while (operations < operationBudget && (_active.Count > 0 || _panicMode))
    {
      if (_panicMode)
      {
        if (_panicRebuildIndex < 0 && _panicY >= 3 && _panicY < minimumPanicRow) {
          break;
        }
        operations++;
        AdvancePanicWork(tiles, stagedTiles, geometry, facts, changed, merges);
        continue;
      }
      if (recovering) {
        break;
      }
      TileCoordinate coordinate = _active.Dequeue();
      _queued.Remove(coordinate);

      operations++;
      ProcessCoordinate(
        tiles,
        stagedTiles,
        geometry,
        facts,
        coordinate,
        changed,
        merges,
        random!);
      PromoteBufferedEntries();
    }

    if (!recovering) {
      UpdateRecoveryState(tiles, geometry, changed.Count);
    }
    _quickFall = _quickSettle || _panicMode;
    TileCoordinate[] orderedChanges = changed
      .OrderBy(coordinate => coordinate.Y)
      .ThenBy(coordinate => coordinate.X)
      .ToArray();
    return new LiquidStepResult(
      stagedTiles,
      orderedChanges,
      merges,
      _queueCapacityExceeded,
      operations);
  }

  /// <summary>
  /// Runs bounded quick-settle work against a private tile copy. The caller can commit the
  /// returned overlay in one world transaction, or retain it as a loading/recovery preview.
  /// </summary>
  public LiquidQuickSettleResult QuickSettle(
    IReadOnlyList<WorldTile> tiles,
    WorldGeometry geometry,
    WorldFacts facts,
    int maximumOperations,
    ILiquidRandomSource? random = null)
  {
    ArgumentNullException.ThrowIfNull(tiles);
    ArgumentNullException.ThrowIfNull(facts);
    if (tiles.Count != geometry.TileCount)
    {
      throw new ArgumentException("The tile list does not match the world geometry.", nameof(tiles));
    }

    if (maximumOperations <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumOperations));
    }

    if (PendingCount > 0 || _panicMode)
    {
      ArgumentNullException.ThrowIfNull(random);
    }

    WorldTile[] workingTiles = tiles.ToArray();
    var allUpdates = new Dictionary<int, WorldTile>();
    var allChanged = new HashSet<TileCoordinate>();
    var allMerges = new List<LiquidMergeAction>();
    bool previousQuickSettle = _quickSettle;
    _quickSettle = true;
    int operationsUsed = 0;
    bool capacityExceeded = false;
    while (operationsUsed < maximumOperations && (PendingCount > 0 || _panicMode))
    {
      LiquidStepResult step = Step(
        workingTiles,
        geometry,
        facts,
        maximumOperations - operationsUsed,
        random);
      operationsUsed += step.OperationsUsed;
      capacityExceeded |= step.QueueCapacityExceeded;
      foreach (KeyValuePair<int, WorldTile> update in step.TileUpdates)
      {
        workingTiles[update.Key] = update.Value;
        TileCoordinate coordinate = GetCoordinate(update.Key, geometry);
        if (tiles[update.Key] == update.Value)
        {
          allUpdates.Remove(update.Key);
          allChanged.Remove(coordinate);
        }
        else
        {
          allUpdates[update.Key] = update.Value;
          allChanged.Add(coordinate);
        }
      }

      allMerges.AddRange(step.MergeActions);
      if (step.OperationsUsed == 0 || step.QueueCapacityExceeded)
      {
        break;
      }
    }

    _quickSettle = previousQuickSettle;
    _quickFall = _quickSettle || _panicMode;
    return new LiquidQuickSettleResult(
      allUpdates,
      allChanged,
      allMerges,
      operationsUsed,
      !capacityExceeded && PendingCount == 0 && !_panicMode,
      capacityExceeded);
  }

  private void ProcessCoordinate(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    WorldFacts facts,
    TileCoordinate coordinate,
    ISet<TileCoordinate> changed,
    ICollection<LiquidMergeAction> merges,
    ILiquidRandomSource random)
  {
    if (!geometry.Contains(coordinate))
    {
      return;
    }

    int sourceIndex = GetIndex(coordinate, geometry);
    WorldTile source = ReadTile(tiles, stagedTiles, sourceIndex);
    if (source.LiquidAmount == 0 || facts.BlocksLiquid(source))
    {
      return;
    }

    byte originalAmount = source.LiquidAmount;
    if (source.LiquidType == LiquidType.Water && coordinate.Y > _options.UnderworldStartY)
    {
      byte consumedAmount = (byte)Math.Min(2, (int)source.LiquidAmount);
      SetTile(
        tiles,
        stagedTiles,
        geometry,
        coordinate,
        source.WithLiquid((byte)(source.LiquidAmount - consumedAmount), source.LiquidType),
        changed);
      source = ReadTile(tiles, stagedTiles, sourceIndex);
      if (source.LiquidAmount == 0)
      {
        return;
      }
    }

    ScheduleEarlierLiquidChecks(tiles, stagedTiles, geometry, coordinate, source.LiquidType);
    if (source.LiquidType != LiquidType.Water)
    {
      CheckLiquidMerge(tiles, stagedTiles, geometry, facts, coordinate, source, changed, merges);
      source = ReadTile(tiles, stagedTiles, sourceIndex);
      if (source.LiquidAmount == 0)
      {
        return;
      }
    }

    int delayLimit = source.LiquidType switch
    {
      LiquidType.Lava => _options.LavaDelaySteps,
      LiquidType.Honey => _options.HoneyDelaySteps,
      _ => 0
    };
    if (ShouldDelay(coordinate, delayLimit))
    {
      return;
    }

    if (coordinate.Y + 1 < geometry.Height)
    {
      TryTransferDown(
        tiles,
        stagedTiles,
        geometry,
        facts,
        coordinate,
        new TileCoordinate(coordinate.X, coordinate.Y + 1),
        changed);
    }

    source = ReadTile(tiles, stagedTiles, sourceIndex);
    if (source.LiquidAmount == 0)
    {
      return;
    }

    TryEqualizeHorizontally(
      tiles,
      stagedTiles,
      geometry,
      facts,
      coordinate,
      changed,
      random);
    source = ReadTile(tiles, stagedTiles, sourceIndex);
    if (_quickSettle && originalAmount == byte.MaxValue && source.LiquidAmount == 254)
    {
      SetTile(tiles, stagedTiles, geometry, coordinate,
          source.WithLiquid(byte.MaxValue, source.LiquidType), changed);
    }
  }

  private void ScheduleEarlierLiquidChecks(
      IReadOnlyList<WorldTile> tiles, IReadOnlyDictionary<int, WorldTile> stagedTiles,
      WorldGeometry geometry, TileCoordinate coordinate, LiquidType sourceType) {
    foreach (TileCoordinate neighbor in GetNeighbors(coordinate, geometry)) {
      WorldTile tile = ReadTile(tiles, stagedTiles, GetIndex(neighbor, geometry));
      bool schedule = tile.LiquidType switch {
        LiquidType.Lava => sourceType != LiquidType.Lava,
        LiquidType.Honey => sourceType is LiquidType.Water or LiquidType.Shimmer,
        LiquidType.Shimmer => sourceType == LiquidType.Water,
        _ => false
      };
      if (schedule && tile.LiquidAmount > 0) {
        Enqueue(neighbor);
      }
    }
  }

  private void CheckLiquidMerge(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    WorldFacts facts,
    TileCoordinate coordinate,
    WorldTile source,
    ISet<TileCoordinate> changed,
    ICollection<LiquidMergeAction> merges,
    LiquidType? checkedType = null)
  {
    if (coordinate.X <= 0 || coordinate.Y <= 0 ||
        coordinate.X >= geometry.Width - 1 || coordinate.Y >= geometry.Height - 1)
    {
      return;
    }

    LiquidType liquidType = checkedType ?? source.LiquidType;
    var conflicting = new List<(TileCoordinate Coordinate, WorldTile Tile)>();
    foreach (TileCoordinate neighbor in GetNeighbors(coordinate, geometry))
    {
      if (neighbor.Y > coordinate.Y)
      {
        continue;
      }
      WorldTile tile = ReadTile(tiles, stagedTiles, GetIndex(neighbor, geometry));
      if (tile.LiquidAmount > 0 && tile.LiquidType != liquidType)
      {
        conflicting.Add((neighbor, tile));
      }
    }

    // LiquidCheck consumes conflicting left/right/up cells even when the sum is below
    // 24 or the center cannot be replaced. A side conflict excludes the downward branch.
    if (conflicting.Count > 0)
    {
      int neighborAmount = 0;
      foreach ((TileCoordinate neighbor, WorldTile tile) in conflicting)
      {
        neighborAmount += tile.LiquidAmount;
        SetTile(tiles, stagedTiles, geometry, neighbor,
            tile.WithLiquid(0, tile.LiquidType), changed);
      }
      if (neighborAmount >= LiquidMergeRules.MinimumMergeAmount &&
          facts.AllowsLiquidMerge(source) &&
          LiquidMergeRules.TryResolve(liquidType,
              conflicting.Select(item => item.Tile.LiquidType).ToArray(),
              out ushort resultTileType, out _, out LiquidType existingType))
      {
        StageMergeTile(tiles, stagedTiles, geometry, coordinate, source,
            resultTileType, liquidType, existingType,
            source.LiquidAmount + neighborAmount, changed, merges);
      }
      return;
    }

    TileCoordinate below = new(coordinate.X, coordinate.Y + 1);
    WorldTile target = ReadTile(tiles, stagedTiles, GetIndex(below, geometry));
    if (target.LiquidAmount == 0 || target.LiquidType == liquidType)
    {
      return;
    }
    // TileID.Sets.IsAContainer is exactly 21, 467, 88 in the same-version source.
    bool containerOverride = source.Active && (source.Type is 21 or 467 or 88) &&
        target.Type is not (21 or 467 or 88);
    if (!facts.AllowsLiquidMerge(target) && !containerOverride)
    {
      return;
    }
    if (source.LiquidAmount < LiquidMergeRules.MinimumMergeAmount)
    {
      SetTile(tiles, stagedTiles, geometry, coordinate,
          source.WithLiquid(0, LiquidType.Water), changed);
      return;
    }
    if (!LiquidMergeRules.TryResolve(liquidType, [target.LiquidType],
        out ushort belowTileType, out _, out LiquidType belowExistingType))
    {
      return;
    }
    SetTile(tiles, stagedTiles, geometry, coordinate,
        source.WithLiquid(0, source.LiquidType), changed);
    StageMergeTile(tiles, stagedTiles, geometry, below, target,
        belowTileType, liquidType, belowExistingType,
        source.LiquidAmount + target.LiquidAmount, changed, merges);
  }

  private void StageMergeTile(
      IReadOnlyList<WorldTile> tiles, Dictionary<int, WorldTile> stagedTiles,
      WorldGeometry geometry, TileCoordinate coordinate, WorldTile target,
      ushort resultTileType, LiquidType incomingType, LiquidType existingType,
      int consumedAmount, ISet<TileCoordinate> changed, ICollection<LiquidMergeAction> merges) {
    const byte blockCoatingMask = 0xA0;
    SetTile(
      tiles,
      stagedTiles,
      geometry,
      coordinate,
      target with
      {
        Active = true,
        Type = resultTileType,
        FrameX = -1,
        FrameY = -1,
        TileColor = 0,
        TileHeader3 = (byte)(target.TileHeader3 & ~blockCoatingMask),
        // PlaceTile clears slope for an empty target. Active replacement retains the
        // existing shape until the full CanPoundTile policy is applied.
        HalfBlock = target.Active && target.HalfBlock,
        Slope = target.Active ? target.Slope : (byte)0,
        LiquidAmount = 0
      },
      changed);
    merges.Add(new LiquidMergeAction(
      coordinate,
      resultTileType,
      incomingType,
      existingType,
      consumedAmount));
  }

  private bool TryEqualizeHorizontally(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    WorldFacts facts,
    TileCoordinate coordinate,
    ISet<TileCoordinate> changed,
    ILiquidRandomSource random)
  {
    WorldTile source = ReadTile(tiles, stagedTiles, GetIndex(coordinate, geometry));
    if (source.LiquidAmount == 0)
    {
      return false;
    }

    int offset = source.LiquidAmount < 3 ? -1 : 0;
    bool leftImmediate = CanShareHorizontally(
      tiles, stagedTiles, geometry, facts, coordinate, -1, source.LiquidType, requireLiquid: false);
    bool rightImmediate = CanShareHorizontally(
      tiles, stagedTiles, geometry, facts, coordinate, 1, source.LiquidType, requireLiquid: false);

    // Version4 selects exactly one of the 7/5/4/3/2-cell branches. It does not average
    // every contiguous neighbor that happens to be present, because the farther cells are
    // considered only after both immediate cells have passed their own checks.
    if (leftImmediate && rightImmediate)
    {
      bool leftTwo = source.LiquidAmount <= 250 && CanShareHorizontally(
        tiles, stagedTiles, geometry, facts, coordinate, -2, source.LiquidType, requireLiquid: true);
      bool rightTwo = source.LiquidAmount <= 250 && CanShareHorizontally(
        tiles, stagedTiles, geometry, facts, coordinate, 2, source.LiquidType, requireLiquid: true);
      if (leftTwo && rightTwo)
      {
        bool leftThree = CanShareHorizontally(
          tiles, stagedTiles, geometry, facts, coordinate, -3, source.LiquidType, requireLiquid: true);
        bool rightThree = CanShareHorizontally(
          tiles, stagedTiles, geometry, facts, coordinate, 3, source.LiquidType, requireLiquid: true);
        if (leftThree && rightThree)
        {
          return EqualizeLine(
            tiles,
            stagedTiles,
            geometry,
            coordinate,
            source.LiquidType,
            [-3, -2, -1, 0, 1, 2, 3],
            offset,
            preserveStableCenter: true,
            changed,
            random);
        }

        return EqualizeLine(
          tiles,
          stagedTiles,
          geometry,
          coordinate,
          source.LiquidType,
          [-2, -1, 0, 1, 2],
          offset,
          preserveStableCenter: true,
          changed,
          random);
      }

      if (leftTwo)
      {
        return EqualizeLine(
          tiles,
          stagedTiles,
          geometry,
          coordinate,
          source.LiquidType,
          [-2, -1, 0, 1],
          offset,
          preserveStableCenter: false,
          changed,
          random);
      }

      if (rightTwo)
      {
        return EqualizeLine(
          tiles,
          stagedTiles,
          geometry,
          coordinate,
          source.LiquidType,
          [-1, 0, 1, 2],
          offset,
          preserveStableCenter: false,
          changed,
          random);
      }

      return EqualizeLine(
        tiles,
        stagedTiles,
        geometry,
        coordinate,
        source.LiquidType,
        [-1, 0, 1],
        offset,
        preserveStableCenter: false,
        changed,
        random);
    }

    if (leftImmediate)
    {
      return EqualizeLine(
        tiles,
        stagedTiles,
        geometry,
        coordinate,
        source.LiquidType,
        [-1, 0],
        offset,
        preserveStableCenter: false,
        changed,
        random);
    }

    if (rightImmediate)
    {
      return EqualizeLine(
        tiles,
        stagedTiles,
        geometry,
        coordinate,
        source.LiquidType,
        [0, 1],
        offset,
        preserveStableCenter: false,
        changed,
        random);
    }

    return false;
  }

  private bool CanShareHorizontally(
    IReadOnlyList<WorldTile> tiles,
    IReadOnlyDictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    WorldFacts facts,
    TileCoordinate origin,
    int offset,
    LiquidType liquidType,
    bool requireLiquid)
  {
    TileCoordinate coordinate = new(origin.X + offset, origin.Y);
    if (!geometry.Contains(coordinate))
    {
      return false;
    }

    WorldTile tile = ReadTile(tiles, stagedTiles, GetIndex(coordinate, geometry));
    if (facts.BlocksLiquid(tile))
    {
      return false;
    }

    if (tile.LiquidAmount == 0)
    {
      return !requireLiquid;
    }

    return tile.LiquidType == liquidType;
  }

  private bool EqualizeLine(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    TileCoordinate origin,
    LiquidType liquidType,
    IReadOnlyList<int> offsets,
    int offsetAdjustment,
    bool preserveStableCenter,
    ISet<TileCoordinate> changed,
    ILiquidRandomSource random)
  {
    int total = 0;
    var coordinates = new TileCoordinate[offsets.Count];
    for (int index = 0; index < offsets.Count; index++)
    {
      coordinates[index] = new TileCoordinate(origin.X + offsets[index], origin.Y);
      total += ReadTile(
        tiles,
        stagedTiles,
        GetIndex(coordinates[index], geometry)).LiquidAmount;
    }

    int targetAmount = (int)Math.Round(
      (total + offsetAdjustment) / (double)coordinates.Length,
      MidpointRounding.ToEven);
    targetAmount = Math.Clamp(targetAmount, 0, byte.MaxValue);
    if (coordinates.Length == 3 && targetAmount == 254) {
      int draw = random.Next(30);
      if ((uint)draw >= 30) {
        throw new InvalidOperationException("The liquid random source returned an invalid draw.");
      }
      if (draw == 0) {
        targetAmount = byte.MaxValue;
      }
    }

    int unchangedNeighborCount = 0;
    foreach (TileCoordinate coordinate in coordinates)
    {
      if (coordinate != origin &&
          ReadTile(tiles, stagedTiles, GetIndex(coordinate, geometry)).LiquidAmount == targetAmount)
      {
        unchangedNeighborCount++;
      }
    }

    bool preserveCenter = preserveStableCenter &&
      unchangedNeighborCount == coordinates.Length - 1 &&
      origin.Y > 0 &&
      ReadTile(tiles, stagedTiles, GetIndex(new TileCoordinate(origin.X, origin.Y - 1), geometry))
        .LiquidAmount > 0;
    bool changedAny = false;
    foreach (TileCoordinate coordinate in coordinates)
    {
      if (coordinate == origin && preserveCenter)
      {
        continue;
      }

      WorldTile current = ReadTile(tiles, stagedTiles, GetIndex(coordinate, geometry));
      WorldTile next = current.WithLiquid((byte)targetAmount, liquidType);
      if (current == next)
      {
        continue;
      }

      SetTile(tiles, stagedTiles, geometry, coordinate, next, changed);
      EnqueueAround(coordinate, geometry);
      changedAny = true;
    }

    return changedAny;
  }

  private bool TryTransferDown(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    WorldFacts facts,
    TileCoordinate sourceCoordinate,
    TileCoordinate targetCoordinate,
    ISet<TileCoordinate> changed)
  {
    if (!geometry.Contains(targetCoordinate))
    {
      return false;
    }

    int sourceIndex = GetIndex(sourceCoordinate, geometry);
    int targetIndex = GetIndex(targetCoordinate, geometry);
    WorldTile source = ReadTile(tiles, stagedTiles, sourceIndex);
    WorldTile target = ReadTile(tiles, stagedTiles, targetIndex);
    if (source.LiquidAmount == 0 || facts.BlocksLiquid(target) ||
        (target.LiquidAmount != 0 && target.LiquidType != source.LiquidType))
    {
      return false;
    }

    int capacity = byte.MaxValue - target.LiquidAmount;
    int amount = Math.Min(capacity, source.LiquidAmount);
    if (amount <= 0)
    {
      return false;
    }

    bool retainFullSource = amount == 1 && source.LiquidAmount == byte.MaxValue;
    byte sourceAmount = retainFullSource
      ? source.LiquidAmount
      : (byte)(source.LiquidAmount - amount);
    if (_quickSettle && sourceAmount > 250)
    {
      sourceAmount = byte.MaxValue;
    }

    SetTile(
      tiles,
      stagedTiles,
      geometry,
      sourceCoordinate,
      source.WithLiquid(sourceAmount, source.LiquidType),
      changed);
    SetTile(
      tiles,
      stagedTiles,
      geometry,
      targetCoordinate,
      target.WithLiquid((byte)(target.LiquidAmount + amount), source.LiquidType),
      changed);
    EnqueueAround(sourceCoordinate, geometry);
    EnqueueAround(targetCoordinate, geometry);
    return true;
  }

  private bool ShouldDelay(TileCoordinate coordinate, int delayLimit)
  {
    if (_quickFall || delayLimit == 0)
    {
      _delays.Remove(coordinate);
      return false;
    }

    int nextDelay = _delays.TryGetValue(coordinate, out int currentDelay)
      ? currentDelay + 1
      : 1;
    if (nextDelay <= delayLimit)
    {
      _delays[coordinate] = nextDelay;
      Enqueue(coordinate);
      return true;
    }

    _delays.Remove(coordinate);
    return false;
  }

  private void SetTile(
    IReadOnlyList<WorldTile> tiles,
    Dictionary<int, WorldTile> stagedTiles,
    WorldGeometry geometry,
    TileCoordinate coordinate,
    WorldTile next,
    ISet<TileCoordinate> changed)
  {
    int index = GetIndex(coordinate, geometry);
    WorldTile current = ReadTile(tiles, stagedTiles, index);
    if (current == next)
    {
      return;
    }

    // Only final differences are published. Queue effects still reflect all intermediate
    // changes, including a quick-settle cell that was restored to its original amount.
    if (tiles[index] == next)
    {
      stagedTiles.Remove(index);
      changed.Remove(coordinate);
    }
    else
    {
      stagedTiles[index] = next;
      changed.Add(coordinate);
    }
  }

  private void UpdateRecoveryState(
    IReadOnlyList<WorldTile> tiles,
    WorldGeometry geometry,
    int changedCount)
  {
    if (_buffered.Count >= _options.PanicBufferThreshold && !_panicMode)
    {
      _panicCounter++;
      if (_panicCounter >= _options.PanicTriggerSteps)
      {
        StartPanic(geometry);
      }
    }
    else if (_buffered.Count < _options.PanicBufferThreshold)
    {
      _panicCounter = 0;
    }

    int pending = PendingCount;
    if (pending > 0 && Math.Abs(pending - _stuckAmount) < 50 && changedCount == 0)
    {
      _stuckCount++;
      if (_stuckCount >= _options.StuckTriggerSteps)
      {
        _stuck = true;
        RebuildFromTiles(tiles, geometry);
        _stuck = false;
        _stuckCount = 0;
      }
    }
    else
    {
      _stuckAmount = pending;
      _stuckCount = 0;
    }
  }

  private void PromoteBufferedEntries()
  {
    while (_active.Count < MaximumActiveEntries && _buffered.Count > 0)
    {
      _active.Enqueue(_buffered.Dequeue());
    }
  }

  private static WorldTile ReadTile(
    IReadOnlyList<WorldTile> tiles,
    IReadOnlyDictionary<int, WorldTile> stagedTiles,
    int index)
  {
    return stagedTiles.TryGetValue(index, out WorldTile staged) ? staged : tiles[index];
  }

  private static int GetIndex(TileCoordinate coordinate, WorldGeometry geometry)
  {
    return checked((coordinate.Y * geometry.Width) + coordinate.X);
  }

  private static TileCoordinate GetCoordinate(int index, WorldGeometry geometry)
  {
    return new TileCoordinate(index % geometry.Width, index / geometry.Width);
  }

  private static IReadOnlyList<TileCoordinate> GetNeighbors(
    TileCoordinate coordinate,
    WorldGeometry geometry)
  {
    var result = new List<TileCoordinate>(4);
    if (coordinate.X > 0)
    {
      result.Add(new TileCoordinate(coordinate.X - 1, coordinate.Y));
    }

    if (coordinate.X + 1 < geometry.Width)
    {
      result.Add(new TileCoordinate(coordinate.X + 1, coordinate.Y));
    }

    if (coordinate.Y > 0)
    {
      result.Add(new TileCoordinate(coordinate.X, coordinate.Y - 1));
    }

    if (coordinate.Y + 1 < geometry.Height)
    {
      result.Add(new TileCoordinate(coordinate.X, coordinate.Y + 1));
    }

    return result;
  }

}
