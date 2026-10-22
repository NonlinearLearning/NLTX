using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Rules;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>World-owned checks that decide whether a player may interact with a chest.</summary>
public interface IChestAccessPolicy
{
  bool CanOpen(EntityUuid playerId, WorldScopeId worldScope, ChestSnapshot chest);
}

public sealed record ChestAccessResult(
  OwnerCommitResult Commit,
  ChestSnapshot? Chest,
  ChestSession? Session);

/// <summary>Coordinates chest access leases and validated slot updates on the server commit loop.</summary>
public sealed class ChestAccessService
{
  private readonly ChestStore _chests;
  private readonly ChestLeaseStore _leases;
  private readonly ItemValidationPolicy _items;
  private readonly IChestAccessPolicy _accessPolicy;
  private readonly ServerStateCommitGate _commitGate;
  private readonly TimeSpan _leaseDuration;

  public ChestAccessService(
    ChestStore chests,
    ChestLeaseStore leases,
    ItemValidationPolicy items,
    IChestAccessPolicy accessPolicy,
    TimeSpan leaseDuration)
  {
    ArgumentNullException.ThrowIfNull(chests);
    ArgumentNullException.ThrowIfNull(leases);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(accessPolicy);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(leaseDuration.Ticks);
    if (!ReferenceEquals(chests.CommitGate, leases.CommitGate))
    {
      throw new ArgumentException("Chest and lease stores must share one commit gate.", nameof(leases));
    }

    _chests = chests;
    _leases = leases;
    _items = items;
    _accessPolicy = accessPolicy;
    _commitGate = chests.CommitGate;
    _leaseDuration = leaseDuration;
  }

  public ChestAccessResult TryOpen(
    ChestId chestId,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!playerId.IsValid || !epoch.IsValid || !worldScope.IsValid)
      {
        return Failed(OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The player session or world scope is invalid."));
      }
  
      if (!_chests.TryCapture(chestId, out ChestSnapshot? chest) || chest is null)
      {
        return Failed(Unavailable("The chest is not available."));
      }
  
      if (chest.WorldScope != worldScope)
      {
        return new ChestAccessResult(Stale("The chest belongs to a different world scope."), chest, null);
      }
  
      if (!_accessPolicy.CanOpen(playerId, worldScope, chest))
      {
        return new ChestAccessResult(
          OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest is not accessible to this player."),
          chest,
          null);
      }
  
      ChestLeaseResult lease = _leases.TryAcquire(
        chest,
        playerId,
        epoch,
        now,
        _leaseDuration);
      if (lease.Status != OwnerCommitStatus.Applied || lease.Session is null)
      {
        return new ChestAccessResult(ToCommitResult(lease), chest, null);
      }
  
      return new ChestAccessResult(OwnerCommitResult.Applied(chest.Revision), chest, lease.Session);
    }
  }

  public ChestAccessResult TryClose(
    ChestId chestId,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      if (!_chests.TryCapture(chestId, out ChestSnapshot? chest) || chest is null)
      {
        return Failed(Unavailable("The chest is not available."));
      }
  
      if (!_leases.IsCurrent(
        chestId,
        worldScope,
        chest.Generation,
        playerId,
        epoch,
        now,
        out ChestSession? session)
        || session is null)
      {
        return new ChestAccessResult(Stale("The chest lease is no longer current."), chest, null);
      }
  
      if (!_leases.Release(chestId, chest.Generation, playerId, epoch, session.LeaseToken))
      {
        return new ChestAccessResult(Stale("The chest lease changed before it could be released."), chest, null);
      }
  
      return new ChestAccessResult(OwnerCommitResult.Applied(chest.Revision), chest, null);
    }
  }

  public ChestAccessResult TrySetSlot(
    ChestId chestId,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    PacketSequence sequence,
    int slotId,
    InventoryItemState item,
    DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentNullException.ThrowIfNull(item);
      if (!_items.IsValid(item))
      {
        return Failed(OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The chest slot update contains an invalid item."));
      }
  
      if (!_chests.TryCapture(chestId, out ChestSnapshot? chest) || chest is null)
      {
        return Failed(Unavailable("The chest is not available."));
      }
  
      if (chest.WorldScope != worldScope
        || !_leases.IsCurrent(
          chestId,
          worldScope,
          chest.Generation,
          playerId,
          epoch,
          now,
          out _))
      {
        return new ChestAccessResult(Stale("The chest lease or world scope is no longer current."), chest, null);
      }
  
      if (!_accessPolicy.CanOpen(playerId, worldScope, chest))
      {
        return new ChestAccessResult(
          OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest is no longer accessible to this player."),
          chest,
          null);
      }
  
      if (slotId < 0 || slotId >= chest.Slots.Count)
      {
        return new ChestAccessResult(
          OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest slot id is outside the active chest."),
          chest,
          null);
      }
  
      ChestSnapshot? committedChest = null;
      OwnerCommitResult result = _leases.ExecuteSequence(
        chestId,
        worldScope,
        chest.Generation,
        playerId,
        epoch,
        sequence,
        now,
        _leaseDuration,
        () => _chests.ApplyItems(
          chestId,
          worldScope,
          chest.Generation,
          chest.Revision,
          [new ChestItemMutation(slotId, chest.Slots[slotId].Generation, item)],
          out committedChest),
        out ChestSession? updatedSession);
  
      if (result.Status != OwnerCommitStatus.Applied
        && _chests.TryCapture(chestId, out ChestSnapshot? latestChest)
        && latestChest is not null)
      {
        chest = latestChest;
      }
  
      return new ChestAccessResult(result, committedChest ?? chest, updatedSession);
    }
  }

  public ChestAccessResult TrySetName(
    ChestId chestId,
    EntityUuid playerId,
    SessionEpoch epoch,
    WorldScopeId worldScope,
    PacketSequence sequence,
    string name,
    DateTimeOffset now)
  {
    lock (_commitGate.SyncRoot)
    {
      ArgumentNullException.ThrowIfNull(name);
      if (!_chests.TryCapture(chestId, out ChestSnapshot? chest) || chest is null)
      {
        return Failed(Unavailable("The chest is not available."));
      }

      if (chest.WorldScope != worldScope
        || !_leases.IsCurrent(
          chestId,
          worldScope,
          chest.Generation,
          playerId,
          epoch,
          now,
          out _))
      {
        return new ChestAccessResult(Stale("The chest lease or world scope is no longer current."), chest, null);
      }

      if (!_accessPolicy.CanOpen(playerId, worldScope, chest))
      {
        return new ChestAccessResult(
          OwnerCommitResult.Rejected(ProtocolErrorCode.Forbidden, "The chest is no longer accessible to this player."),
          chest,
          null);
      }

      ChestSnapshot? committedChest = null;
      OwnerCommitResult result = _leases.ExecuteSequence(
        chestId,
        worldScope,
        chest.Generation,
        playerId,
        epoch,
        sequence,
        now,
        _leaseDuration,
        () => _chests.SetName(
          chestId,
          worldScope,
          chest.Generation,
          chest.Revision,
          name,
          out committedChest),
        out ChestSession? updatedSession);

      if (result.Status != OwnerCommitStatus.Applied
        && _chests.TryCapture(chestId, out ChestSnapshot? latestChest)
        && latestChest is not null)
      {
        chest = latestChest;
      }

      return new ChestAccessResult(result, committedChest ?? chest, updatedSession);
    }
  }

  private static ChestAccessResult Failed(OwnerCommitResult result) => new(result, null, null);

  private static OwnerCommitResult ToCommitResult(ChestLeaseResult lease)
  {
    return new OwnerCommitResult(lease.Status, null, lease.ErrorCode, lease.Detail);
  }

  private static OwnerCommitResult Stale(string detail) =>
    new(OwnerCommitStatus.Stale, null, ProtocolErrorCode.StaleEntity, detail);

  private static OwnerCommitResult Unavailable(string detail) =>
    new(OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);
}
