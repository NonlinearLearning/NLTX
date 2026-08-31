using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemVariantSystem
{
  private readonly ItemDefinitionRegistry? _itemDefinitions;

  public ItemVariantSystem()
  {
  }

  public ItemVariantSystem(ItemDefinitionRegistry itemDefinitions)
  {
    _itemDefinitions = itemDefinitions ?? throw new ArgumentNullException(nameof(itemDefinitions));
  }

  public bool TryApply(
    ItemStack stack,
    ref ItemInstanceStateComponent state,
    ItemVariantDefinition variant,
    out ItemStack result,
    out ItemCommandRejection rejection,
    IReadOnlyDictionary<string, int>? conditionValues = null)
  {
    try
    {
      state.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      result = stack;
      rejection = ItemCommandRejection.Invalid("The item instance state is invalid.");
      return false;
    }

    if (stack.IsEmpty || variant.VariantId == 0 || variant.SourceItemType != stack.ItemType ||
        variant.ReplacementItemType == 0 || (variant.OneTime && state.VariantId != 0) ||
        !IsItemDefinitionCompatible(stack.ItemType, stack.Quantity) ||
        !IsItemDefinitionCompatible(variant.ReplacementItemType, stack.Quantity) ||
        !AreConditionsMet(variant.Conditions, conditionValues))
    {
      result = stack;
      rejection = ItemCommandRejection.Invalid("The item variant is not valid for this instance.");
      return false;
    }

    state = state with { VariantId = variant.VariantId };
    result = new ItemStack(variant.ReplacementItemType, stack.Quantity, stack.Prefix);
    rejection = default;
    return true;
  }

  private bool IsItemDefinitionCompatible(ushort itemType, int quantity)
  {
    return _itemDefinitions is null ||
      _itemDefinitions.TryGet(itemType, out ItemDefinition definition) &&
      quantity <= definition.StackLimit;
  }

  private static bool AreConditionsMet(
    IReadOnlyList<ItemDropCondition>? conditions,
    IReadOnlyDictionary<string, int>? conditionValues)
  {
    if (conditions is null)
    {
      return true;
    }

    for (int index = 0; index < conditions.Count; index++)
    {
      ItemDropCondition condition = conditions[index];
      bool matches = !string.IsNullOrWhiteSpace(condition.Key) &&
        conditionValues is not null &&
        conditionValues.TryGetValue(condition.Key, out int value) &&
        value >= condition.MinimumValue;
      if (condition.Invert)
      {
        matches = !matches;
      }

      if (!matches)
      {
        return false;
      }
    }

    return true;
  }
}
