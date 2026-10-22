using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>Commits P03's once-per-tick world time and liquid progression.</summary>
public sealed class WorldTickCoordinator
{
  private readonly WorldOwner _world;
  private ulong _lastWorldEpoch;
  private long _lastAppliedTick = -1;

  public WorldTickCoordinator(WorldOwner world)
  {
    ArgumentNullException.ThrowIfNull(world);
    _world = world;
  }

  public WorldTickResult Advance(
    ulong worldEpoch,
    long tick,
    double elapsedGameTicks,
    int dayRate,
    int liquidOperationBudget)
  {
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    WorldTickCapture capture = _world.CaptureWorldTick();
    if (capture.WorldEpoch != worldEpoch)
    {
      return WorldTickResult.Rejected(
        tick,
        capture.WorldEpoch,
        capture.Version,
        capture.Version,
        "The world epoch is stale.");
    }

    return Advance(capture, tick, elapsedGameTicks, dayRate, liquidOperationBudget);
  }

  public WorldTickResult Advance(
    WorldTickCapture capture,
    long tick,
    double elapsedGameTicks,
    int dayRate,
    int liquidOperationBudget)
  {
    ArgumentNullException.ThrowIfNull(capture);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }

    if (liquidOperationBudget <= 0)
    {
      return WorldTickResult.Rejected(
        tick,
        capture.WorldEpoch,
        capture.Version,
        capture.Version,
        "The liquid operation budget must be positive.");
    }

    if (capture.Version.Value == long.MaxValue)
    {
      return WorldTickResult.Rejected(
        tick,
        capture.WorldEpoch,
        capture.Version,
        capture.Version,
        "The world commit version is exhausted.");
    }

    if (_lastWorldEpoch == capture.WorldEpoch && tick <= _lastAppliedTick)
    {
      return WorldTickResult.Rejected(
        tick,
        capture.WorldEpoch,
        capture.Version,
        capture.Version,
        "The world tick is duplicated or out of order.");
    }

    WorldTickMutationResult mutation = _world.AdvanceWorldTick(
      capture,
      elapsedGameTicks,
      dayRate,
      liquidOperationBudget);
    if (!mutation.Applied)
    {
      return WorldTickResult.Rejected(
        tick,
        capture.WorldEpoch,
        capture.Version,
        mutation.Version,
        mutation.Rejection!);
    }

    _lastWorldEpoch = capture.WorldEpoch;
    _lastAppliedTick = tick;
    return WorldTickResult.Success(
      tick,
      capture.WorldEpoch,
      capture.Version,
      mutation.Version,
      mutation.TimeChanged,
      mutation.MorningTransitionCount,
      mutation.ChangedLiquidTiles,
      mutation.DirtySections,
      mutation.MergeActions);
  }
}

internal sealed class WorldTickMutationResult
{
  private WorldTickMutationResult(
    bool applied,
    CommitVersion version,
    bool timeChanged,
    long morningTransitionCount,
    IReadOnlyCollection<TileCoordinate> changedLiquidTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    string? rejection)
  {
    Applied = applied;
    Version = version;
    TimeChanged = timeChanged;
    MorningTransitionCount = morningTransitionCount;
    ChangedLiquidTiles = Array.AsReadOnly(changedLiquidTiles.ToArray());
    DirtySections = Array.AsReadOnly(dirtySections
      .OrderBy(section => section.Y)
      .ThenBy(section => section.X)
      .ToArray());
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    Rejection = rejection;
  }

  public bool Applied { get; }

  public CommitVersion Version { get; }

  public bool TimeChanged { get; }

  public long MorningTransitionCount { get; }

  public IReadOnlyCollection<TileCoordinate> ChangedLiquidTiles { get; }

  public IReadOnlyCollection<WorldSectionCoordinate> DirtySections { get; }

  public IReadOnlyList<LiquidMergeAction> MergeActions { get; }

  public string? Rejection { get; }

  internal static WorldTickMutationResult Success(
    CommitVersion version,
    bool timeChanged,
    long morningTransitionCount,
    IReadOnlyCollection<TileCoordinate> changedLiquidTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions)
  {
    return new WorldTickMutationResult(
      true,
      version,
      timeChanged,
      morningTransitionCount,
      changedLiquidTiles,
      dirtySections,
      mergeActions,
      null);
  }

  internal static WorldTickMutationResult Rejected(
    CommitVersion version,
    string rejection)
  {
    return new WorldTickMutationResult(
      false,
      version,
      false,
      0,
      Array.Empty<TileCoordinate>(),
      Array.Empty<WorldSectionCoordinate>(),
      Array.Empty<LiquidMergeAction>(),
      rejection);
  }
}

public sealed class WorldTickResult
{
  private WorldTickResult(
    bool applied,
    long tick,
    ulong worldEpoch,
    CommitVersion sourceVersion,
    CommitVersion version,
    bool timeChanged,
    long morningTransitionCount,
    IReadOnlyCollection<TileCoordinate> changedLiquids,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    string? rejection)
  {
    Applied = applied;
    Tick = tick;
    WorldEpoch = worldEpoch;
    SourceVersion = sourceVersion;
    Version = version;
    TimeChanged = timeChanged;
    MorningTransitionCount = morningTransitionCount;
    ChangedLiquidTiles = Array.AsReadOnly(changedLiquids.ToArray());
    DirtySections = Array.AsReadOnly(dirtySections.ToArray());
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    Rejection = rejection;
  }

  public bool Applied { get; }

  public long Tick { get; }

  public ulong WorldEpoch { get; }

  public CommitVersion SourceVersion { get; }

  public CommitVersion Version { get; }

  public bool TimeChanged { get; }

  public long MorningTransitionCount { get; }

  public bool MorningTransition => MorningTransitionCount > 0;

  public IReadOnlyList<TileCoordinate> ChangedLiquidTiles { get; }

  public IReadOnlyList<WorldSectionCoordinate> DirtySections { get; }

  /// <summary>Committed merge facts; returning these does not execute cross-owner effects.</summary>
  public IReadOnlyList<LiquidMergeAction> MergeActions { get; }

  public string? Rejection { get; }

  internal static WorldTickResult Success(
    long tick,
    ulong worldEpoch,
    CommitVersion sourceVersion,
    CommitVersion version,
    bool timeChanged,
    long morningTransitionCount,
    IReadOnlyCollection<TileCoordinate> changedLiquids,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions)
  {
    return new WorldTickResult(
      true,
      tick,
      worldEpoch,
      sourceVersion,
      version,
      timeChanged,
      morningTransitionCount,
      changedLiquids,
      dirtySections,
      mergeActions,
      null);
  }

  internal static WorldTickResult Rejected(
    long tick,
    ulong worldEpoch,
    CommitVersion sourceVersion,
    CommitVersion version,
    string rejection)
  {
    return new WorldTickResult(
      false,
      tick,
      worldEpoch,
      sourceVersion,
      version,
      false,
      0,
      Array.Empty<TileCoordinate>(),
      Array.Empty<WorldSectionCoordinate>(),
      Array.Empty<LiquidMergeAction>(),
      rejection);
  }
}
