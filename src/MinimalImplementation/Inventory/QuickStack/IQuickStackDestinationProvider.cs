using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Selects active, nearby chests from server-owned world, position, access, and lease state.
/// Results must be ordered deterministically and remain a read-only snapshot.
/// </summary>
public interface IQuickStackDestinationProvider
{
  ServerStateCommitGate CommitGate { get; }

  IReadOnlyList<QuickStackDestinationCandidate>? SelectDestinations(
    EntityUuid authenticatedPlayerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now);
}

/// <summary>Provides a server-owned player position in fractional world-tile coordinates.</summary>
public interface IQuickStackPlayerPositionProvider
{
  bool TryGetPosition(
    EntityUuid authenticatedPlayerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    out QuickStackPlayerPosition position);
}
