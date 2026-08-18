using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class InventoryCommandSystem
{
  public bool TryTransfer(
    InventoryComponent inventory,
    int sourceSlot,
    int destinationSlot,
    int quantity,
    ItemDefinitionRegistry definitions,
    out ItemCommandRejection rejection)
  {
    return TryApply(
      inventory,
      sourceSlot,
      destinationSlot,
      quantity,
      definitions,
      requireEmptyDestination: false,
      requireNonEmptyDestination: false,
      allowWholeSource: true,
      out rejection);
  }

  public bool TrySplit(
    InventoryComponent inventory,
    int sourceSlot,
    int destinationSlot,
    int quantity,
    ItemDefinitionRegistry definitions,
    out ItemCommandRejection rejection)
  {
    return TryApply(
      inventory,
      sourceSlot,
      destinationSlot,
      quantity,
      definitions,
      requireEmptyDestination: true,
      requireNonEmptyDestination: false,
      allowWholeSource: false,
      out rejection);
  }

  public bool TryMerge(
    InventoryComponent inventory,
    int sourceSlot,
    int destinationSlot,
    int quantity,
    ItemDefinitionRegistry definitions,
    out ItemCommandRejection rejection)
  {
    return TryApply(
      inventory,
      sourceSlot,
      destinationSlot,
      quantity,
      definitions,
      requireEmptyDestination: false,
      requireNonEmptyDestination: true,
      allowWholeSource: true,
      out rejection);
  }

  private static bool TryApply(
    InventoryComponent inventory,
    int sourceSlot,
    int destinationSlot,
    int quantity,
    ItemDefinitionRegistry definitions,
    bool requireEmptyDestination,
    bool requireNonEmptyDestination,
    bool allowWholeSource,
    out ItemCommandRejection rejection)
  {
    if (sourceSlot < 0 || sourceSlot >= InventoryComponent.SlotCount ||
        destinationSlot < 0 || destinationSlot >= InventoryComponent.SlotCount ||
        sourceSlot == destinationSlot || quantity <= 0)
    {
      rejection = ItemCommandRejection.Invalid("The inventory transfer slots or quantity are invalid.");
      return false;
    }

    ItemStack source = inventory.GetSlot(sourceSlot);
    ItemStack destination = inventory.GetSlot(destinationSlot);
    if (source.IsEmpty || quantity > source.Quantity ||
        (!allowWholeSource && quantity >= source.Quantity))
    {
      rejection = ItemCommandRejection.Invalid("The source stack cannot provide the requested quantity.");
      return false;
    }

    if (requireEmptyDestination && !destination.IsEmpty)
    {
      rejection = ItemCommandRejection.Invalid("The destination slot must be empty for a split.");
      return false;
    }

    if (requireNonEmptyDestination && destination.IsEmpty)
    {
      rejection = ItemCommandRejection.Invalid("The destination slot must contain a stack for a merge.");
      return false;
    }

    ItemInstanceStateComponent sourceState = inventory.GetInstanceState(sourceSlot);
    if (destination.IsEmpty)
    {
      inventory.SetSlot(sourceSlot, source.WithQuantity(source.Quantity - quantity));
      inventory.SetSlot(destinationSlot, new ItemStack(source.ItemType, quantity));
      inventory.SetInstanceState(destinationSlot, sourceState);
      rejection = default;
      return true;
    }

    if (!definitions.TryGet(source.ItemType, out ItemDefinition definition) ||
        destination.ItemType != source.ItemType ||
        !InventoryComponent.CanMerge(
          destination,
          inventory.GetInstanceState(destinationSlot),
          source,
          sourceState,
          definition.Identity?.UniqueStack ?? false))
    {
      rejection = ItemCommandRejection.Invalid("The item instances cannot be merged.");
      return false;
    }

    int available = definition.StackLimit - destination.Quantity;
    if (available < quantity)
    {
      rejection = ItemCommandRejection.Invalid("The destination stack cannot accept the quantity.");
      return false;
    }

    inventory.SetSlot(sourceSlot, source.WithQuantity(source.Quantity - quantity));
    inventory.SetSlot(
      destinationSlot,
      destination.WithQuantity(destination.Quantity + quantity));
    rejection = default;
    return true;
  }
}
