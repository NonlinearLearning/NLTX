using System;
using System.Collections.Generic;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Projectiles;

/// <summary>Owns player projectile tracker references without retaining projectile objects.</summary>
public sealed class ProjectileTrackerManager
{
  private sealed record PlayerTrackers(
    SessionEpoch OwnerEpoch,
    ProjectileTrackerReference? PiggyBank,
    ProjectileTrackerReference? VoidLens,
    long Revision,
    CommitVersion CommitVersion);

  private readonly ICombatEntityIdentityAuthority _identityAuthority;
  private readonly Dictionary<byte, PlayerTrackers> _players = [];
  private long _worldScope;
  private long _revision;
  private CommitVersion _commitVersion = CommitVersion.Initial;

  public ProjectileTrackerManager(ICombatEntityIdentityAuthority identityAuthority)
  {
    ArgumentNullException.ThrowIfNull(identityAuthority);
    if (identityAuthority.CurrentWorldScope <= 0)
    {
      throw new ArgumentException("Projectile trackers require an active world scope.", nameof(identityAuthority));
    }

    _identityAuthority = identityAuthority;
    _worldScope = identityAuthority.CurrentWorldScope;
  }

  public long Revision => _revision;

  public CommitVersion CommitVersion => _commitVersion;

  public OwnerCommitResult Apply(
    byte player,
    SessionEpoch ownerEpoch,
    ProjectileTrackerReference? piggyBank,
    ProjectileTrackerReference? voidLens)
  {
    if (player == byte.MaxValue || !ownerEpoch.IsValid)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "A projectile tracker update requires an authenticated player session.");
    }

    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Projectile trackers must be cleared after world-scope invalidation before updates.");
    }

    if (!IsCurrent(piggyBank) || !IsCurrent(voidLens))
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "A projectile tracker target is no longer active in the current world scope.");
    }

    if (!CanAdvanceCommit())
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile tracker version range is exhausted.");
    }

    long nextRevision = _revision + 1;
    CommitVersion nextVersion = _commitVersion.Next();
    _players[player] = new PlayerTrackers(
      ownerEpoch,
      piggyBank,
      voidLens,
      nextRevision,
      nextVersion);
    CommitMutation(nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  public bool TryCapture(
    byte player,
    SessionEpoch ownerEpoch,
    ProjectileManager projectiles,
    out ProjectileTrackerSnapshotView? snapshot)
  {
    ArgumentNullException.ThrowIfNull(projectiles);
    if (player == byte.MaxValue || !ownerEpoch.IsValid || !IsWorldScopeCurrent())
    {
      snapshot = null;
      return false;
    }

    if (!_players.TryGetValue(player, out PlayerTrackers? trackers) || trackers.OwnerEpoch != ownerEpoch)
    {
      snapshot = new ProjectileTrackerSnapshotView(
        player,
        ownerEpoch,
        null,
        null,
        _revision,
        _commitVersion);
      return true;
    }

    ProjectileTrackerReference? piggyBank = ResolveCurrent(trackers.PiggyBank, projectiles);
    ProjectileTrackerReference? voidLens = ResolveCurrent(trackers.VoidLens, projectiles);
    snapshot = new ProjectileTrackerSnapshotView(
      player,
      ownerEpoch,
      piggyBank,
      voidLens,
      trackers.Revision,
      trackers.CommitVersion);
    return true;
  }

  public OwnerCommitResult RemoveEntity(CombatEntityIdentity entity)
  {
    return RemoveEntities([entity]);
  }

  public OwnerCommitResult RemoveEntities(IReadOnlyCollection<CombatEntityIdentity> entities)
  {
    ArgumentNullException.ThrowIfNull(entities);
    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Projectile trackers must be cleared after world-scope invalidation before entity cleanup.");
    }

    var identities = new HashSet<CombatEntityIdentity>();
    foreach (CombatEntityIdentity entity in entities)
    {
      if (entity.IsValid)
      {
        identities.Add(entity);
      }
    }

    var affected = new List<(byte Player, PlayerTrackers Trackers)>();
    foreach (KeyValuePair<byte, PlayerTrackers> entry in _players)
    {
      if (Matches(entry.Value.PiggyBank, identities) || Matches(entry.Value.VoidLens, identities))
      {
        affected.Add((entry.Key, entry.Value));
      }
    }

    if (affected.Count == 0)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!CanAdvanceCommit())
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile tracker version range is exhausted.");
    }

    CommitVersion nextVersion = _commitVersion.Next();
    foreach ((byte player, PlayerTrackers trackers) in affected)
    {
      ProjectileTrackerReference? piggyBank = Matches(trackers.PiggyBank, identities)
        ? null
        : trackers.PiggyBank;
      ProjectileTrackerReference? voidLens = Matches(trackers.VoidLens, identities)
        ? null
        : trackers.VoidLens;
      _players[player] = trackers with
      {
        PiggyBank = piggyBank,
        VoidLens = voidLens,
        Revision = _revision + 1,
        CommitVersion = nextVersion
      };
    }

    CommitMutation(nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  public OwnerCommitResult RemoveSession(SessionEpoch ownerEpoch)
  {
    if (!IsWorldScopeCurrent())
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.StaleEntity,
        "Projectile trackers must be cleared after world-scope invalidation before session cleanup.");
    }

    var removedPlayers = new List<byte>();
    foreach (KeyValuePair<byte, PlayerTrackers> entry in _players)
    {
      if (entry.Value.OwnerEpoch == ownerEpoch)
      {
        removedPlayers.Add(entry.Key);
      }
    }

    if (removedPlayers.Count == 0)
    {
      return OwnerCommitResult.Applied(_commitVersion);
    }

    if (!CanAdvanceCommit())
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile tracker version range is exhausted.");
    }

    foreach (byte player in removedPlayers)
    {
      _players.Remove(player);
    }

    CommitVersion nextVersion = _commitVersion.Next();
    CommitMutation(nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  public OwnerCommitResult ClearAfterWorldScopeInvalidation()
  {
    long currentWorldScope = _identityAuthority.CurrentWorldScope;
    if (currentWorldScope <= 0 || currentWorldScope == _worldScope)
    {
      return OwnerCommitResult.Rejected(
        ProtocolErrorCode.Conflict,
        "Projectile tracker cleanup requires a changed, active world scope.");
    }

    foreach (PlayerTrackers trackers in _players.Values)
    {
      if (IsActive(trackers.PiggyBank) || IsActive(trackers.VoidLens))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Conflict,
          "Old-world projectile tracker identities must be invalidated before cleanup.");
      }
    }

    if (!CanAdvanceCommit())
    {
      return OwnerCommitResult.Rejected(ProtocolErrorCode.NotReady, "Projectile tracker version range is exhausted.");
    }

    _players.Clear();
    _worldScope = currentWorldScope;
    CommitVersion nextVersion = _commitVersion.Next();
    CommitMutation(nextVersion);
    return OwnerCommitResult.Applied(nextVersion);
  }

  private static bool Matches(
    ProjectileTrackerReference? reference,
    HashSet<CombatEntityIdentity> identities)
  {
    return reference is not null && identities.Contains(reference.Entity);
  }

  private bool IsCurrent(ProjectileTrackerReference? reference)
  {
    return reference is null || reference.Entity.WorldScope == _worldScope &&
      _identityAuthority.IsActive(reference.Entity);
  }

  private bool IsActive(ProjectileTrackerReference? reference)
  {
    return reference is not null && _identityAuthority.IsActive(reference.Entity);
  }

  private static ProjectileTrackerReference? ResolveCurrent(
    ProjectileTrackerReference? reference,
    ProjectileManager projectiles)
  {
    if (reference is null || !projectiles.TryGet(reference.Entity, out ProjectileView? projectile) ||
        projectile is null || projectile.Owner != reference.Owner ||
        projectile.ProtocolIdentity != reference.Identity || projectile.Type != reference.Type)
    {
      return null;
    }

    return reference;
  }

  private bool CanAdvanceCommit()
  {
    return _revision < long.MaxValue && _commitVersion.Value < long.MaxValue;
  }

  private void CommitMutation(CommitVersion nextVersion)
  {
    _revision++;
    _commitVersion = nextVersion;
  }

  private bool IsWorldScopeCurrent()
  {
    return _worldScope > 0 && _identityAuthority.CurrentWorldScope == _worldScope;
  }
}
