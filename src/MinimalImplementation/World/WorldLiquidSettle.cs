using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>Result of one bounded quick-settle commit at the world-owner boundary.</summary>
public sealed class WorldLiquidSettleResult
{
  private WorldLiquidSettleResult(
    bool applied,
    bool completed,
    int operationsUsed,
    CommitVersion? version,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions,
    string? rejection)
  {
    Applied = applied;
    Completed = completed;
    OperationsUsed = operationsUsed;
    Version = version;
    ChangedTiles = Array.AsReadOnly(changedTiles.ToArray());
    DirtySections = Array.AsReadOnly(dirtySections.ToArray());
    MergeActions = Array.AsReadOnly(mergeActions.ToArray());
    Rejection = rejection;
  }

  public bool Applied { get; }

  public bool Completed { get; }

  public int OperationsUsed { get; }

  public CommitVersion? Version { get; }

  public IReadOnlyList<TileCoordinate> ChangedTiles { get; }

  public IReadOnlyList<WorldSectionCoordinate> DirtySections { get; }

  public IReadOnlyList<LiquidMergeAction> MergeActions { get; }

  public string? Rejection { get; }

  internal static WorldLiquidSettleResult Success(
    bool completed,
    int operationsUsed,
    CommitVersion? version,
    IReadOnlyCollection<TileCoordinate> changedTiles,
    IReadOnlyCollection<WorldSectionCoordinate> dirtySections,
    IReadOnlyCollection<LiquidMergeAction> mergeActions)
  {
    return new WorldLiquidSettleResult(
      true,
      completed,
      operationsUsed,
      version,
      changedTiles,
      dirtySections,
      mergeActions,
      null);
  }

  internal static WorldLiquidSettleResult Rejected(string rejection)
  {
    return new WorldLiquidSettleResult(
      false,
      false,
      0,
      null,
      Array.Empty<TileCoordinate>(),
      Array.Empty<WorldSectionCoordinate>(),
      Array.Empty<LiquidMergeAction>(),
      rejection);
  }
}
