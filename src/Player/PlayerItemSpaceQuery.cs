namespace Terraria.Player;

public static class PlayerItemSpaceQuery
{
  public static PlayerItemSpaceSnapshot Evaluate(
    in PlayerItemSpaceInput input)
  {
    return new PlayerItemSpaceSnapshot(
      input.CanTakeItem,
      input.ItemIsGoingToVoidVault);
  }

  public static PlayerItemSpaceSnapshot Evaluate(
    in PlayerItemSpaceEvaluationInput input)
  {
    ArgumentNullException.ThrowIfNull(input.InventorySlots);
    ArgumentNullException.ThrowIfNull(input.VoidVaultSlots);

    if (input.Candidate.IsPickup)
    {
      return new PlayerItemSpaceSnapshot(true, false);
    }

    if (input.Candidate.IsUniqueStack && HasItem(input))
    {
      return new PlayerItemSpaceSnapshot(false, false);
    }

    int personalInventoryLimit = input.Candidate.IsCoin ? 54 : 50;
    int personalInventoryEnd = Math.Min(
      personalInventoryLimit,
      input.InventorySlots.Count);
    for (int i = 0; i < personalInventoryEnd; i++)
    {
      if (CanItemSlotAcceptPickup(
        input.InventorySlots[i],
        input.Candidate))
      {
        return new PlayerItemSpaceSnapshot(true, false);
      }
    }

    if (input.Candidate.HasAmmo && !input.Candidate.IsNotAmmo)
    {
      for (int i = 54; i < 58 && i < input.InventorySlots.Count; i++)
      {
        PlayerItemSpaceSlotSnapshot slot = input.InventorySlots[i];
        if (slot.TypeId == 0 && !input.Candidate.CanFillEmptyAmmoSlot)
        {
          continue;
        }

        if (CanItemSlotAcceptPickup(slot, input.Candidate))
        {
          return new PlayerItemSpaceSnapshot(true, false);
        }
      }
    }

    for (int i = 54; i < 58 && i < input.InventorySlots.Count; i++)
    {
      PlayerItemSpaceSlotSnapshot slot = input.InventorySlots[i];
      if (slot.TypeId > 0 &&
        slot.Stack < slot.MaximumStack &&
        CanStack(input.Candidate, slot))
      {
        return new PlayerItemSpaceSnapshot(true, false);
      }
    }

    if (input.IsVoidVaultEnabled &&
      input.CanVoidVaultAccept &&
      HasAcceptingVoidVaultSlot(input))
    {
      return new PlayerItemSpaceSnapshot(true, true);
    }

    return new PlayerItemSpaceSnapshot(false, false);
  }

  private static bool HasItem(in PlayerItemSpaceEvaluationInput input)
  {
    int inventoryEnd = Math.Min(58, input.InventorySlots.Count);
    for (int i = 0; i < inventoryEnd; i++)
    {
      PlayerItemSpaceSlotSnapshot slot = input.InventorySlots[i];
      if (slot.TypeId == input.Candidate.TypeId && slot.Stack > 0)
      {
        return true;
      }
    }

    return false;
  }

  private static bool HasAcceptingVoidVaultSlot(
    in PlayerItemSpaceEvaluationInput input)
  {
    foreach (PlayerItemSpaceSlotSnapshot slot in input.VoidVaultSlots)
    {
      if (CanItemSlotAcceptPickup(slot, input.Candidate))
      {
        return true;
      }
    }

    return false;
  }

  private static bool CanItemSlotAcceptPickup(
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

    return slot.Stack < slot.MaximumStack && CanStack(candidate, slot);
  }

  private static bool CanStack(
    PlayerItemSpaceCandidate candidate,
    PlayerItemSpaceSlotSnapshot slot)
  {
    return candidate.TypeId == slot.TypeId &&
      candidate.PrefixId == slot.PrefixId;
  }
}
