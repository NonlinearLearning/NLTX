using System;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemPrefixSystem
{
  private readonly ItemDefinitionRegistry? _itemDefinitions;

  public ItemPrefixSystem()
  {
  }

  public ItemPrefixSystem(ItemDefinitionRegistry itemDefinitions)
  {
    _itemDefinitions = itemDefinitions ??
      throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public bool TryApply(
    ItemStack stack,
    ItemDefinition definition,
    ref ItemInstanceStateComponent state,
    ushort prefixId,
    out ItemPrefixChangedEvent changed,
    out ItemCommandRejection rejection)
  {
    try
    {
      state.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      changed = default;
      rejection = ItemCommandRejection.Invalid("The item instance state is invalid.");
      return false;
    }

    if (stack.IsEmpty || stack.Quantity > definition.StackLimit ||
        !IsAuthoritativeDefinition(stack, definition))
    {
      changed = default;
      rejection = ItemCommandRejection.Invalid("An empty item cannot receive a prefix.");
      return false;
    }

    if (definition.ItemType != stack.ItemType)
    {
      changed = default;
      rejection = ItemCommandRejection.Invalid(
        "The item definition does not match the item stack.");
      return false;
    }

    if (prefixId != 0 &&
        (definition.Prefixes is null || !definition.Prefixes.CanApply(prefixId)))
    {
      changed = default;
      rejection = ItemCommandRejection.Invalid(
        "The requested prefix is not eligible for this item.");
      return false;
    }

    ushort previous = state.PrefixId;
    state = state with { PrefixId = prefixId };
    changed = new ItemPrefixChangedEvent(stack.ItemType, previous, prefixId);
    rejection = default;
    return true;
  }

  private bool IsAuthoritativeDefinition(ItemStack stack, ItemDefinition definition)
  {
    return _itemDefinitions is null ||
      _itemDefinitions.TryGet(stack.ItemType, out ItemDefinition registeredDefinition) &&
      registeredDefinition == definition;
  }
}
