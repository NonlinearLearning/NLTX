using System;
using Terraria.Dome.Simulation.Items.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public sealed class WorldItemStackingSystem
{
  public bool TryMerge(
    WorldItemComponent receiver,
    WorldItemComponent donor,
    ItemDefinitionRegistry definitions,
    long tick,
    float maximumDistance,
    out WorldItemComponent mergedReceiver,
    out WorldItemComponent mergedDonor)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (tick < 0 || !float.IsFinite(maximumDistance) || maximumDistance < 0.0f ||
        !float.IsFinite(receiver.Position.X) || !float.IsFinite(receiver.Position.Y) ||
        !float.IsFinite(donor.Position.X) || !float.IsFinite(donor.Position.Y) ||
        receiver.ReplicationId >= donor.ReplicationId ||
        !receiver.IsActive || !donor.IsActive || receiver.Section != donor.Section ||
        receiver.WorldState.PickupDelayTicks > 0 || donor.WorldState.PickupDelayTicks > 0 ||
        receiver.WorldState.LastMergeTick == tick || donor.WorldState.LastMergeTick == tick ||
        receiver.Stack.IsEmpty || donor.Stack.IsEmpty ||
        !definitions.TryGet(receiver.Stack.ItemType, out ItemDefinition definition) ||
        receiver.Stack.ItemType != donor.Stack.ItemType ||
        !InventoryComponent.CanMerge(
          receiver.Stack,
          receiver.InstanceState,
          donor.Stack,
          donor.InstanceState,
          definition.Identity?.UniqueStack ?? false))
    {
      mergedReceiver = receiver;
      mergedDonor = donor;
      return false;
    }

    float deltaX = receiver.Position.X - donor.Position.X;
    float deltaY = receiver.Position.Y - donor.Position.Y;
    if (deltaX * deltaX + deltaY * deltaY > maximumDistance * maximumDistance)
    {
      mergedReceiver = receiver;
      mergedDonor = donor;
      return false;
    }

    int available = definition.StackLimit - receiver.Stack.Quantity;
    int accepted = Math.Min(available, donor.Stack.Quantity);
    if (accepted <= 0)
    {
      mergedReceiver = receiver;
      mergedDonor = donor;
      return false;
    }

    mergedReceiver = receiver with
    {
      Stack = receiver.Stack.WithQuantity(receiver.Stack.Quantity + accepted),
      Revision = checked(receiver.Revision + 1),
      WorldState = receiver.WorldState with
      {
        LastMergeTick = tick,
        Revision = checked(receiver.WorldState.Revision + 1)
      }
    };

    ItemStack donorStack = donor.Stack.WithQuantity(donor.Stack.Quantity - accepted);
    mergedDonor = donor with
    {
      Stack = donorStack,
      IsActive = !donorStack.IsEmpty,
      Revision = checked(donor.Revision + 1),
      InstanceState = donorStack.IsEmpty ? default : donor.InstanceState,
      WorldState = donor.WorldState with
      {
        IsActive = !donorStack.IsEmpty,
        LastMergeTick = tick,
        Revision = checked(donor.WorldState.Revision + 1)
      }
    };
    return true;
  }
}
