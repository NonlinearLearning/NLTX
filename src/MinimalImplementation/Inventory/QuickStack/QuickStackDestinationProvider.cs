using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.QuickStack;

/// <summary>
/// Selects same-world chests from server-owned positions, access checks, and leases. The maximum
/// distance must be supplied explicitly because the inspected Version4 QuickStack body is empty.
/// </summary>
public sealed class QuickStackDestinationProvider : IQuickStackDestinationProvider
{
  private readonly ChestStore _chests;
  private readonly ChestLeaseStore _leases;
  private readonly IChestAccessPolicy _accessPolicy;
  private readonly IQuickStackPlayerPositionProvider _positions;
  private readonly double _maximumDistanceTiles;

  public QuickStackDestinationProvider(
    ChestStore chests,
    ChestLeaseStore leases,
    IChestAccessPolicy accessPolicy,
    IQuickStackPlayerPositionProvider positions,
    double maximumDistanceTiles)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(leases);
    ArgumentNullException.ThrowIfNull(accessPolicy);
    ArgumentNullException.ThrowIfNull(positions);
    if (!double.IsFinite(maximumDistanceTiles) || maximumDistanceTiles <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumDistanceTiles));
    }

    if (!ReferenceEquals(chests.CommitGate, leases.CommitGate))
    {
      throw new ArgumentException("Chest and lease stores must share one commit gate.", nameof(leases));
    }

    _chests = chests;
    _leases = leases;
    _accessPolicy = accessPolicy;
    _positions = positions;
    _maximumDistanceTiles = maximumDistanceTiles;
    CommitGate = chests.CommitGate;
  }

  public ServerStateCommitGate CommitGate { get; }

  public IReadOnlyList<QuickStackDestinationCandidate>? SelectDestinations(
    EntityUuid authenticatedPlayerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now)
  {
    if (!authenticatedPlayerId.IsValid
      || !epoch.IsValid
      || !worldScope.IsValid
      || !_positions.TryGetPosition(
        authenticatedPlayerId,
        epoch,
        worldScope,
        out QuickStackPlayerPosition playerPosition)
      || !playerPosition.IsValid)
    {
      return null;
    }

    var selected = new List<(QuickStackDestinationCandidate Candidate, double DistanceSquared)>();
    foreach (ChestSnapshot chest in _chests.CaptureWorld(worldScope))
    {
      if (chest.Position.X < 0 || chest.Position.Y < 0)
      {
        continue;
      }

      double deltaX = playerPosition.TileX - chest.Position.X;
      double deltaY = playerPosition.TileY - chest.Position.Y;
      double distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);
      if (Math.Sqrt(distanceSquared) > _maximumDistanceTiles
        || !_accessPolicy.CanOpen(authenticatedPlayerId, worldScope, chest))
      {
        continue;
      }

      bool isHeldByDifferentSession = _leases.IsHeldByDifferentSession(
        chest,
        authenticatedPlayerId,
        epoch,
        now);
      selected.Add((
        new QuickStackDestinationCandidate(chest, isHeldByDifferentSession),
        distanceSquared));
    }

    return Array.AsReadOnly(selected
      .OrderBy(item => item.DistanceSquared)
      .ThenBy(item => item.Candidate.Chest.Id.Value)
      .Select(item => item.Candidate)
      .ToArray());
  }
}
