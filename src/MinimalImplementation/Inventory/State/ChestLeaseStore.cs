using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Maintains one writable lease per chest and at most one active chest per session.</summary>
public sealed class ChestLeaseStore
{
  private readonly Dictionary<ChestId, ChestSession> _byChest = new();
  private readonly Dictionary<SessionEpoch, ChestId> _chestByEpoch = new();
  private readonly ServerStateCommitGate _commitGate;

  public ChestLeaseStore(ServerStateCommitGate commitGate)
  {
    ArgumentNullException.ThrowIfNull(commitGate);
    _commitGate = commitGate;
  }

  internal ServerStateCommitGate CommitGate => _commitGate;

  public ChestLeaseResult TryAcquire(
    ChestSnapshot chest,
    EntityUuid playerId,
    SessionEpoch epoch,
    DateTimeOffset now,
    TimeSpan leaseDuration)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentNullException.ThrowIfNull(chest);
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(leaseDuration.Ticks);
      if (!playerId.IsValid || !epoch.IsValid)
      {
        return ChestLeaseResult.Rejected(ProtocolErrorCode.Forbidden, "The player session is invalid.");
      }
  
      if (!TryAddDuration(now, leaseDuration, out DateTimeOffset expiresAt))
      {
        return ChestLeaseResult.Unavailable("The chest lease expiry is outside the supported clock range.");
      }
  
      ExpireAt(chest.Id, now);
      if (_byChest.TryGetValue(chest.Id, out ChestSession? current))
      {
        if (current.PlayerId == playerId
          && current.Epoch == epoch
          && current.WorldScope == chest.WorldScope
          && current.ChestGeneration == chest.Generation)
        {
          ChestSession renewed = current with { ExpiresAt = expiresAt };
          _byChest[chest.Id] = renewed;
          return ChestLeaseResult.Acquired(renewed);
        }
  
        return ChestLeaseResult.Conflict("The chest already has an active writer.");
      }
  
      if (_chestByEpoch.TryGetValue(epoch, out ChestId alreadyOpen))
      {
        ExpireAt(alreadyOpen, now);
        if (_chestByEpoch.ContainsKey(epoch))
        {
          return ChestLeaseResult.Conflict("The session must release its current chest before opening another.");
        }
      }
  
      var session = new ChestSession(
        playerId,
        epoch,
        chest.WorldScope,
        chest.Id,
        chest.Generation,
        Guid.NewGuid(),
        now,
        expiresAt,
        null);
      _byChest.Add(chest.Id, session);
      _chestByEpoch.Add(epoch, chest.Id);
      return ChestLeaseResult.Acquired(session);
    }
  }

  public bool IsCurrent(
    ChestId chestId,
    WorldScopeId worldScope,
    ChestGeneration generation,
    EntityUuid playerId,
    SessionEpoch epoch,
    DateTimeOffset now,
    out ChestSession? session)
  {
    lock (_commitGate.SyncRoot)
    {
      ExpireAt(chestId, now);
      if (_byChest.TryGetValue(chestId, out ChestSession? current)
        && current.WorldScope == worldScope
        && current.ChestGeneration == generation
        && current.PlayerId == playerId
        && current.Epoch == epoch)
      {
        session = current;
        return true;
      }
  
      session = null;
      return false;
    }
  }

  /// <summary>
  /// Checks the selected historical-candidate compatibility rule: an unlocked chest may be
  /// consumed when it is unused or already used by this same player session; another session's
  /// lease blocks it. The inspected Version4 implementation does not establish this behavior.
  /// </summary>
  public bool CanUseForCrafting(
    ChestSnapshot chest,
    EntityUuid playerId,
    SessionEpoch epoch,
    DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentNullException.ThrowIfNull(chest);
      if (!playerId.IsValid || !epoch.IsValid)
      {
        return false;
      }
  
      ExpireAt(chest.Id, now);
      if (!_byChest.TryGetValue(chest.Id, out ChestSession? current))
      {
        return true;
      }
  
      return current.WorldScope == chest.WorldScope
        && current.ChestGeneration == chest.Generation
        && current.PlayerId == playerId
        && current.Epoch == epoch;
    }
  }

  /// <summary>
  /// Returns whether a current chest lease belongs to a different authenticated session.
  /// Expired and generation-stale leases do not block the active chest.
  /// </summary>
  public bool IsHeldByDifferentSession(
    ChestSnapshot chest,
    EntityUuid playerId,
    SessionEpoch epoch,
    DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(chest);
    lock (_commitGate.SyncRoot)
    {
      ExpireAt(chest.Id, now);
      if (!_byChest.TryGetValue(chest.Id, out ChestSession? current))
      {
        return false;
      }

      if (current.WorldScope != chest.WorldScope
        || current.ChestGeneration != chest.Generation)
      {
        Remove(current);
        return false;
      }

      return current.PlayerId != playerId || current.Epoch != epoch;
    }
  }

  public ChestLeaseResult AdvanceSequence(
    ChestId chestId,
    ChestGeneration generation,
    EntityUuid playerId,
    SessionEpoch epoch,
    PacketSequence sequence,
    DateTimeOffset now,
    TimeSpan leaseDuration)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(leaseDuration.Ticks);
      if (!sequence.IsValid)
      {
        return ChestLeaseResult.Rejected(ProtocolErrorCode.MalformedFrame, "The packet sequence is invalid.");
      }
  
      if (!TryAddDuration(now, leaseDuration, out DateTimeOffset expiresAt))
      {
        return ChestLeaseResult.Unavailable("The chest lease expiry is outside the supported clock range.");
      }
  
      ExpireAt(chestId, now);
      if (!_byChest.TryGetValue(chestId, out ChestSession? current)
        || current.ChestGeneration != generation
        || current.PlayerId != playerId
        || current.Epoch != epoch)
      {
        return ChestLeaseResult.Stale("The chest lease is no longer current.");
      }
  
      if (current.LastSequence is { } last && sequence.Value <= last.Value)
      {
        return ChestLeaseResult.Conflict("The chest command sequence is not newer than the last committed sequence.");
      }
  
      ChestSession updated = current with
      {
        LastSequence = sequence,
        ExpiresAt = expiresAt
      };
      _byChest[chestId] = updated;
      return ChestLeaseResult.Acquired(updated);
    }
  }

  /// <summary>Commits one serialized chest command and advances its sequence only on success.</summary>
  public OwnerCommitResult ExecuteSequence(
    ChestId chestId,
    WorldScopeId worldScope,
    ChestGeneration generation,
    EntityUuid playerId,
    SessionEpoch epoch,
    PacketSequence sequence,
    DateTimeOffset now,
    TimeSpan leaseDuration,
    Func<OwnerCommitResult> commit,
    out ChestSession? session)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentNullException.ThrowIfNull(commit);
      session = null;
      if (!sequence.IsValid)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.MalformedFrame,
          "The packet sequence is invalid.");
      }
  
      if (leaseDuration.Ticks <= 0 || !TryAddDuration(now, leaseDuration, out DateTimeOffset expiresAt))
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Unavailable,
          null,
          ProtocolErrorCode.NotReady,
          "The chest lease duration is outside the supported clock range.");
      }
  
      ExpireAt(chestId, now);
      if (!_byChest.TryGetValue(chestId, out ChestSession? current)
        || current.WorldScope != worldScope
        || current.ChestGeneration != generation
        || current.PlayerId != playerId
        || current.Epoch != epoch)
      {
        return new OwnerCommitResult(
          OwnerCommitStatus.Stale,
          null,
          ProtocolErrorCode.StaleEntity,
          "The chest lease is no longer current.");
      }
  
      if (current.LastSequence is { } last && sequence.Value <= last.Value)
      {
        session = current;
        return new OwnerCommitResult(
          OwnerCommitStatus.Conflict,
          null,
          ProtocolErrorCode.Conflict,
          "The chest command sequence is not newer than the last committed sequence.");
      }
  
      OwnerCommitResult result = commit();
      if (result.Status == OwnerCommitStatus.Applied)
      {
        current = current with
        {
          LastSequence = sequence,
          ExpiresAt = expiresAt
        };
        _byChest[chestId] = current;
      }
  
      session = current;
      return result;
    }
  }

  public bool Release(
    ChestId chestId,
    ChestGeneration generation,
    EntityUuid playerId,
    SessionEpoch epoch,
    Guid leaseToken)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_byChest.TryGetValue(chestId, out ChestSession? current)
        || current.ChestGeneration != generation
        || current.PlayerId != playerId
        || current.Epoch != epoch
        || current.LeaseToken != leaseToken)
      {
        return false;
      }
  
      Remove(current);
      return true;
    }
  }

  public int ReleaseSession(SessionEpoch epoch)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_chestByEpoch.TryGetValue(epoch, out ChestId chestId)
        || !_byChest.TryGetValue(chestId, out ChestSession? current)
        || current.Epoch != epoch)
      {
        _chestByEpoch.Remove(epoch);
        return 0;
      }
  
      Remove(current);
      return 1;
    }
  }

  public int ReleaseWorldScope(WorldScopeId worldScope)
  {
    lock (_commitGate.SyncRoot)
    {
      ChestSession[] sessions = _byChest.Values
        .Where(session => session.WorldScope == worldScope)
        .ToArray();
      foreach (ChestSession session in sessions)
      {
        Remove(session);
      }
  
      return sessions.Length;
    }
  }

  public int Expire(DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      ChestSession[] expired = _byChest.Values
        .Where(session => session.ExpiresAt <= now)
        .ToArray();
      foreach (ChestSession session in expired)
      {
        Remove(session);
      }
  
      return expired.Length;
    }
  }

  private void ExpireAt(ChestId chestId, DateTimeOffset now)
  {
    if (_byChest.TryGetValue(chestId, out ChestSession? session) && session.ExpiresAt <= now)
    {
      Remove(session);
    }
  }

  private void Remove(ChestSession session)
  {
    if (_byChest.TryGetValue(session.ChestId, out ChestSession? current)
      && current.LeaseToken == session.LeaseToken)
    {
      _byChest.Remove(session.ChestId);
    }

    if (_chestByEpoch.TryGetValue(session.Epoch, out ChestId chestId)
      && chestId == session.ChestId)
    {
      _chestByEpoch.Remove(session.Epoch);
    }
  }

  private static bool TryAddDuration(
    DateTimeOffset now,
    TimeSpan duration,
    out DateTimeOffset result)
  {
    try
    {
      result = now.Add(duration);
      return true;
    }
    catch (ArgumentOutOfRangeException)
    {
      result = default;
      return false;
    }
  }
}
