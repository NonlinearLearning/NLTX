namespace MinimalImplementation.Server.World;

/// <summary>Persisted team-spawn facts; unavailable is distinct from a resolved empty list.</summary>
public sealed class WorldTeamSpawnFacts {
  public static WorldTeamSpawnFacts Unavailable { get; } = new();

  public bool IsReady { get; }
  public bool TeamBasedSpawns { get; }
  public IReadOnlyList<TileCoordinate> ExtraSpawnPoints { get; }

  private WorldTeamSpawnFacts() {
    ExtraSpawnPoints = Array.Empty<TileCoordinate>();
  }

  public WorldTeamSpawnFacts(bool teamBasedSpawns, IReadOnlyList<TileCoordinate> extraSpawnPoints) {
    ArgumentNullException.ThrowIfNull(extraSpawnPoints);
    if (extraSpawnPoints.Count > byte.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(extraSpawnPoints));
    }
    IsReady = true;
    TeamBasedSpawns = teamBasedSpawns;
    ExtraSpawnPoints = Array.AsReadOnly(extraSpawnPoints.ToArray());
  }
}
