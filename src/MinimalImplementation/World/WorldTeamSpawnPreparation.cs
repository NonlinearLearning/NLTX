using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Read-only preparation for packet 157. The host encodes Baselines, validates all owners, then
/// commits in the same exclusive ServerLoop window before publishing the ordered outbound batch.
/// </summary>
public sealed class WorldTeamSpawnPreparation {
  private readonly Func<WorldSectionTransferStatus> _validate;
  private readonly Action _commit;
  private bool _committed;

  public WorldSectionTransferToken Token { get; }
  public SessionEpoch Epoch => Token.SessionEpoch;
  public byte Team { get; }
  public ulong WorldEpoch => Token.WorldEpoch;
  public CommitVersion Version { get; }
  public TileCoordinate? SpawnPoint { get; }
  public bool RequiresExtraSpawn => SpawnPoint.HasValue;
  public IReadOnlyList<WorldSectionCoordinate> RequiredSections { get; }
  public IReadOnlyList<WorldSectionSnapshot> Baselines { get; }

  internal WorldTeamSpawnPreparation(
      WorldSectionTransferToken token, byte team, CommitVersion version,
      TileCoordinate? spawnPoint, WorldSectionCoordinate[] requiredSections,
      WorldSectionSnapshot[] baselines, Func<WorldSectionTransferStatus> validate, Action commit) {
    Token = token;
    Team = team;
    Version = version;
    SpawnPoint = spawnPoint;
    RequiredSections = Array.AsReadOnly(requiredSections);
    Baselines = Array.AsReadOnly(baselines);
    _validate = validate;
    _commit = commit;
  }

  public WorldSectionTransferStatus Validate() {
    return _committed ? WorldSectionTransferStatus.InvalidTransition : _validate.Invoke();
  }

  /// <summary>
  /// Requires a successful Validate with no intervening owner/transfer writes. This publishes
  /// preallocated state; it does not encode, perform I/O, or return a late business rejection.
  /// </summary>
  public void Commit() {
    if (_committed) {
      throw new InvalidOperationException("The team-spawn preparation was already committed.");
    }
    _commit.Invoke();
    _committed = true;
  }
}
