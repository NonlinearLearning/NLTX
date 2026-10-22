using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Inventory.Rules;
using MinimalImplementation.Server.Player;
using MinimalImplementation.Server.Session;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Inventory.State;

/// <summary>Prepares balanced player debits and world-item output as one transaction.</summary>
public sealed class WorldItemDropCoordinator
{
  private readonly InventoryStore _inventories;
  private readonly WorldItemStore _items;
  private readonly WorldItemReservationStore _reservations;
  private readonly WorldItemTransactionCoordinator _transactions;
  private readonly ItemValidationPolicy _itemPolicy;
  private readonly PlayerOwner _players;
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly WorldOwner _world;

  public WorldItemDropCoordinator(
    InventoryStore inventories,
    WorldItemStore items,
    WorldItemReservationStore reservations,
    ItemValidationPolicy itemPolicy,
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    WorldOwner world)
  {
    ArgumentNullException.ThrowIfNull(inventories);
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(reservations);
    ArgumentNullException.ThrowIfNull(itemPolicy);
    ArgumentNullException.ThrowIfNull(players);
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(world);
    _transactions = new WorldItemTransactionCoordinator(inventories, items, reservations);
    _inventories = inventories;
    _items = items;
    _reservations = reservations;
    _itemPolicy = itemPolicy;
    _players = players;
    _sessions = sessions;
    _world = world;
  }

  public OwnerCommitResult Prepare(
    WorldScopeId scope,
    WorldHeaderCapture header,
    DateTimeOffset now,
    IWorldItemDropPreparation drop,
    IReadOnlyList<IWorldItemTransactionParticipant> participants,
    out WorldItemTransactionCoordinator.PreparedTransaction? transaction,
    bool echoInventoryToOwner = true)
  {
    ArgumentNullException.ThrowIfNull(header);
    ArgumentNullException.ThrowIfNull(drop);
    ArgumentNullException.ThrowIfNull(participants);
    transaction = null;
    lock (_items.CommitGate.SyncRoot)
    {
      if (!scope.IsValid || !drop.PlayerHandle.Id.IsValid
        || !drop.PlayerHandle.WorldScope.IsValid || !drop.PlayerHandle.Generation.IsValid
        || !drop.Epoch.IsValid || !drop.PlayerVersion.IsValid || !drop.SourceRevision.IsValid
        || !drop.InventoryRevision.IsValid || drop.WorldEpoch == 0
        || !drop.WorldVersion.IsValid || drop.PlayerHandle.WorldScope != scope)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The player drop identity or captured revisions are invalid.");
      }

      if (!_world.IsReady)
      {
        return Unavailable("The authoritative world is not ready for item drops.");
      }

      if (drop.WorldEpoch != header.WorldEpoch || drop.WorldVersion != header.Version
        || !_world.IsCurrentWorldRevision(header.WorldEpoch, header.Version))
      {
        return Conflict("The drop's world capture is not the current published revision.");
      }

      if (!_sessions.TryGetStage(drop.Epoch, out SessionStage stage)
        || stage != SessionStage.Active
        || !_sessions.TryGetBinding(drop.Epoch, out SessionBinding? binding)
        || binding is null || binding.PlayerHandle != drop.PlayerHandle
        || !_players.TryGetBoundPlayer(drop.Epoch, _sessions, out Player.Player? player)
        || player is null || player.IsDead || player.Version != drop.PlayerVersion
        || player.Handle != drop.PlayerHandle || player.Epoch != drop.Epoch)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.StaleEpoch, "The drop does not belong to the current active player.");
      }

      WorldItemChange[] changes = drop.WorldChanges?.ToArray() ?? [];
      WorldItemSpawn[] spawns = drop.Spawns?.ToArray() ?? [];
      InventoryMutation[] debits = drop.InventoryDebits?.ToArray() ?? [];
      WorldItemHandle[] releaseClaims = drop.ReleaseClaims?.ToArray() ?? [];
      if (releaseClaims.Any(handle => !handle.IsValid || handle.WorldScope != scope
        || !_reservations.IsReservedBy(handle, drop.PlayerHandle.Id, drop.Epoch, now)))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "A drop can only release an active claim owned by its player and session.");
      }

      IWorldItemTransactionParticipant[] otherParticipants = participants.ToArray();
      var participantSet = new HashSet<IWorldItemTransactionParticipant>(
        ReferenceEqualityComparer.Instance);
      bool hasInvalidParticipant = otherParticipants.Any(participant => participant is null
        || ReferenceEquals(participant, drop) || !participantSet.Add(participant));
      if (hasInvalidParticipant
        || changes.Length + spawns.Length == 0 || debits.Length == 0)
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "A drop requires distinct participants, item output and inventory debits.");
      }

      if (!_inventories.TryCapture(drop.PlayerHandle.Id, out InventorySnapshot? inventory)
        || inventory is null)
      {
        return Unavailable("The player's authoritative inventory is not available.");
      }

      if (inventory.Revision != drop.InventoryRevision)
      {
        return Conflict("The inventory changed after the drop was prepared.");
      }

      if (!TryCaptureBalancedDrop(
        scope, inventory, changes, spawns, debits, releaseClaims,
        out Dictionary<ItemStackKey, long> debited,
        out Dictionary<ItemStackKey, long> produced,
        out OwnerCommitResult? failure))
      {
        return failure!;
      }

      if (!TotalsMatch(debited, produced))
      {
        return OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The drop output does not equal its authoritative inventory debit.");
      }

      var readGuard = new DropOwnerReadGuard(
        _players, _sessions, _world, player, binding, drop.PlayerVersion, header);
      IWorldItemTransactionParticipant[] allParticipants = otherParticipants
        .Append(drop)
        .Append(readGuard)
        .ToArray();
      var inventoryChange = new WorldItemInventoryChange(
        drop.PlayerHandle.Id, drop.Epoch, inventory.Revision, debits, echoInventoryToOwner);
      return _transactions.Prepare(
        changes, spawns, releaseClaims, inventoryChange, allParticipants, out transaction);
    }
  }

  private bool TryCaptureBalancedDrop(
    WorldScopeId scope,
    InventorySnapshot inventory,
    IReadOnlyList<WorldItemChange> changes,
    IReadOnlyList<WorldItemSpawn> spawns,
    IReadOnlyList<InventoryMutation> debits,
    IReadOnlyList<WorldItemHandle> releaseClaims,
    out Dictionary<ItemStackKey, long> debitTotals,
    out Dictionary<ItemStackKey, long> outputTotals,
    out OwnerCommitResult? failure)
  {
    debitTotals = [];
    outputTotals = [];
    failure = null;
    var seenSlots = new HashSet<short>();
    foreach (InventoryMutation debit in debits)
    {
      if (debit is null || debit.Item is null || debit.SlotId < 0
        || debit.SlotId >= inventory.Slots.Count || !seenSlots.Add(debit.SlotId))
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The inventory debit contains an invalid or repeated slot.");
        return false;
      }

      InventorySlotSnapshot slot = inventory.Slots[debit.SlotId];
      if (slot.Generation != debit.ExpectedGeneration || slot.Item.IsEmpty)
      {
        failure = Conflict("The inventory debit no longer matches its source slot generation.");
        return false;
      }

      if (!_itemPolicy.IsValid(debit.Item))
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The inventory debit contains an invalid item.");
        return false;
      }

      if (debit.Item.IsEmpty)
      {
        AddTotal(debitTotals, new ItemStackKey(slot.Item.Type, slot.Item.Prefix), slot.Item.Stack);
        continue;
      }

      if (debit.Item.Type != slot.Item.Type || debit.Item.Prefix != slot.Item.Prefix
        || debit.Item.Favorited != slot.Item.Favorited
        || debit.Item.Stack >= slot.Item.Stack)
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "A drop debit may only reduce the existing item stack.");
        return false;
      }

      AddTotal(debitTotals, new ItemStackKey(slot.Item.Type, slot.Item.Prefix),
        slot.Item.Stack - debit.Item.Stack);
    }

    var changedHandles = new HashSet<WorldItemHandle>();
    foreach (WorldItemChange change in changes)
    {
      if (change is null)
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The drop contains a null world-item change.");
        return false;
      }

      WorldItemMutation? mutation = change.Mutation;
      if (!change.Handle.IsValid || change.Handle.WorldScope != scope
        || mutation is null || mutation.Item is null || !changedHandles.Add(change.Handle))
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "The drop contains an invalid, repeated or out-of-scope world-item change.");
        return false;
      }

      if (!_items.TryCapture(change.Handle.SlotId, out WorldItemSnapshot? before)
        || before is null || before.Handle != change.Handle
        || before.Revision != change.ExpectedRevision)
      {
        failure = Conflict("A changed world item is no longer current.");
        return false;
      }

      InventoryItemState after = mutation.Item;
      if (!_itemPolicy.IsValid(after))
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The drop contains an invalid world-item stack.");
        return false;
      }

      if (after.IsEmpty
        || after.Type != before.Item.Type || after.Prefix != before.Item.Prefix
        || after.Stack <= before.Item.Stack)
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden,
          "A drop may only increase a matching world-item stack.");
        return false;
      }

      AddTotal(outputTotals, new ItemStackKey(after.Type, after.Prefix),
        after.Stack - before.Item.Stack);
    }

    if (releaseClaims.Any(handle => !handle.IsValid || handle.WorldScope != scope)
      || releaseClaims.Distinct().Count() != releaseClaims.Count
      || releaseClaims.Any(handle => !changedHandles.Contains(handle)))
    {
      failure = OwnerCommitResult.Rejected(
        ProtocolErrorCode.Forbidden,
        "Drop claim releases must be distinct targets changed by the same operation.");
      return false;
    }

    foreach (WorldItemSpawn spawn in spawns)
    {
      if (spawn is null || spawn.WorldScope != scope || spawn.InitialState is null
        || spawn.InitialState.Item is null || !_itemPolicy.IsValid(spawn.InitialState.Item)
        || spawn.InitialState.Item.IsEmpty)
      {
        failure = OwnerCommitResult.Rejected(
          ProtocolErrorCode.Forbidden, "The drop contains an invalid world-item spawn.");
        return false;
      }

      AddTotal(outputTotals,
        new ItemStackKey(spawn.InitialState.Item.Type, spawn.InitialState.Item.Prefix),
        spawn.InitialState.Item.Stack);
    }

    if (debitTotals.Count == 0 || outputTotals.Count == 0)
    {
      failure = OwnerCommitResult.Rejected(
        ProtocolErrorCode.Forbidden, "The drop must transfer at least one item unit.");
      return false;
    }

    return true;
  }

  private static void AddTotal(
    IDictionary<ItemStackKey, long> totals, ItemStackKey key, int amount)
  {
    totals.TryGetValue(key, out long current);
    totals[key] = current + amount;
  }

  private static bool TotalsMatch(
    IReadOnlyDictionary<ItemStackKey, long> debited,
    IReadOnlyDictionary<ItemStackKey, long> produced)
  {
    return debited.Count == produced.Count
      && debited.All(pair => produced.TryGetValue(pair.Key, out long amount)
        && amount == pair.Value);
  }

  private static OwnerCommitResult Conflict(string detail) => new(
    OwnerCommitStatus.Conflict, null, ProtocolErrorCode.Conflict, detail);

  private static OwnerCommitResult Unavailable(string detail) => new(
    OwnerCommitStatus.Unavailable, null, ProtocolErrorCode.NotReady, detail);

  private readonly record struct ItemStackKey(int Type, byte Prefix);

  private sealed class DropOwnerReadGuard(
    PlayerOwner players,
    SessionAdmissionCoordinator sessions,
    WorldOwner world,
    Player.Player player,
    SessionBinding binding,
    CommitVersion playerVersion,
    WorldHeaderCapture header) : IWorldItemTransactionParticipant
  {
    public OwnerCommitResult Validate()
    {
      if (!world.IsReady
        || !world.IsCurrentWorldRevision(header.WorldEpoch, header.Version)
        || !sessions.TryGetStage(binding.Epoch, out SessionStage stage)
        || stage != SessionStage.Active
        || !sessions.TryGetBinding(binding.Epoch, out SessionBinding? currentBinding)
        || currentBinding != binding
        || !players.TryGet(binding.PlayerHandle, out Player.Player? currentPlayer)
        || currentPlayer is null
        || !ReferenceEquals(currentPlayer, player) || currentPlayer.IsDead
        || currentPlayer.Epoch != binding.Epoch
        || currentPlayer.Handle != binding.PlayerHandle
        || currentPlayer.Version != playerVersion)
      {
        return Conflict("The player, session or world changed after the drop was prepared.");
      }

      return OwnerCommitResult.Applied(header.Version);
    }

    public void Commit()
    {
    }
  }
}
