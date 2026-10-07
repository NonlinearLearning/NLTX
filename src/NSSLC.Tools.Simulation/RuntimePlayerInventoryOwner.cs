using EntityEcs;
using EntityEcs.Components;
using Terraria.Items;
using Terraria.Relationships;
using Terraria.Player;
using ItemEntityRef = Terraria.Relationships.ItemEntityRef;

namespace Terraria.NonAuthoritative.SimulationHost;

internal sealed class RuntimePlayerInventoryOwner :
  IPlayerInventoryItemQuery,
  IPlayerInventoryEffectPort
{
  private static readonly IReadOnlyList<PlayerInventoryItemSnapshot> EmptyItems =
    Array.Empty<PlayerInventoryItemSnapshot>();

  private readonly EntityRuntime _entityRuntime;
  private readonly RuntimeEntityHandle _playerHandle;
  private readonly RuntimeItemRegistry _items;
  private readonly int _playerSlot;
  private readonly IPlayerInventoryEffectPort? _externalEffectPort;
  private readonly Dictionary<ItemEntityRef, CachedPickupResult> _pickupResults = [];
  private readonly Dictionary<Guid, ConsumeCommandResult> _consumeResults = [];
  private readonly Dictionary<Guid, SplitCommandResult> _splitResults = [];
  private List<ItemEntityRef>? _pendingReleaseItems;
  private long _nextCommandId;
  private bool _isReleased;

  public RuntimePlayerInventoryOwner(
    EntityRuntime entityRuntime,
    RuntimeEntityHandle playerHandle,
    RuntimeItemRegistry items,
    int playerSlot,
    IPlayerInventoryEffectPort? externalEffectPort = null)
  {
    _entityRuntime = entityRuntime ?? throw new ArgumentNullException(nameof(entityRuntime));
    _playerHandle = playerHandle;
    _items = items ?? throw new ArgumentNullException(nameof(items));
    if (!_items.IsBoundTo(_entityRuntime))
    {
      throw new ArgumentException(
        "The Player inventory owner and Item registry must share one EntityRuntime.",
        nameof(items));
    }

    if (!_entityRuntime.TryGetReference(
          _playerHandle,
          EntityReferenceScope.Player,
          out _) ||
        !_entityRuntime.Has<PlayerInventorySlotsComponent>(_playerHandle))
    {
      throw new ArgumentException(
        "The Player inventory owner requires a live Player root with inventory slots.",
        nameof(playerHandle));
    }

    ArgumentOutOfRangeException.ThrowIfNegative(playerSlot);
    _playerSlot = playerSlot;
    _externalEffectPort = externalEffectPort;
  }

  public int EffectIntentCount { get; private set; }

  /// <summary>Validates all owned Item roots without changing slots, relations, or release state.</summary>
  public bool TryValidateReleaseAllItems()
  {
    if (_isReleased)
    {
      return true;
    }

    if (_pendingReleaseItems is List<ItemEntityRef> pendingReleaseItems)
    {
      return pendingReleaseItems.All(_items.CanRemove);
    }

    return TryCaptureReleaseItems(out _);
  }

  public bool TryGetItemAtSlot(int slotIndex, out PlayerInventoryItemSnapshot snapshot)
  {
    if ((uint)slotIndex >= PlayerInventorySlotsComponent.MainInventorySlotCount ||
        !IsRootAvailable() ||
        !_entityRuntime.TryCapture<PlayerInventorySlotsComponent, ItemEntityRef>(
          _playerHandle,
          slots => slots.MainInventorySlots[slotIndex],
          out ItemEntityRef item) ||
        !_items.TryGet(item, out snapshot))
    {
      snapshot = default;
      return false;
    }

    if (!HasInventoryRelation(item, ItemInventorySlotKind.MainInventory, slotIndex))
    {
      snapshot = default;
      return false;
    }

    return true;
  }

  public void ReleaseAllItems()
  {
    if (_isReleased)
    {
      return;
    }

    if (_pendingReleaseItems is null && !IsRootAvailable())
    {
      _isReleased = true;
      return;
    }

    if (_pendingReleaseItems is null)
    {
      if (!TryCaptureReleaseItems(out List<ItemEntityRef> items))
      {
        throw new InvalidOperationException(
          "The Player inventory and its item roots must be available before release.");
      }

      if (!_entityRuntime.TryEdit<PlayerInventorySlotsComponent>(
            _playerHandle,
            (ref PlayerInventorySlotsComponent slots) =>
            {
              Array.Clear(slots.MainInventorySlots);
              Array.Clear(slots.InventoryChestStackMarkers);
              slots.ClearTrashItem();
            }))
      {
        throw new InvalidOperationException("The Player inventory could not be cleared.");
      }

      _pendingReleaseItems = items;
    }

    List<Exception>? cleanupFailures = null;
    List<ItemEntityRef> pendingReleaseItems = _pendingReleaseItems ??
      throw new InvalidOperationException("The pending inventory release state is unavailable.");
    foreach (ItemEntityRef item in pendingReleaseItems.ToArray())
    {
      if (_items.Remove(item))
      {
        pendingReleaseItems.Remove(item);
      }
      else
      {
        (cleanupFailures ??= []).Add(
          new InvalidOperationException("An inventory item root could not be removed."));
      }
    }

    if (cleanupFailures is not null)
    {
      throw new AggregateException(
        "Some inventory item roots could not be removed after the Player slots were cleared.",
        cleanupFailures);
    }

    _pendingReleaseItems = null;
    _isReleased = true;
    _pickupResults.Clear();
    _consumeResults.Clear();
    _splitResults.Clear();
  }

  public void InitializeStartingLoadout()
  {
    if (!IsRootAvailable())
    {
      return;
    }

    ItemEntityRef[] initialSlots = CaptureMainSlots();
    if (!initialSlots[0].IsEmpty || !initialSlots[54].IsEmpty)
    {
      return;
    }

    var createdItems = new List<ItemEntityRef>(2);
    try
    {
      PlayerInventoryItemSnapshot bow = _items.Create(typeId: 39, stack: 1);
      createdItems.Add(bow.Entity);
      PlayerInventoryItemSnapshot arrows = _items.Create(typeId: 40, stack: 99);
      createdItems.Add(arrows.Entity);

      bool committed = EditSlots(slots =>
      {
        if (!slots.MainInventorySlots[0].IsEmpty ||
            !slots.MainInventorySlots[54].IsEmpty)
        {
          return false;
        }

        if (!TrySetInventoryRelation(bow.Entity, ItemInventorySlotKind.MainInventory, 0) ||
            !TrySetInventoryRelation(arrows.Entity, ItemInventorySlotKind.MainInventory, 54))
        {
          return false;
        }

        slots.MainInventorySlots[0] = bow.Entity;
        slots.MainInventorySlots[54] = arrows.Entity;
        return true;
      });
      if (!committed)
      {
        throw new InvalidOperationException("The starting loadout could not be committed.");
      }
    }
    catch (Exception loadoutException)
    {
      try
      {
        RemoveCreatedItems(createdItems);
      }
      catch (Exception cleanupException)
      {
        throw new AggregateException(
          "Starting loadout initialization failed and its new item roots could not be removed.",
          loadoutException,
          cleanupException);
      }

      throw;
    }
  }

  public void PrepareWorldItemPickupProbeInventory(bool fillEverySlot)
  {
    if (!IsRootAvailable())
    {
      return;
    }

    ItemEntityRef[] originalSlots = CaptureMainSlots();
    if (!ValidateMainInventoryRelations(originalSlots, requireRemovable: fillEverySlot))
    {
      throw new InvalidOperationException("The pickup probe inventory has an invalid Item relation.");
    }

    if (!fillEverySlot)
    {
      int emptySlot = Array.FindIndex(originalSlots, static item => item.IsEmpty);
      if (emptySlot < 0)
      {
        throw new InvalidOperationException(
          "The pickup probe could not reserve an inventory slot for its partial stack.");
      }

      PlayerInventoryItemSnapshot partialStack = _items.Create(typeId: 23, stack: 9_998);
      bool committed = false;
      try
      {
        committed = EditSlots(slots =>
        {
          if (!slots.MainInventorySlots[emptySlot].IsEmpty)
          {
            return false;
          }

          if (!TrySetInventoryRelation(
                partialStack.Entity,
                ItemInventorySlotKind.MainInventory,
                emptySlot))
          {
            return false;
          }

          slots.MainInventorySlots[emptySlot] = partialStack.Entity;
          return true;
        });
        if (!committed)
        {
          throw new InvalidOperationException("The partial pickup probe stack could not be committed.");
        }
      }
      catch (Exception setupException)
      {
        if (!committed && !_items.Remove(partialStack.Entity))
        {
          throw new AggregateException(
            "The partial pickup probe failed and its item root could not be removed.",
            setupException,
            new InvalidOperationException("The partial pickup probe root remains live."));
        }

        throw;
      }

      return;
    }

    var replacementItems = new List<PlayerInventoryItemSnapshot>(originalSlots.Length);
    try
    {
      for (int slotIndex = 0; slotIndex < originalSlots.Length; slotIndex++)
      {
        replacementItems.Add(_items.CreateMaximumStack(typeId: 23));
      }

      bool committed = EditSlots(slots =>
      {
        if (!slots.MainInventorySlots.SequenceEqual(originalSlots))
        {
          return false;
        }

        for (int slotIndex = 0; slotIndex < replacementItems.Count; slotIndex++)
        {
          if (!TrySetInventoryRelation(
                replacementItems[slotIndex].Entity,
                ItemInventorySlotKind.MainInventory,
                slotIndex))
          {
            return false;
          }
        }

        for (int slotIndex = 0; slotIndex < replacementItems.Count; slotIndex++)
        {
          slots.MainInventorySlots[slotIndex] = replacementItems[slotIndex].Entity;
        }

        return true;
      });
      if (!committed)
      {
        throw new InvalidOperationException("The full pickup probe inventory could not be committed.");
      }
    }
    catch (Exception setupException)
    {
      try
      {
        RemoveCreatedItems(replacementItems.Select(static item => item.Entity));
      }
      catch (Exception cleanupException)
      {
        throw new AggregateException(
          "Full pickup probe setup failed and its replacement roots could not be removed.",
          setupException,
          cleanupException);
      }

      throw;
    }

    RemoveCreatedItems(originalSlots);
  }

  public int CountItem(int typeId)
  {
    return ReadSlots(slots =>
    {
      int count = 0;
      for (int slotIndex = 0; slotIndex < slots.MainInventorySlots.Length; slotIndex++)
      {
        ItemEntityRef entity = slots.MainInventorySlots[slotIndex];
        if (entity.IsEmpty)
        {
          continue;
        }

        if (!HasInventoryRelation(entity, ItemInventorySlotKind.MainInventory, slotIndex) ||
            !_items.TryGet(entity, out PlayerInventoryItemSnapshot item))
        {
          throw new InvalidOperationException("A Player inventory slot has no matching Item relation.");
        }

        if (item.TypeId == typeId)
        {
          count = checked(count + item.Stack);
        }
      }

      return count;
    });
  }

  public int CountUsedSlots()
  {
    return ReadSlots(slots =>
    {
      int count = 0;
      for (int slotIndex = 0; slotIndex < slots.MainInventorySlots.Length; slotIndex++)
      {
        ItemEntityRef item = slots.MainInventorySlots[slotIndex];
        if (item.IsEmpty)
        {
          continue;
        }

        if (!HasInventoryRelation(item, ItemInventorySlotKind.MainInventory, slotIndex))
        {
          throw new InvalidOperationException("A Player inventory slot has no matching Item relation.");
        }

        count++;
      }

      return count;
    });
  }

  public bool TrySplit(
    Guid commandId,
    int sourceSlotIndex,
    int amount,
    int destinationSlotIndex)
  {
    if (commandId == Guid.Empty ||
        amount <= 0 ||
        (uint)sourceSlotIndex >= PlayerInventorySlotsComponent.MainInventorySlotCount ||
        (uint)destinationSlotIndex >= PlayerInventorySlotsComponent.MainInventorySlotCount ||
        sourceSlotIndex == destinationSlotIndex)
    {
      return false;
    }

    if (_splitResults.TryGetValue(commandId, out SplitCommandResult previousResult))
    {
      return previousResult.SourceSlotIndex == sourceSlotIndex &&
        previousResult.DestinationSlotIndex == destinationSlotIndex &&
        previousResult.Amount == amount &&
        previousResult.Applied;
    }

    if (!IsRootAvailable() ||
        !TryGetItemAtSlot(sourceSlotIndex, out PlayerInventoryItemSnapshot source) ||
        source.IsUniqueStack ||
        source.MaximumStack <= 1 ||
        amount >= source.Stack ||
        !ReadSlots(slots => slots.MainInventorySlots[destinationSlotIndex].IsEmpty))
    {
      _splitResults.Add(
        commandId,
        new SplitCommandResult(sourceSlotIndex, destinationSlotIndex, amount, false));
      return false;
    }

    if (!_items.TryCreateSplitItem(source, amount, out PlayerInventoryItemSnapshot splitItem))
    {
      _splitResults.Add(
        commandId,
        new SplitCommandResult(sourceSlotIndex, destinationSlotIndex, amount, false));
      return false;
    }

    bool sourceWasUpdated = false;
    bool sourceWasRestored = false;
    bool splitRootWasRemoved = false;
    try
    {
      bool committed = EditSlots(slots =>
      {
        if (slots.MainInventorySlots[sourceSlotIndex] != source.Entity ||
            !slots.MainInventorySlots[destinationSlotIndex].IsEmpty ||
            !HasInventoryRelation(
              source.Entity,
              ItemInventorySlotKind.MainInventory,
              sourceSlotIndex) ||
            !_items.TryGet(source.Entity, out PlayerInventoryItemSnapshot current) ||
            current.MutationRevision != source.MutationRevision ||
            current.Stack != source.Stack ||
            amount >= current.Stack ||
            !_items.TryUpdate(
              current with { Stack = current.Stack - amount },
              current.MutationRevision))
        {
          return false;
        }

        sourceWasUpdated = true;
        if (!TrySetInventoryRelation(
              splitItem.Entity,
              ItemInventorySlotKind.MainInventory,
              destinationSlotIndex))
        {
          return false;
        }

        slots.MainInventorySlots[destinationSlotIndex] = splitItem.Entity;
        slots.InventoryChestStackMarkers[destinationSlotIndex] = false;
        return true;
      });

      if (committed)
      {
        _splitResults.Add(
          commandId,
          new SplitCommandResult(
            sourceSlotIndex,
            destinationSlotIndex,
            amount,
            true,
            splitItem.Entity));
        return true;
      }

      if (sourceWasUpdated)
      {
        RestoreAfterFailedTransfer(source);
        sourceWasRestored = true;
      }

      if (!_items.Remove(splitItem.Entity))
      {
        throw new InvalidOperationException("A rejected split item root could not be removed.");
      }

      splitRootWasRemoved = true;
      _splitResults.Add(
        commandId,
        new SplitCommandResult(sourceSlotIndex, destinationSlotIndex, amount, false));
      return false;
    }
    catch (Exception splitException)
    {
      try
      {
        if (sourceWasUpdated && !sourceWasRestored)
        {
          RestoreAfterFailedTransfer(source);
          sourceWasRestored = true;
        }

        if (!splitRootWasRemoved)
        {
          if (!_items.Remove(splitItem.Entity))
          {
            throw new InvalidOperationException("A failed split item root could not be removed.");
          }

          splitRootWasRemoved = true;
        }
      }
      catch (Exception rollbackException)
      {
        throw new AggregateException(
          "Item splitting failed and its source/root state could not be restored.",
          splitException,
          rollbackException);
      }

      throw;
    }
  }

  public bool TryConsume(Guid commandId, int typeId, int amount)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
    if (commandId == Guid.Empty || typeId <= 0)
    {
      return false;
    }

    if (_consumeResults.TryGetValue(commandId, out ConsumeCommandResult previousResult))
    {
      return previousResult.TypeId == typeId &&
        previousResult.Amount == amount &&
        previousResult.Applied;
    }

    bool applied = EditSlots(slots =>
    {
      for (int index = 0; index < slots.MainInventorySlots.Length; index++)
      {
        ItemEntityRef entity = slots.MainInventorySlots[index];
        if (!_items.TryGet(entity, out PlayerInventoryItemSnapshot item) ||
            item.TypeId != typeId || item.Stack < amount)
        {
          continue;
        }

        if (!HasInventoryRelation(entity, ItemInventorySlotKind.MainInventory, index))
        {
          return false;
        }

        int remaining = item.Stack - amount;
        if (remaining == 0)
        {
          if (!_items.CanRemove(entity) ||
              !_items.Remove(entity))
          {
            return false;
          }

          _pickupResults.Remove(entity);
          slots.MainInventorySlots[index] = ItemEntityRef.None;
          return true;
        }

        if (!_items.TryUpdate(
              item with { Stack = remaining },
              item.MutationRevision))
        {
          return false;
        }

        return true;
      }

      return false;
    });

    _consumeResults.Add(commandId, new ConsumeCommandResult(typeId, amount, applied));
    return applied;
  }

  public PlayerInventoryPickupResult TryPickup(
    PlayerInventoryItemSnapshot item,
    Func<bool>? releaseWorldProjection = null)
  {
    PrunePickupResults();
    if (_pickupResults.TryGetValue(item.Entity, out CachedPickupResult cachedResult) &&
        cachedResult.MutationRevision == item.MutationRevision)
    {
      return cachedResult.Result;
    }

    _pickupResults.Remove(item.Entity);

    if (!IsRootAvailable() || item.IsEmpty || !item.MutationRevision.IsAssigned)
    {
      return RejectedPickup(item, PlayerInventoryCommitResult.Rejected(
        item.Stack,
        PlayerInventoryCommitRejectionReason.ItemLookupFailed));
    }

    PlayerInventoryTransferSettings settings =
      PlayerInventoryTransferSettings.PickupItemFromWorld;
    var command = new PlayerInventoryPickupCommand(
      CreateCommandId(),
      item,
      item.ToSpaceCandidate(isPickup: true),
      settings);
    PlayerInventoryCommitResult inventoryResult = EditSlots(slots =>
    {
      var commitPort = new BoundInventoryCommitPort(this, releaseWorldProjection);
      var commitSystem = new PlayerInventoryCommitSystem(slots, this, commitPort);
      return commitSystem.Commit(
        new PlayerInventoryCommitCommand(
          command.CommandId,
          command.Item,
          command.Candidate),
        settings.CanGoIntoVoidVault);
    });

    if (!inventoryResult.Applied)
    {
      return RejectedPickup(item, inventoryResult);
    }

    var pickupResult = new PlayerInventoryPickupResult(
      Applied: true,
      RemainingStack: inventoryResult.RemainingStack,
      Inventory: inventoryResult,
      CoinMergeAttempted: false,
      CoinMerge: default,
      EffectsApplied: false,
      RejectionReason: PlayerInventoryPickupRejectionReason.EffectPortRejected);
    CachePickupResult(item, pickupResult);

    if (!ApplyPickupEffect(
          PlayerInventoryEffectIntentKind.PickupSound,
          command,
          inventoryResult,
          enabled: !settings.NoSound) ||
        !ApplyPickupEffect(
          PlayerInventoryEffectIntentKind.PickupLog,
          command,
          inventoryResult,
          enabled: true) ||
        !ApplyPickupEffect(
          PlayerInventoryEffectIntentKind.PickupText,
          command,
          inventoryResult,
          enabled: !settings.NoText))
    {
      return pickupResult;
    }

    PlayerCoinMergeResult coinResult = default;
    bool coinMergeAttempted = false;
    if (item.IsCoin && !settings.NoCoinMerge && inventoryResult.Target.IsMainInventory)
    {
      coinMergeAttempted = true;
      coinResult = EditSlots(slots =>
      {
        var coinMergePort = new BoundCoinMergePort(this, slots);
        var coinMergeSystem = new PlayerCoinMergeSystem(slots, this, coinMergePort);
        return coinMergeSystem.Merge(
          new PlayerCoinMergeCommand(command.CommandId, inventoryResult.Target.SlotIndex));
      });
      if (!coinResult.Applied &&
          coinResult.RejectionReason !=
          PlayerCoinMergeRejectionReason.SourceIsNotUpgradeableCoin)
      {
        pickupResult = pickupResult with
        {
          CoinMergeAttempted = true,
          CoinMerge = coinResult,
        };
        CachePickupResult(item, pickupResult);
        return pickupResult;
      }
    }

    if (!ApplyPickupEffect(
          PlayerInventoryEffectIntentKind.Achievement,
          command,
          inventoryResult,
          enabled: true) ||
        !ApplyPickupEffect(
          PlayerInventoryEffectIntentKind.PostAction,
          command,
          inventoryResult,
          enabled: true))
    {
      pickupResult = pickupResult with
      {
        CoinMergeAttempted = coinMergeAttempted,
        CoinMerge = coinResult,
      };
      CachePickupResult(item, pickupResult);
      return pickupResult;
    }

    pickupResult = new PlayerInventoryPickupResult(
      Applied: true,
      RemainingStack: inventoryResult.RemainingStack,
      Inventory: inventoryResult,
      CoinMergeAttempted: coinMergeAttempted,
      CoinMerge: coinResult,
      EffectsApplied: true,
      RejectionReason: PlayerInventoryPickupRejectionReason.None);
    CachePickupResult(item, pickupResult);
    return pickupResult;
  }

  public bool TryGetItem(ItemEntityRef item, out PlayerInventoryItemSnapshot snapshot)
  {
    if (!IsRootAvailable())
    {
      snapshot = default;
      return false;
    }

    return _items.TryGet(item, out snapshot);
  }

  public IReadOnlyList<PlayerInventoryItemSnapshot> VoidVaultItems => EmptyItems;

  public bool IsVoidVaultEnabled => false;

  public bool CanVoidVaultAccept(PlayerInventoryItemSnapshot item) => false;

  public bool TryApply(in PlayerInventoryEffectIntent intent)
  {
    if (!IsRootAvailable())
    {
      return false;
    }

    EffectIntentCount = checked(EffectIntentCount + 1);
    return _externalEffectPort?.TryApply(in intent) ?? true;
  }

  private TResult ReadSlots<TResult>(Func<PlayerInventorySlotsComponent, TResult> read)
    where TResult : struct
  {
    ArgumentNullException.ThrowIfNull(read);
    if (!IsRootAvailable())
    {
      return default;
    }

    if (!_entityRuntime.TryCapture(
          _playerHandle,
          read,
          out TResult result))
    {
      throw new InvalidOperationException("The Player inventory slots are unavailable.");
    }

    return result;
  }

  private TResult EditSlots<TResult>(Func<PlayerInventorySlotsComponent, TResult> edit)
    where TResult : struct
  {
    ArgumentNullException.ThrowIfNull(edit);
    if (!IsRootAvailable())
    {
      return default;
    }

    TResult result = default;
    if (!_entityRuntime.TryEdit<PlayerInventorySlotsComponent>(
          _playerHandle,
          (ref PlayerInventorySlotsComponent slots) => result = edit(slots)))
    {
      throw new InvalidOperationException("The Player inventory slots could not be edited.");
    }

    return result;
  }

  private bool TryApply(
    in PlayerInventoryCommitPlan plan,
    Func<bool>? releaseWorldProjection)
  {
    if (!_items.TryGet(plan.IncomingItem, out PlayerInventoryItemSnapshot incoming) ||
        _items.TryGetInventoryRelation(plan.IncomingItem, out _) ||
        incoming.MutationRevision != plan.IncomingMutationRevision ||
        plan.AcceptedStack <= 0 ||
        plan.RemainingStack < 0 ||
        incoming.Stack != checked(plan.AcceptedStack + plan.RemainingStack))
    {
      return false;
    }

    if (plan.AssignsEmptySlot)
    {
      if (!plan.Target.IsMainInventory ||
          plan.RemainingStack != 0 ||
          plan.AcceptedStack != incoming.Stack ||
          !TrySetInventoryRelation(
            plan.IncomingItem,
            ItemInventorySlotKind.MainInventory,
            plan.Target.SlotIndex))
      {
        return false;
      }

      try
      {
        if (_items.TryDetachWorldPresence(plan.IncomingItem, releaseWorldProjection))
        {
          return true;
        }
      }
      catch (Exception detachException)
      {
        if (!TryClearInventoryRelation(
              plan.IncomingItem,
              ItemInventorySlotKind.MainInventory,
              plan.Target.SlotIndex))
        {
          throw new AggregateException(
            "World-item pickup failed and its inventory relation could not be removed.",
            detachException,
            new InvalidOperationException("The Item inventory relation remains attached."));
        }

        throw;
      }

      if (!TryClearInventoryRelation(
            plan.IncomingItem,
            ItemInventorySlotKind.MainInventory,
            plan.Target.SlotIndex))
      {
        throw new InvalidOperationException(
          "A rejected world-item pickup retained its provisional inventory relation.");
      }

      return false;
    }

    if (!_items.TryGet(plan.ExistingItem, out PlayerInventoryItemSnapshot existing) ||
        !HasInventoryRelation(
          plan.ExistingItem,
          ItemInventorySlotKind.MainInventory,
          plan.Target.SlotIndex) ||
        existing.MutationRevision != plan.ExistingMutationRevision ||
        existing.Stack != plan.ExistingStackBefore ||
        plan.ExistingStackAfter != existing.Stack + plan.AcceptedStack)
    {
      return false;
    }

    if (plan.RemainingStack == 0 && !_items.CanRemove(plan.IncomingItem))
    {
      return false;
    }

    if (!_items.TryUpdate(
          existing with { Stack = plan.ExistingStackAfter },
          plan.ExistingMutationRevision))
    {
      return false;
    }

    if (plan.RemainingStack == 0)
    {
      bool worldPresenceDetached;
      try
      {
        worldPresenceDetached =
          _items.TryDetachWorldPresence(plan.IncomingItem, releaseWorldProjection);
      }
      catch (Exception detachException)
      {
        try
        {
          RestoreAfterFailedTransfer(existing);
        }
        catch (Exception rollbackException)
        {
          throw new AggregateException(
            "World-item merge failed and its destination stack could not be restored.",
            detachException,
            rollbackException);
        }

        throw;
      }

      if (!worldPresenceDetached)
      {
        RestoreAfterFailedTransfer(existing);
        return false;
      }

      if (!_items.Remove(plan.IncomingItem))
      {
        RestoreAfterFailedTransfer(existing);
        throw new InvalidOperationException(
          "An exhausted item root could not be removed after releasing world presence.");
      }

      _pickupResults.Remove(plan.IncomingItem);
      return true;
    }

    if (_items.TryUpdate(
          incoming with { Stack = plan.RemainingStack },
          plan.IncomingMutationRevision))
    {
      return true;
    }

    RestoreAfterFailedTransfer(existing);
    return false;
  }

  private bool TryApply(
    PlayerInventorySlotsComponent slots,
    in PlayerCoinMergePlan plan)
  {
    if ((uint)plan.SourceSlotIndex >= (uint)slots.MainInventorySlots.Length ||
        slots.MainInventorySlots[plan.SourceSlotIndex] != plan.SourceItem ||
        !HasInventoryRelation(
          plan.SourceItem,
          ItemInventorySlotKind.MainInventory,
          plan.SourceSlotIndex) ||
        !_items.TryGet(plan.SourceItem, out PlayerInventoryItemSnapshot source) ||
        source.MutationRevision != plan.SourceMutationRevision ||
        source.Stack != 100 ||
        source.TypeId + 1 != plan.UpgradedSourceTypeId)
    {
      return false;
    }

    if (!plan.HasDestination)
    {
      return _items.TryUpdate(source with
      {
        TypeId = plan.UpgradedSourceTypeId,
        PrefixId = 0,
        Stack = 1,
      }, plan.SourceMutationRevision);
    }

    if ((uint)plan.DestinationSlotIndex >= (uint)slots.MainInventorySlots.Length ||
        slots.MainInventorySlots[plan.DestinationSlotIndex] != plan.DestinationItem ||
        !HasInventoryRelation(
          plan.DestinationItem,
          ItemInventorySlotKind.MainInventory,
          plan.DestinationSlotIndex) ||
        !_items.TryGet(plan.DestinationItem, out PlayerInventoryItemSnapshot destination) ||
        destination.MutationRevision != plan.DestinationMutationRevision ||
        destination.TypeId != plan.UpgradedSourceTypeId ||
        destination.Stack != plan.DestinationStackBefore ||
        plan.DestinationStackAfter != destination.Stack + 1)
    {
      return false;
    }

    if (!_items.CanRemove(plan.SourceItem))
    {
      return false;
    }

    if (!_items.TryUpdate(
          destination with { Stack = plan.DestinationStackAfter },
          plan.DestinationMutationRevision))
    {
      return false;
    }

    if (_items.Remove(plan.SourceItem))
    {
      _pickupResults.Remove(plan.SourceItem);
      slots.MainInventorySlots[plan.SourceSlotIndex] = ItemEntityRef.None;
      return true;
    }

    RestoreAfterFailedTransfer(destination);
    return false;
  }

  private bool ApplyPickupEffect(
    PlayerInventoryEffectIntentKind kind,
    in PlayerInventoryPickupCommand command,
    in PlayerInventoryCommitResult inventoryResult,
    bool enabled)
  {
    if (!enabled)
    {
      return true;
    }

    var intent = new PlayerInventoryEffectIntent(
      kind,
      inventoryResult.Target,
      command.Item.Entity,
      inventoryResult.AcceptedStack,
      command.Item.IsCoin,
      command.Settings.LongText,
      command.Settings.MakeNewAndShiny);
    return TryApply(in intent);
  }

  private void RestoreAfterFailedTransfer(PlayerInventoryItemSnapshot previousItem)
  {
    if (!_items.TryGet(previousItem.Entity, out PlayerInventoryItemSnapshot current) ||
        !_items.TryUpdate(
          previousItem with { MutationRevision = current.MutationRevision },
          current.MutationRevision))
    {
      throw new InvalidOperationException(
        "A failed item transfer could not restore its previously updated stack.");
    }
  }

  private void CachePickupResult(
    PlayerInventoryItemSnapshot item,
    PlayerInventoryPickupResult result)
  {
    if (_items.TryGet(item.Entity, out _))
    {
      _pickupResults[item.Entity] = new CachedPickupResult(item.MutationRevision, result);
    }
    else
    {
      _pickupResults.Remove(item.Entity);
    }
  }

  private void PrunePickupResults()
  {
    foreach (ItemEntityRef item in _pickupResults.Keys.ToArray())
    {
      if (!_items.TryGet(item, out _))
      {
        _pickupResults.Remove(item);
      }
    }
  }

  private ItemEntityRef[] CaptureMainSlots()
  {
    ItemEntityRef[]? items = null;
    if (!IsRootAvailable() ||
        !_entityRuntime.TryInspect<PlayerInventorySlotsComponent>(
          _playerHandle,
          (in PlayerInventorySlotsComponent slots) =>
            items = (ItemEntityRef[])slots.MainInventorySlots.Clone()))
    {
      throw new InvalidOperationException("The Player inventory slots are unavailable.");
    }

    return items ?? throw new InvalidOperationException("The Player slots were not captured.");
  }

  private bool TryCollectReleaseItems(
    ItemEntityRef[] mainSlots,
    ItemEntityRef trashItem,
    out List<ItemEntityRef> items)
  {
    items = [];
    if (!TryGetPlayerReference(out _))
    {
      return false;
    }

    var seen = new HashSet<ItemEntityRef>();
    for (int slotIndex = 0; slotIndex < mainSlots.Length; slotIndex++)
    {
      ItemEntityRef item = mainSlots[slotIndex];
      if (item.IsEmpty)
      {
        continue;
      }

      if (!seen.Add(item) ||
          !HasInventoryRelation(item, ItemInventorySlotKind.MainInventory, slotIndex) ||
          !_items.CanRemove(item))
      {
        items.Clear();
        return false;
      }

      items.Add(item);
    }

    if (!trashItem.IsEmpty)
    {
      if (!seen.Add(trashItem) ||
          !HasInventoryRelation(trashItem, ItemInventorySlotKind.Trash, 0) ||
          !_items.CanRemove(trashItem))
      {
        items.Clear();
        return false;
      }

      items.Add(trashItem);
    }

    return true;
  }

  private bool TryCaptureReleaseItems(out List<ItemEntityRef> items)
  {
    items = [];
    if (!IsRootAvailable())
    {
      return false;
    }

    ItemEntityRef[]? mainSlots = null;
    ItemEntityRef trashItem = ItemEntityRef.None;
    if (!_entityRuntime.TryInspect<PlayerInventorySlotsComponent>(
          _playerHandle,
          (in PlayerInventorySlotsComponent slots) =>
          {
            mainSlots = (ItemEntityRef[])slots.MainInventorySlots.Clone();
            trashItem = slots.TrashItem;
          }) ||
        mainSlots is null)
    {
      return false;
    }

    return TryCollectReleaseItems(mainSlots, trashItem, out items);
  }

  private bool ValidateMainInventoryRelations(
    ItemEntityRef[] items,
    bool requireRemovable)
  {
    if (!TryGetPlayerReference(out _))
    {
      return false;
    }

    var seen = new HashSet<ItemEntityRef>();
    for (int slotIndex = 0; slotIndex < items.Length; slotIndex++)
    {
      ItemEntityRef item = items[slotIndex];
      if (item.IsEmpty)
      {
        continue;
      }

      if (!seen.Add(item) ||
          !HasInventoryRelation(item, ItemInventorySlotKind.MainInventory, slotIndex) ||
          (requireRemovable && !_items.CanRemove(item)))
      {
        return false;
      }
    }

    return true;
  }

  private bool HasInventoryRelation(
    ItemEntityRef item,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    return TryGetPlayerReference(out EntityReference playerReference) &&
      _items.TryGetInventoryRelation(item, out ItemInventoryRelationComponent relation) &&
      relation.PlayerReference == playerReference &&
      relation.SlotKind == slotKind &&
      relation.SlotIndex == slotIndex;
  }

  private bool TrySetInventoryRelation(
    ItemEntityRef item,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    return TryGetPlayerReference(out EntityReference playerReference) &&
      _items.TrySetInventoryRelation(item, playerReference, slotKind, slotIndex);
  }

  private bool TryClearInventoryRelation(
    ItemEntityRef item,
    ItemInventorySlotKind slotKind,
    int slotIndex)
  {
    return TryGetPlayerReference(out EntityReference playerReference) &&
      _items.TryClearInventoryRelation(
        item,
        new ItemInventoryRelationComponent(playerReference, slotKind, slotIndex));
  }

  private bool TryGetPlayerReference(out EntityReference playerReference)
  {
    return _entityRuntime.TryGetReference(
      _playerHandle,
      EntityReferenceScope.Player,
      out playerReference);
  }

  private void RemoveCreatedItems(IEnumerable<ItemEntityRef> items)
  {
    List<Exception>? failures = null;
    foreach (ItemEntityRef item in items.Where(static item => !item.IsEmpty).Distinct())
    {
      if (!_items.Remove(item))
      {
        (failures ??= []).Add(
          new InvalidOperationException("A newly created item root could not be removed."));
      }
    }

    if (failures is not null)
    {
      throw new AggregateException("One or more created item roots remain live.", failures);
    }
  }

  private Guid CreateCommandId()
  {
    long sequence = checked(++_nextCommandId);
    Span<byte> bytes = stackalloc byte[16];
    BitConverter.TryWriteBytes(bytes[..8], 0x4E4C54585049434B);
    BitConverter.TryWriteBytes(bytes[8..], ((long)_playerSlot << 32) | sequence);
    return new Guid(bytes);
  }

  private bool IsRootAvailable()
  {
    return !_isReleased &&
      _entityRuntime.TryGetStatus(_playerHandle, out EntityRuntimeStatus status) &&
      status == EntityRuntimeStatus.Running &&
      _entityRuntime.Has<PlayerInventorySlotsComponent>(_playerHandle);
  }

  private static PlayerInventoryPickupResult RejectedPickup(
    PlayerInventoryItemSnapshot item,
    PlayerInventoryCommitResult inventoryResult)
  {
    return PlayerInventoryPickupResult.Rejected(
      Math.Max(0, item.Stack),
      inventoryResult,
      PlayerInventoryPickupRejectionReason.InventoryCommitRejected);
  }

  private sealed class BoundInventoryCommitPort : IPlayerInventoryCommitPort
  {
    private readonly RuntimePlayerInventoryOwner _owner;
    private readonly Func<bool>? _releaseWorldProjection;

    public BoundInventoryCommitPort(
      RuntimePlayerInventoryOwner owner,
      Func<bool>? releaseWorldProjection)
    {
      _owner = owner;
      _releaseWorldProjection = releaseWorldProjection;
    }

    public bool TryApply(in PlayerInventoryCommitPlan plan)
    {
      return _owner.TryApply(in plan, _releaseWorldProjection);
    }
  }

  private sealed class BoundCoinMergePort : IPlayerInventoryCoinMergePort
  {
    private readonly RuntimePlayerInventoryOwner _owner;
    private readonly PlayerInventorySlotsComponent _slots;

    public BoundCoinMergePort(
      RuntimePlayerInventoryOwner owner,
      PlayerInventorySlotsComponent slots)
    {
      _owner = owner;
      _slots = slots;
    }

    public bool TryApply(in PlayerCoinMergePlan plan)
    {
      return _owner.TryApply(_slots, in plan);
    }
  }

  private readonly record struct CachedPickupResult(
    ItemMutationRevision MutationRevision,
    PlayerInventoryPickupResult Result);

  private readonly record struct ConsumeCommandResult(
    int TypeId,
    int Amount,
    bool Applied);

  private readonly record struct SplitCommandResult(
    int SourceSlotIndex,
    int DestinationSlotIndex,
    int Amount,
    bool Applied,
    ItemEntityRef SplitItem = default);

}
