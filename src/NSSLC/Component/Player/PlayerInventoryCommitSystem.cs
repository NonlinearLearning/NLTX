namespace Terraria.Player;

public sealed class PlayerInventoryCommitSystem
{
  private const int PersonalInventoryEnd = 50;
  private const int CoinInventoryEnd = 54;
  private const int AmmoSlotStart = 54;
  private const int AmmoSlotEnd = 58;

  private readonly HashSet<Guid> _acceptedCommandIds = [];
  private readonly PlayerInventorySlotsComponent _inventory;
  private readonly IPlayerInventoryItemQuery _itemQuery;
  private readonly IPlayerInventoryCommitPort _commitPort;

  public PlayerInventoryCommitSystem(
    PlayerInventorySlotsComponent inventory,
    IPlayerInventoryItemQuery itemQuery,
    IPlayerInventoryCommitPort commitPort)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(itemQuery);
    ArgumentNullException.ThrowIfNull(commitPort);

    _inventory = inventory;
    _itemQuery = itemQuery;
    _commitPort = commitPort;
  }

  public PlayerInventoryCommitResult Commit(
    in PlayerInventoryCommitCommand command)
  {
    return Commit(in command, allowVoidVault: true);
  }

  public PlayerInventoryCommitResult Commit(
    in PlayerInventoryCommitCommand command,
    bool allowVoidVault)
  {
    PlayerInventoryItemSnapshot incoming = command.Item;
    int remainingStack = Math.Max(0, incoming.Stack);

    if (command.CommandId == Guid.Empty)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.EmptyCommand);
    }

    if (incoming.Entity.IsEmpty)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.EmptyItem);
    }

    if (incoming.Stack <= 0 ||
      incoming.MaximumStack <= 0 ||
      incoming.Stack > incoming.MaximumStack)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.InvalidItemStack);
    }

    if (command.Candidate.TypeId != incoming.TypeId ||
      command.Candidate.PrefixId != incoming.PrefixId)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.CandidateMismatch);
    }

    if (_acceptedCommandIds.Contains(command.CommandId))
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.DuplicateCommand);
    }

    InventorySnapshot snapshot;
    try
    {
      snapshot = CreateSnapshot();
    }
    catch (InvalidOperationException)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.ItemLookupFailed);
    }

    if (ContainsItemEntity(snapshot, incoming.Entity))
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.ItemAlreadyInInventory);
    }

    PlayerItemSpaceEvaluationInput evaluation = new(
      command.Candidate,
      snapshot.Slots,
      CreateVoidVaultSlots(),
      allowVoidVault && _itemQuery.IsVoidVaultEnabled,
      allowVoidVault && _itemQuery.CanVoidVaultAccept(incoming));

    PlayerItemSpaceSnapshot space = PlayerItemSpaceQuery.Evaluate(evaluation);
    if (command.Candidate.IsUniqueStack &&
      ContainsType(snapshot, command.Candidate.TypeId))
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.UniqueItemAlreadyPresent);
    }

    if (!space.CanTakeItem)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.NoSpace);
    }

    if (!TryFindTarget(
      command.Candidate,
      snapshot,
      allowVoidVault,
      out PlayerInventoryCommitTarget target,
      out PlayerInventoryItemSnapshot existingItem))
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        command.Candidate.IsPickup
          ? PlayerInventoryCommitRejectionReason.PickupRequiresWorldItemAdapter
          : PlayerInventoryCommitRejectionReason.NoSpace);
    }

    int acceptedStack = CalculateAcceptedStack(incoming, existingItem);
    if (acceptedStack <= 0)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.NoSpace);
    }

    if (existingItem.IsEmpty && acceptedStack != incoming.Stack)
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.InvalidItemStack);
    }

    int remainingAfterCommit = incoming.Stack - acceptedStack;
    int existingStackAfter = existingItem.IsEmpty
      ? acceptedStack
      : existingItem.Stack + acceptedStack;
    PlayerInventoryCommitPlan plan = new(
      command.CommandId,
      target,
      incoming.Entity,
      existingItem.Entity,
      acceptedStack,
      remainingAfterCommit,
      existingItem.IsEmpty ? 0 : existingItem.Stack,
      existingStackAfter);

    if (!_commitPort.TryApply(plan))
    {
      return PlayerInventoryCommitResult.Rejected(
        remainingStack,
        PlayerInventoryCommitRejectionReason.CommitPortRejected);
    }

    if (target.IsMainInventory && existingItem.IsEmpty)
    {
      _inventory.MainInventorySlots[target.SlotIndex] = incoming.Entity;
    }

    _acceptedCommandIds.Add(command.CommandId);
    return new PlayerInventoryCommitResult(
      Applied: true,
      Target: target,
      AcceptedStack: acceptedStack,
      RemainingStack: remainingAfterCommit,
      RejectionReason: PlayerInventoryCommitRejectionReason.None);
  }

  public void ResetForLifecycle()
  {
    _acceptedCommandIds.Clear();
  }

  private InventorySnapshot CreateSnapshot()
  {
    PlayerItemSpaceSlotSnapshot[] slots =
      new PlayerItemSpaceSlotSnapshot[_inventory.MainInventorySlots.Length];
    PlayerInventoryItemSnapshot[] items =
      new PlayerInventoryItemSnapshot[_inventory.MainInventorySlots.Length];

    for (int index = 0; index < _inventory.MainInventorySlots.Length; index++)
    {
      ItemEntityRef entity = _inventory.MainInventorySlots[index];
      if (entity.IsEmpty)
      {
        continue;
      }

      if (!_itemQuery.TryGetItem(entity, out PlayerInventoryItemSnapshot item) ||
        item.Entity != entity ||
        item.IsEmpty ||
        item.MaximumStack <= 0 ||
        item.Stack > item.MaximumStack)
      {
        throw new InvalidOperationException(
          $"Unable to read inventory item at slot {index}.");
      }

      items[index] = item;
      slots[index] = item.ToSlotSnapshot();
    }

    return new InventorySnapshot(slots, items);
  }

  private bool TryFindTarget(
    PlayerItemSpaceCandidate candidate,
    InventorySnapshot snapshot,
    bool allowVoidVault,
    out PlayerInventoryCommitTarget target,
    out PlayerInventoryItemSnapshot existingItem)
  {
    if (candidate.HasAmmo &&
      !candidate.IsNotAmmo &&
      TryFindAmmoTarget(candidate, snapshot, out target, out existingItem))
    {
      return true;
    }

    int inventoryEnd = candidate.IsCoin
      ? Math.Min(CoinInventoryEnd, snapshot.Slots.Length)
      : Math.Min(PersonalInventoryEnd, snapshot.Slots.Length);
    if (candidate.IsCoin)
    {
      for (int index = PersonalInventoryEnd; index < inventoryEnd; index++)
      {
        if (CanStackInto(index, candidate, snapshot, out existingItem))
        {
          target = new PlayerInventoryCommitTarget(
            PlayerInventoryCommitTargetKind.MainInventory,
            index);
          return true;
        }
      }
    }

    for (int index = 0; index < inventoryEnd; index++)
    {
      if (CanStackInto(index, candidate, snapshot, out existingItem))
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.MainInventory,
          index);
        return true;
      }
    }

    if (!candidate.IsCoin &&
      candidate.HasUseStyle &&
      TryFindEmptyTarget(0, Math.Min(10, inventoryEnd), snapshot,
        out target, out existingItem))
    {
      return true;
    }

    if (candidate.IsFavorited)
    {
      if (TryFindEmptyTarget(
        0,
        inventoryEnd,
        snapshot,
        out target,
        out existingItem))
      {
        return true;
      }
    }
    else if (TryFindEmptyTargetReverse(
      inventoryEnd,
      snapshot,
      out target,
      out existingItem))
    {
      return true;
    }

    if (!allowVoidVault)
    {
      target = default;
      existingItem = default;
      return false;
    }

    IReadOnlyList<PlayerInventoryItemSnapshot> voidVaultItems =
      _itemQuery.VoidVaultItems;
    for (int index = 0; index < voidVaultItems.Count; index++)
    {
      PlayerInventoryItemSnapshot voidVaultItem = voidVaultItems[index];
      PlayerItemSpaceSlotSnapshot slot = voidVaultItem.ToSlotSnapshot();
      if (CanAcceptSlot(slot, candidate))
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.VoidVault,
          index);
        existingItem = voidVaultItem.IsEmpty
          ? default
          : voidVaultItem;
        return true;
      }
    }

    target = default;
    existingItem = default;
    return false;
  }

  private PlayerItemSpaceSlotSnapshot[] CreateVoidVaultSlots()
  {
    IReadOnlyList<PlayerInventoryItemSnapshot> voidVaultItems =
      _itemQuery.VoidVaultItems;
    PlayerItemSpaceSlotSnapshot[] slots =
      new PlayerItemSpaceSlotSnapshot[voidVaultItems.Count];
    for (int index = 0; index < voidVaultItems.Count; index++)
    {
      slots[index] = voidVaultItems[index].ToSlotSnapshot();
    }

    return slots;
  }

  private bool TryFindAmmoTarget(
    PlayerItemSpaceCandidate candidate,
    InventorySnapshot snapshot,
    out PlayerInventoryCommitTarget target,
    out PlayerInventoryItemSnapshot existingItem)
  {
    for (int index = AmmoSlotStart;
      index < AmmoSlotEnd && index < snapshot.Slots.Length;
      index++)
    {
      if (CanStackInto(index, candidate, snapshot, out existingItem))
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.MainInventory,
          index);
        return true;
      }
    }

    if (!candidate.CanFillEmptyAmmoSlot)
    {
      target = default;
      existingItem = default;
      return false;
    }

    for (int index = AmmoSlotStart;
      index < AmmoSlotEnd && index < snapshot.Slots.Length;
      index++)
    {
      if (snapshot.Slots[index].TypeId == 0)
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.MainInventory,
          index);
        existingItem = default;
        return true;
      }
    }

    target = default;
    existingItem = default;
    return false;
  }

  private static bool TryFindEmptyTarget(
    int start,
    int end,
    InventorySnapshot snapshot,
    out PlayerInventoryCommitTarget target,
    out PlayerInventoryItemSnapshot existingItem)
  {
    for (int index = start; index < end; index++)
    {
      if (snapshot.Slots[index].TypeId == 0)
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.MainInventory,
          index);
        existingItem = default;
        return true;
      }
    }

    target = default;
    existingItem = default;
    return false;
  }

  private static bool TryFindEmptyTargetReverse(
    int end,
    InventorySnapshot snapshot,
    out PlayerInventoryCommitTarget target,
    out PlayerInventoryItemSnapshot existingItem)
  {
    for (int index = end - 1; index >= 0; index--)
    {
      if (snapshot.Slots[index].TypeId == 0)
      {
        target = new PlayerInventoryCommitTarget(
          PlayerInventoryCommitTargetKind.MainInventory,
          index);
        existingItem = default;
        return true;
      }
    }

    target = default;
    existingItem = default;
    return false;
  }

  private static bool CanStackInto(
    int index,
    PlayerItemSpaceCandidate candidate,
    InventorySnapshot snapshot,
    out PlayerInventoryItemSnapshot existingItem)
  {
    existingItem = snapshot.Items[index];
    if (snapshot.Slots[index].TypeId == 0)
    {
      return false;
    }

    return CanAcceptSlot(snapshot.Slots[index], candidate);
  }

  private static bool CanAcceptSlot(
    PlayerItemSpaceSlotSnapshot slot,
    PlayerItemSpaceCandidate candidate)
  {
    if (slot.TypeId == 0)
    {
      return true;
    }

    if (slot.IsFavorited && slot.OnlyNeedOneInInventory)
    {
      return false;
    }

    return slot.Stack < slot.MaximumStack &&
      slot.TypeId == candidate.TypeId &&
      slot.PrefixId == candidate.PrefixId;
  }

  private static int CalculateAcceptedStack(
    PlayerInventoryItemSnapshot incoming,
    PlayerInventoryItemSnapshot existing)
  {
    if (existing.IsEmpty)
    {
      return incoming.Stack;
    }

    return Math.Min(
      incoming.Stack,
      Math.Max(0, existing.MaximumStack - existing.Stack));
  }

  private static bool ContainsItemEntity(
    InventorySnapshot snapshot,
    ItemEntityRef item)
  {
    return snapshot.Items.Any(existing => existing.Entity == item);
  }

  private static bool ContainsType(
    InventorySnapshot snapshot,
    int typeId)
  {
    return snapshot.Slots.Any(slot => slot.TypeId == typeId && slot.Stack > 0);
  }

  private readonly record struct InventorySnapshot(
    PlayerItemSpaceSlotSnapshot[] Slots,
    PlayerInventoryItemSnapshot[] Items);
}
