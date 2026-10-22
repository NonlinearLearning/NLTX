using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Session;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Commits world items, claims and inventory as one exclusive state transition.</summary>
public sealed class WorldItemTransactionCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemStore _items;
  private readonly WorldItemReservationStore _reservations;

  internal WorldItemStore Items => _items;
  internal InventoryStore Inventories => _inventories;
  internal WorldItemReservationStore Reservations => _reservations;

  public WorldItemTransactionCoordinator(
    InventoryStore inventories, WorldItemStore items, WorldItemReservationStore reservations)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(reservations);
    if (!ReferenceEquals(inventories.CommitGate, items.CommitGate)
      || !ReferenceEquals(inventories.CommitGate, reservations.CommitGate))
    {
      throw new ArgumentException("Inventory, world items and claims must share one commit gate.");
    }

    _inventories = inventories;
    _items = items;
    _reservations = reservations;
  }

  public OwnerCommitResult Prepare(
    IReadOnlyList<WorldItemChange> changes,
    IReadOnlyList<WorldItemSpawn> spawns,
    IReadOnlyList<WorldItemHandle> releaseClaims,
    WorldItemInventoryChange? inventory,
    IReadOnlyList<IWorldItemTransactionParticipant> participants,
    out PreparedTransaction? transaction)
  {
    return Prepare(changes, spawns, releaseClaims, [], inventory, participants, out transaction);
  }

  public OwnerCommitResult Prepare(
    IReadOnlyList<WorldItemChange> changes,
    IReadOnlyList<WorldItemSpawn> spawns,
    IReadOnlyList<WorldItemHandle> releaseClaims,
    IReadOnlyList<WorldItemOwnershipChange> ownershipChanges,
    WorldItemInventoryChange? inventory,
    IReadOnlyList<IWorldItemTransactionParticipant> participants,
    out PreparedTransaction? transaction)
  {
    ArgumentNullException.ThrowIfNull(changes);
    ArgumentNullException.ThrowIfNull(spawns);
    ArgumentNullException.ThrowIfNull(releaseClaims);
    ArgumentNullException.ThrowIfNull(ownershipChanges);
    ArgumentNullException.ThrowIfNull(participants);
    transaction = null;
    lock (_items.CommitGate.SyncRoot)
    {
      IWorldItemTransactionParticipant[] preparedParticipants = participants.ToArray();
      var participantSet = new HashSet<IWorldItemTransactionParticipant>(
        ReferenceEqualityComparer.Instance);
      if (preparedParticipants.Any(participant => participant is null
          || !participantSet.Add(participant))
        || inventory is not null && (!inventory.PlayerId.IsValid || !inventory.Epoch.IsValid
          || !inventory.ExpectedRevision.IsValid || inventory.Mutations is null
          || inventory.Mutations.Count == 0 || changes.Count + spawns.Count == 0))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "Transaction participants must be non-null and unique; target session must be valid.");
      }

      OwnerCommitResult result = _items.PrepareBatch(changes, spawns, out var world);
      if (world is null)
      {
        return result;
      }

      result = ValidatePickupParticipants(
        preparedParticipants, changes, world, inventory,
        out WorldScopeId? pickupScope,
        out SessionPacketProjection[] playerProjections);
      if (result.Status != OwnerCommitStatus.Applied)
      {
        return result;
      }

      if (inventory is not null && !pickupScope.HasValue
        && !TryGetSingleWorldScope(world, out pickupScope))
      {
        return Unavailable("The inventory projection has no single authoritative world scope.");
      }

      // Retiring an item always retires its claim. Additional releases guard merge/pickup claims.
      WorldItemHandle[] handles = releaseClaims.Concat(world.Before
        .Where(item => world.Retired.Contains(item.Handle))
        .Select(item => item.Handle)).Distinct().ToArray();
      if (handles.Any(handle => !handle.IsValid)
        || ownershipChanges.Any(change => change is null || change.Item is null
          || world.Before.Any(item => item.Handle == change.Item.Handle)
          || change.Owner is not null && !participantSet.Contains(change.Owner))
        || !_reservations.PrepareChanges(handles, ownershipChanges, out var claims))
      {
        return Conflict("The world-item claims cannot be prepared.");
      }

      InventoryStore.PreparedMutation? slots = null;
      if (inventory is not null)
      {
        result = _inventories.PrepareApply(inventory.PlayerId, inventory.ExpectedRevision,
          inventory.Mutations, out slots);
        if (slots is null)
        {
          return result;
        }
      }

      var prepared = new PreparedTransaction(this, world, claims, slots,
        inventory?.Epoch, preparedParticipants, pickupScope, playerProjections,
        ownershipChanges.Select(change => change.Item).ToArray(), inventory?.EchoToOwner ?? true);
      result = prepared.Validate();
      if (result.Status == OwnerCommitStatus.Applied)
      {
        transaction = prepared;
      }

      return result;
    }
  }

  private static OwnerCommitResult Conflict(string detail) => new(
    OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private static OwnerCommitResult ValidatePickupParticipants(
    IReadOnlyList<IWorldItemTransactionParticipant> participants,
    IReadOnlyList<WorldItemChange> changes,
    WorldItemStore.PreparedBatch world,
    WorldItemInventoryChange? inventory,
    out WorldScopeId? pickupScope,
    out SessionPacketProjection[] playerProjections)
  {
    pickupScope = null;
    playerProjections = [];
    IWorldItemPickupPreparation[] pickups = participants
      .OfType<IWorldItemPickupPreparation>().ToArray();
    if (pickups.Length == 0)
    {
      return OwnerCommitResult.Applied(
        world.After.Count == 0 ? CommitVersion.Initial : world.After[0].Revision);
    }

    var itemHandles = new HashSet<WorldItemHandle>();
    var projections = new List<SessionPacketProjection>();
    foreach (IWorldItemPickupPreparation pickup in pickups)
    {
      if (pickup is null || !pickup.PlayerHandle.Id.IsValid
        || !pickup.PlayerHandle.WorldScope.IsValid
        || !pickup.PlayerHandle.Generation.IsValid || !pickup.Epoch.IsValid
        || !pickup.ItemHandle.IsValid || !pickup.ItemRevision.IsValid
        || pickup.WorldEpoch == 0 || pickup.Width <= 0 || pickup.Height <= 0
        || !pickup.CanAcceptItem || !itemHandles.Add(pickup.ItemHandle))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The pickup participant identity or item binding is invalid.");
      }

      WorldItemChange? change = changes.FirstOrDefault(candidate =>
        candidate.Handle == pickup.ItemHandle && candidate.ExpectedRevision == pickup.ItemRevision);
      WorldItemSnapshot? before = world.Before.FirstOrDefault(item =>
        item.Handle == pickup.ItemHandle && item.Revision == pickup.ItemRevision);
      WorldItemSnapshot? after = world.After.FirstOrDefault(item =>
        item.Handle == pickup.ItemHandle);
      if (change is null || before is null || after is null
        || !ConsumesItem(before, after, change.Mutation)
        || before.Handle.WorldScope != pickup.PlayerHandle.WorldScope)
      {
        return new OwnerCommitResult(OwnerCommitStatus.Conflict, null,
          ProtocolErrorCode.Conflict,
          "The pickup participant is not bound to a consumed current world item.");
      }

      if (pickup.Effect != WorldItemPickupEffect.ForItem(before.Item.Type))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The pickup effect does not match the authoritative item type.");
      }

      if (pickupScope.HasValue && pickupScope.Value != pickup.PlayerHandle.WorldScope)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "Pickup participants must belong to one world scope.");
      }

      pickupScope ??= pickup.PlayerHandle.WorldScope;
      if (inventory is not null && (inventory.PlayerId != pickup.PlayerHandle.Id
        || inventory.Epoch != pickup.Epoch))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "Inventory credit does not match the pickup participant.");
      }

      if (pickup.Effect != default && inventory is not null)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "An effect pickup cannot also consume an inventory debit or credit.");
      }

      if (pickup.Effect == default
        && (inventory is null || inventory.Mutations.Count == 0))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "A normal pickup must carry an authoritative inventory credit.");
      }

      if (pickup.Projections is null)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "Pickup projections must be captured before commit.");
      }

      foreach (SessionPacketProjection projection in pickup.Projections)
      {
        if (projection is null || projection.TargetEpochs.Count == 0
          || projection.TargetEpochs.Any(target => !target.IsValid)
          || projection.IgnoreEpoch is { } ignored && !ignored.IsValid
          || !projection.CommitVersion.IsValid)
        {
          return OwnerCommitResult.Rejected(
            ProtocolErrorCode.Forbidden, "A pickup projection contains an invalid target.");
        }

        projections.Add(projection);
      }
    }

    playerProjections = projections.ToArray();
    return OwnerCommitResult.Applied(
      inventory?.ExpectedRevision ?? (world.After.Count == 0
        ? CommitVersion.Initial : world.After[0].Revision));
  }

  private static bool ConsumesItem(
    WorldItemSnapshot before, WorldItemSnapshot after, WorldItemMutation? mutation)
  {
    if (mutation is null)
    {
      return after.Item.IsEmpty;
    }

    return mutation.Item.Stack > 0
      && mutation.Item.Stack < before.Item.Stack
      && after.Item == mutation.Item
      && after.Item == before.Item.WithStack(mutation.Item.Stack);
  }

  private static bool TryGetSingleWorldScope(
    WorldItemStore.PreparedBatch world, out WorldScopeId? scope)
  {
    WorldScopeId[] scopes = world.Before.Select(item => item.Handle.WorldScope)
      .Concat(world.After.Select(item => item.Handle.WorldScope)).Distinct().ToArray();
    scope = scopes.Length == 1 ? scopes[0] : null;
    return scope.HasValue;
  }

  public sealed class PreparedTransaction
  {
    private readonly WorldItemTransactionCoordinator _owner;
    private readonly WorldItemStore.PreparedBatch _world;
    private readonly WorldItemReservationStore.PreparedRelease _claims;
    private readonly InventoryStore.PreparedMutation? _inventory;
    private readonly SessionEpoch? _epoch;
    private readonly bool _echoInventoryToOwner;
    private readonly IWorldItemTransactionParticipant[] _participants;
    private readonly WorldItemSnapshot[] _ownershipItems;
    private readonly WorldItemHandle[] _ownershipProjections;
    private readonly int _privateOwnershipValues;
    private readonly WorldScopeId? _playerProjectionScope;
    private readonly SessionPacketProjection[] _playerProjections;
    private readonly WorldItemTransactionResult _result;
    private bool _committed;

    internal PreparedTransaction(
      WorldItemTransactionCoordinator owner, WorldItemStore.PreparedBatch world,
      WorldItemReservationStore.PreparedRelease claims,
      InventoryStore.PreparedMutation? inventory, SessionEpoch? epoch,
      IWorldItemTransactionParticipant[] participants,
      WorldScopeId? playerProjectionScope,
      SessionPacketProjection[] playerProjections, WorldItemSnapshot[] ownershipItems,
      bool echoInventoryToOwner)
    {
      _owner = owner;
      _world = world;
      _claims = claims;
      _inventory = inventory;
      _epoch = epoch;
      _echoInventoryToOwner = echoInventoryToOwner;
      _participants = participants;
      _ownershipItems = ownershipItems;
      _ownershipProjections = claims.Entries.Where(entry => entry.PublishOwnership)
        .Select(entry => entry.Handle).ToArray();
      _privateOwnershipValues = claims.Entries.Count(entry => entry.RequestRelease);
      _playerProjectionScope = playerProjectionScope;
      _playerProjections = playerProjections;
      CommitVersion version = inventory?.NextRevision
        ?? (world.After.Count == 0
          ? claims.Entries.LastOrDefault()?.Revision ?? CommitVersion.Initial
          : world.After[0].Revision);
      _result = new WorldItemTransactionResult(
        OwnerCommitResult.Applied(version), inventory?.Snapshot, world.After);
    }

    public OwnerCommitResult Validate()
    {
      lock (_owner._items.CommitGate.SyncRoot)
      {
        if (_committed || !_owner._items.CanCommit(_world)
          || !_owner._reservations.CanCommit(_claims)
          || _ownershipItems.Any(item =>
            !_owner._items.TryCapture(item.Handle.SlotId, out var current) || current is null
            || current.Handle != item.Handle || current.Revision != item.Revision)
          || _inventory is not null && !_owner._inventories.CanCommit(_inventory))
        {
          return Conflict("A transaction participant changed after preparation.");
        }

        int inventoryTargets = _inventory is { HasChanges: true } && _epoch.HasValue
          ? _owner._items.Projections.CountInventoryTargets(
            _playerProjectionScope!.Value, _epoch.Value, _echoInventoryToOwner) : 0;
        if (!_owner._items.Projections.CanPublish(_world.After, _ownershipProjections,
          inventoryTargets, _playerProjections, _world.ShimmerEffects, _privateOwnershipValues))
        {
          return new OwnerCommitResult(OwnerCommitStatus.Unavailable, null,
            ProtocolErrorCode.NotReady, "The projection sequence space is exhausted.");
        }

        foreach (IWorldItemTransactionParticipant participant in _participants)
        {
          OwnerCommitResult result = participant.Validate();
          if (result.Status != OwnerCommitStatus.Applied)
          {
            return result;
          }
        }

        return _result.Result;
      }
    }

    /// <summary>
    /// Host must retain its exclusive ServerLoop window across all owner commits.
    /// </summary>
    public WorldItemTransactionResult Commit()
    {
      lock (_owner._items.CommitGate.SyncRoot)
      {
        OwnerCommitResult validation = Validate();
        if (validation.Status != OwnerCommitStatus.Applied)
        {
          return new WorldItemTransactionResult(validation, null, []);
        }

        foreach (IWorldItemTransactionParticipant participant in _participants)
        {
          participant.Commit();
        }

        if (_inventory is not null)
        {
          _owner._inventories.CommitPrepared(_inventory, out _);
        }

        _owner._items.CommitPrepared(_world, publish: false);
        _owner._reservations.CommitPrepared(_claims, publish: false);
        _committed = true;
        // The journal is observable only after every authoritative store has committed.
        _owner._items.Publish(_world);
        _owner._reservations.Publish(_claims);
        if (_playerProjections.Length > 0)
        {
          _owner._items.Projections.PublishPlayer(
            _playerProjectionScope!.Value, _playerProjections);
        }
        if (_inventory is { HasChanges: true } && _epoch.HasValue)
        {
          _owner._items.Projections.PublishInventory(
            _playerProjectionScope!.Value, _epoch.Value, _inventory.Snapshot,
            _echoInventoryToOwner);
        }

        return _result;
      }
    }

  }
}
