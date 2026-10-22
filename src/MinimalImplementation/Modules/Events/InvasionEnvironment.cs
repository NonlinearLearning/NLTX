using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Modules.Events;

/// <summary>
/// Committed world geometry/time and the count of active players with max life >= 200.
/// World and qualified-player facts have separate source revisions because they are owned by
/// different upstream captures. The Host must populate both revisions from the same commit-window
/// fact capture; this type does not infer one source revision from the other.
/// </summary>
public sealed record InvasionEnvironment(
    long WorldScope,
    int QualifiedPlayerCount,
    int SpawnTileX,
    int MaximumTilesX,
    int DayRate,
    CommitVersion WorldRevision,
    CommitVersion QualifiedPlayersRevision);
