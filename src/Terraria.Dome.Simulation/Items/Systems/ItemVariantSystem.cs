using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class ItemVariantSystem
{
  public bool TryApply(
    ItemStack stack,
    ref ItemInstanceStateComponent state,
    ItemVariantDefinition variant,
    out ItemStack result,
    out ItemCommandRejection rejection,
    IReadOnlyDictionary<string, int>? conditionValues = null)
  {
    if (stack.IsEmpty || variant.VariantId == 0 || variant.SourceItemType != stack.ItemType ||
        variant.ReplacementItemType == 0 || (variant.OneTime && state.VariantId != 0) ||
        !AreConditionsMet(variant.Conditions, conditionValues))
    {
      result = stack;
      rejection = ItemCommandRejection.Invalid("The item variant is not valid for this instance.");
      return false;
    }

    state = state with { VariantId = variant.VariantId };
    result = new ItemStack(variant.ReplacementItemType, stack.Quantity);
    rejection = default;
    return true;
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
