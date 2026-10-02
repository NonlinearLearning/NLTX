namespace Terraria.Items.InventoryContainers;

public static class QuickStackPlanQuery
{
  public static QuickStackPlan CreatePlan(
    QuickStackSourceSnapshot source,
    IReadOnlyList<QuickStackDestinationSnapshot> destinations)
  {
    return CreatePlan(source, destinations, new QuickStackPlannerScratchState());
  }

  public static QuickStackPlan CreatePlan(
    QuickStackSourceSnapshot source,
    IReadOnlyList<QuickStackDestinationSnapshot> destinations,
    QuickStackPlannerScratchState scratch)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destinations);
    ArgumentNullException.ThrowIfNull(scratch);

    scratch.Reset();
    Dictionary<InventorySlotReference, int> remainingCapacityByDestination = new();
    foreach (QuickStackDestinationSnapshot destination in destinations)
    {
      if (destination.Locked || destination.TransferBlocked)
      {
        scratch.MarkBlocked(destination.ContainerKey);
      }

      scratch.TypeIndex.Add(destination);
      if (destination.Item is not null)
      {
        remainingCapacityByDestination[destination.Reference.Reference] = destination.Item.AvailableCapacity;
      }
    }

    List<QuickStackTransferIntent> intents = new();
    List<InventorySlotReference> remainingSources = new();
    foreach (QuickStackSourceSlotSnapshot sourceSlot in source.Slots)
    {
      if (sourceSlot.TransferBlocked || sourceSlot.Item is null)
      {
        continue;
      }

      ItemStackSnapshot sourceItem = sourceSlot.Item;
      int remainingSourceAmount = sourceItem.Stack;
      foreach (QuickStackDestinationSnapshot destination in scratch.TypeIndex.GetDestinations(sourceItem.ContentType))
      {
        if (destination.Locked || destination.TransferBlocked || destination.Item is null)
        {
          continue;
        }

        if (!ItemDerivedQuery.CanStack(sourceItem, destination.Item))
        {
          continue;
        }

        int remainingCapacity = remainingCapacityByDestination[destination.Reference.Reference];
        int amount = Math.Min(remainingSourceAmount, remainingCapacity);
        if (amount > 0)
        {
          intents.Add(new QuickStackTransferIntent(
            sourceSlot.Reference,
            destination.Reference.Reference,
            amount));
          remainingCapacityByDestination[destination.Reference.Reference] = remainingCapacity - amount;
          remainingSourceAmount -= amount;
          if (remainingSourceAmount == 0)
          {
            break;
          }
        }
      }

      if (remainingSourceAmount > 0)
      {
        remainingSources.Add(sourceSlot.Reference);
      }
    }

    return new QuickStackPlan(intents, remainingSources, scratch.BlockedDestinationKeys);
  }
}
