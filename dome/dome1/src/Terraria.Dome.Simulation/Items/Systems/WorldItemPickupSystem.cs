using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemPickupSystem
{
  private static bool TryPickupWithinRange(
    ref WorldItemComponent item,
    PickupWorldItemCommand command,
    SimulationVector playerPosition,
    InventoryComponent inventory,
    ItemDefinitionRegistry definitions,
    float pickupRange,
    out WorldItemPickedUpEvent pickupEvent,
    out ItemCommandRejection rejection)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    try
    {
      item.InstanceState.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("World item instance state is invalid.");
      return false;
    }

    if (!item.IsActive || item.ReplicationId != command.WorldItemId ||
        item.WorldState.PickupDelayTicks > 0 ||
        item.WorldState.LastOwnerRevision == long.MaxValue ||
        !item.WorldState.CanBePickedUpBy(command.Player) ||
        !float.IsFinite(pickupRange) ||
        !float.IsFinite(item.Position.X) ||
        !float.IsFinite(item.Position.Y) ||
        pickupRange < 0.0f ||
        !float.IsFinite(playerPosition.X) ||
        !float.IsFinite(playerPosition.Y))
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("World item is inactive or pickup is delayed.");
      return false;
    }

    float deltaX = item.Position.X - playerPosition.X;
    float deltaY = item.Position.Y - playerPosition.Y;
    if (pickupRange < 0 || deltaX * deltaX + deltaY * deltaY > pickupRange * pickupRange)
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("Player is outside the world item pickup range.");
      return false;
    }

    if (!definitions.TryGet(item.Stack.ItemType, out ItemDefinition definition) ||
        item.Stack.Quantity > definition.StackLimit)
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("World item stack exceeds its definition limit.");
      return false;
    }

    int remaining = item.Stack.Quantity;
    for (int slot = 0; slot < InventoryComponent.SlotCount && remaining > 0; slot++)
    {
      int before = remaining;
      remaining = new InventoryTransferSystem().TransferIntoSlot(
        inventory,
        slot,
        item.Stack.WithQuantity(remaining),
        item.InstanceState,
        definitions);
      if (remaining == before)
      {
        continue;
      }
    }

    int accepted = item.Stack.Quantity - remaining;
    if (accepted <= 0)
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("Inventory cannot accept the world item stack.");
      return false;
    }

    item = item with
    {
      Stack = item.Stack.WithQuantity(remaining),
      IsActive = remaining > 0,
      Revision = checked(item.Revision + 1),
      WorldState = item.WorldState with
      {
        IsActive = remaining > 0,
        LastOwnerRevision = item.WorldState.LastOwnerRevision + 1,
        Revision = checked(item.WorldState.Revision + 1)
      },
      InstanceState = remaining > 0 ? item.InstanceState : default
    };
    pickupEvent = new WorldItemPickedUpEvent(
      item.ReplicationId,
      command.Player,
      accepted,
      item.Revision);
    rejection = default;
    return true;
  }

  public bool TryPickup(
    ref WorldItemComponent item,
    PickupWorldItemCommand command,
    SimulationVector playerPosition,
    InventoryComponent inventory,
    ItemDefinitionRegistry definitions,
    float defaultPickupRange,
    out WorldItemPickedUpEvent pickupEvent,
    out ItemCommandRejection rejection)
  {
    if (!definitions.TryGet(item.Stack.ItemType, out ItemDefinition definition))
    {
      pickupEvent = default;
      rejection = ItemCommandRejection.Invalid("World item definition is unknown.");
      return false;
    }

    return TryPickupWithinRange(
      ref item,
      command,
      playerPosition,
      inventory,
      definitions,
      definition.GetPickupRange(defaultPickupRange),
      out pickupEvent,
      out rejection);
  }
}
