using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// World half of a host-coordinated tile/TE registry transaction. The registry owner validates
/// its own anchor and identity constraints before either owner publishes its prepared state.
/// </summary>
public sealed class WorldTileMutationPreparation {
  private readonly Func<WorldValidationResult> _validate;
  private readonly Action _commit;
  private bool _committed;

  public ulong WorldEpoch { get; }
  public CommitVersion BaseVersion { get; }
  public WorldMutationResult Result { get; }
  public IReadOnlyList<KeyValuePair<TileCoordinate, WorldTile>> Tiles { get; }

  internal WorldTileMutationPreparation(
      ulong worldEpoch, CommitVersion baseVersion, WorldMutationResult result,
      KeyValuePair<TileCoordinate, WorldTile>[] tiles,
      Func<WorldValidationResult> validate, Action commit) {
    WorldEpoch = worldEpoch;
    BaseVersion = baseVersion;
    Result = result;
    Tiles = Array.AsReadOnly(tiles);
    _validate = validate;
    _commit = commit;
  }

  public WorldValidationResult Validate() {
    return _committed
        ? WorldValidationResult.Invalid(["The tile preparation was already committed."])
        : _validate.Invoke();
  }

  /// <summary>
  /// Requires successful validation of every participant in one exclusive ServerLoop window,
  /// with no intervening writes. All allocations and business checks happened during prepare.
  /// </summary>
  public void Commit() {
    if (_committed) {
      throw new InvalidOperationException("The tile preparation was already committed.");
    }
    _commit.Invoke();
    _committed = true;
  }
}
