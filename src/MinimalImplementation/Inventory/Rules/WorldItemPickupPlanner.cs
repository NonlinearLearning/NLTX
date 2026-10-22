using MinimalImplementation.Server.Inventory.Content;
using MinimalImplementation.Server.Inventory.State;

namespace MinimalImplementation.Server.Inventory.Rules;

/// <summary>Pure transcription of GetItem slot ordering, partial stacks and DoCoins.</summary>
public sealed class WorldItemPickupPlanner
{
  private readonly ItemValidationPolicy _items;

  public WorldItemPickupPlanner(ItemValidationPolicy items)
  {
    ArgumentNullException.ThrowIfNull(items);
    _items = items;
  }

  /// <summary>
  /// Read-only ItemSpace query for inventory items. IsAmmo is ammo > 0 and !notAmmo,
  /// unlike FitsAmmoSlot,
  /// which also includes bait and paint. False means required content facts are missing.
  /// </summary>
  public bool TryGetAttractionSpace(
    InventorySnapshot inventory, InventoryItemState source,
    ItemPickupFacts facts, bool voidVaultEnabled, out bool canTake, out bool goingToVoid)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(facts);
    canTake = false;
    goingToVoid = false;
    int requiredSlots = voidVaultEnabled
      ? InventorySlotLayout.VoidStart + InventorySlotLayout.BankCount : InventorySlotLayout.Mouse;
    if (inventory.Slots.Count < requiredSlots || !_items.IsValid(source) || source.IsEmpty
      || facts.IsAmmo is not bool isAmmo
      || !ItemContentFacts.TryGetMaximumStack(source.Type, out int maximum))
    {
      return false;
    }

    if (facts.UniqueStack && inventory.Slots.Take(InventorySlotLayout.Mouse)
      .Any(slot => slot.Item.Type == source.Type && !slot.Item.IsEmpty))
    {
      return true;
    }

    int end = ItemContentFacts.IsCoin(source.Type)
      ? InventorySlotLayout.AmmoStart : InventorySlotLayout.MainCount;
    for (int index = 0; index < end; index++)
    {
      if (CanAccept(inventory.Slots[index].Item))
      {
        canTake = true;
        return true;
      }
    }

    for (int index = InventorySlotLayout.AmmoStart; index < InventorySlotLayout.Mouse; index++)
    {
      InventoryItemState slot = inventory.Slots[index].Item;
      if (isAmmo && (facts.CanFillEmptyAmmoSlot || !slot.IsEmpty) && CanAccept(slot)
        || _items.CanStack(slot, source) && slot.Stack < maximum)
      {
        canTake = true;
        return true;
      }
    }

    if (voidVaultEnabled && !facts.QuestItem && source.Type != 3822)
    {
      for (int index = InventorySlotLayout.VoidStart; index < requiredSlots; index++)
      {
        if (CanAccept(inventory.Slots[index].Item))
        {
          canTake = true;
          goingToVoid = true;
          return true;
        }
      }
    }

    return true;

    bool CanAccept(InventoryItemState slot)
    {
      return slot.IsEmpty || _items.CanStack(slot, source) && slot.Stack < maximum
        && (!slot.Favorited || !facts.OnlyNeedOneInInventory);
    }
  }

  public WorldItemPickupPlan Plan(
    InventorySnapshot inventory, InventoryItemState source,
    ItemPickupFacts facts, bool voidVaultEnabled)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(facts);
    int requiredSlots = voidVaultEnabled
      ? InventorySlotLayout.VoidStart + InventorySlotLayout.BankCount : InventorySlotLayout.Mouse;
    if (inventory.Slots.Count < requiredSlots || !_items.IsValid(source) || source.IsEmpty)
    {
      throw new ArgumentException("Pickup requires valid inventory slots and a live item.");
    }

    var values = inventory.Slots.Select(slot => slot.Item).ToArray();
    if (facts.UniqueStack && values.Take(InventorySlotLayout.Mouse)
      .Any(item => item.Type == source.Type && !item.IsEmpty))
    {
      return new WorldItemPickupPlan(source, []);
    }

    int remaining = source.Stack;
    bool coin = ItemContentFacts.IsCoin(source.Type);
    ItemContentFacts.TryGetMaximumStack(source.Type, out int maximum);
    if (facts.FitsAmmoSlot)
    {
      FillOccupied(InventorySlotLayout.AmmoStart, InventorySlotLayout.Mouse, false);
      if (facts.CanFillEmptyAmmoSlot)
      {
        FillEmpty(InventorySlotLayout.AmmoStart, InventorySlotLayout.Mouse, 1);
      }
    }

    if (coin)
    {
      FillOccupied(InventorySlotLayout.CoinStart, InventorySlotLayout.AmmoStart);
    }

    FillOccupied(0, InventorySlotLayout.MainCount);
    if (!coin && facts.HasUseStyle)
    {
      FillEmpty(0, 10, 1);
    }

    int end = coin ? InventorySlotLayout.AmmoStart : InventorySlotLayout.MainCount;
    if (source.Favorited)
    {
      FillEmpty(0, end, 1);
    }
    else
    {
      FillEmpty(end - 1, -1, -1);
    }

    if (voidVaultEnabled && !facts.QuestItem && source.Type != 3822)
    {
      FillOccupied(InventorySlotLayout.VoidStart,
        InventorySlotLayout.VoidStart + InventorySlotLayout.BankCount, false);
      FillEmpty(InventorySlotLayout.VoidStart,
        InventorySlotLayout.VoidStart + InventorySlotLayout.BankCount, 1);
    }

    var mutations = new List<InventoryMutation>();
    for (int index = 0; index < values.Length; index++)
    {
      InventorySlotSnapshot before = inventory.Slots[index];
      if (values[index] != before.Item)
      {
        mutations.Add(new InventoryMutation(before.SlotId, before.Generation, values[index]));
      }
    }

    return new WorldItemPickupPlan(source.WithStack(remaining), mutations.AsReadOnly());

    void FillOccupied(int start, int limit, bool protectFavorite = true)
    {
      for (int index = start; index < limit && remaining > 0; index++)
      {
        InventoryItemState destination = values[index];
        if (!_items.CanStack(destination, source)
          || protectFavorite && destination.Favorited && facts.OnlyNeedOneInInventory)
        {
          continue;
        }

        int moved = Math.Min(remaining, maximum - destination.Stack);
        if (moved <= 0)
        {
          continue;
        }

        values[index] = destination.WithStack(destination.Stack + moved);
        remaining -= moved;
        if (coin && index < InventorySlotLayout.AmmoStart)
        {
          ConvertCoins(index);
        }
      }
    }

    void FillEmpty(int start, int limit, int step)
    {
      for (int index = start; index != limit && remaining > 0; index += step)
      {
        if (!values[index].IsEmpty)
        {
          continue;
        }

        int moved = Math.Min(remaining, maximum);
        values[index] = source.WithStack(moved);
        remaining -= moved;
        if (coin && index < InventorySlotLayout.AmmoStart)
        {
          ConvertCoins(index);
        }
      }
    }

    void ConvertCoins(int index)
    {
      InventoryItemState current = values[index];
      if (current.Stack != 100 || current.Type is < 71 or > 73)
      {
        return;
      }

      values[index] = new InventoryItemState(current.Type + 1, 1, 0, false);
      ItemContentFacts.TryGetMaximumStack(current.Type + 1, out int nextMaximum);
      for (int other = 0; other < InventorySlotLayout.AmmoStart; other++)
      {
        if (other == index || values[other].Type != current.Type + 1
          || values[other].Stack >= nextMaximum)
        {
          continue;
        }

        values[other] = values[other].WithStack(values[other].Stack + 1);
        values[index] = InventoryItemState.Empty;
        ConvertCoins(other);
        return;
      }
    }
  }
}
