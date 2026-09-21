using System;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Snapshots;

namespace Terraria.Dome.Server.Replication;

internal static class ItemReplicationAdmission
{
  public static ItemDefinition? ValidateInstance(
    ItemInstanceSnapshot instance,
    ItemDefinitionRegistry definitions)
  {
    ArgumentNullException.ThrowIfNull(instance);
    ArgumentNullException.ThrowIfNull(definitions);

    instance.State.Validate();
    if (instance.Stack.IsEmpty)
    {
      if (instance.Stack != ItemStack.Empty || instance.State != default)
      {
        throw new InvalidOperationException(
          "An empty item snapshot cannot carry non-canonical state.");
      }

      return null;
    }

    if (instance.Stack.Quantity <= 0 ||
        !definitions.TryGet(instance.Stack.ItemType, out ItemDefinition definition) ||
        instance.Stack.Quantity > definition.StackLimit)
    {
      throw new InvalidOperationException(
        "An inventory item snapshot does not match an authoritative item definition.");
    }

    if (instance.Stack.ItemType > short.MaxValue || instance.Stack.Quantity > short.MaxValue ||
        instance.State.PrefixId > byte.MaxValue)
    {
      throw new InvalidOperationException(
        "An item snapshot cannot be represented by the V1456 equipment packet.");
    }

    return definition;
  }

  public static ItemDefinition ValidateEquipmentInstance(
    ItemInstanceSnapshot instance,
    ItemEquipmentStateComponent state,
    ItemDefinitionRegistry definitions)
  {
    ItemDefinition? definition = ValidateInstance(instance, definitions);
    if (definition is null || definition.Value.Equipment is not ItemEquipmentDefinition equipment ||
        equipment.Slot != state.Slot || state.IsVanity && !equipment.Vanity)
    {
      throw new InvalidOperationException(
        "An equipment source item does not match its authoritative equipment definition.");
    }

    return definition.Value;
  }
}
