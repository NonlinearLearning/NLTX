using System;

namespace Terraria.Dome.Simulation.Items.Systems;

public readonly record struct ItemInputValidationResult(
  bool IsAccepted,
  ItemStack Stack,
  ItemCommandRejection Rejection);

public sealed class ItemInputValidationSystem
{
  public ItemInputValidationResult Validate(
    bool playerIsActive,
    InventoryComponent inventory,
    int selectedSlot,
    ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    if (!playerIsActive)
    {
      return Reject("player-not-active", "The player is not active.");
    }

    if (selectedSlot < 0 || selectedSlot >= InventoryComponent.HotbarSlotCount)
    {
      return Reject("invalid-slot", "The selected item slot is outside the hotbar.");
    }

    ItemStack stack = inventory.GetSlot(selectedSlot);
    if (stack.IsEmpty || !definitions.TryGet(stack.ItemType, out _))
    {
      return Reject("empty-or-unknown", "The selected item is empty or unknown.");
    }

    return new ItemInputValidationResult(true, stack, default);
  }

  private static ItemInputValidationResult Reject(string code, string reason)
  {
    return new ItemInputValidationResult(
      false,
      ItemStack.Empty,
      new ItemCommandRejection(code, reason));
  }
}
