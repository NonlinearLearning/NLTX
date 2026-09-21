namespace Terraria.Items.InventoryContainers;

public static class EmergencyStackingTransferAmountQuery
{
  public static int Calculate(EmergencyStackingTransferPlan plan)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (!ItemDerivedQuery.CanStack(plan.Source.Item, plan.Destination.Item))
    {
      return 0;
    }

    return Math.Min(plan.Source.Item.Stack, plan.Destination.Item.AvailableCapacity);
  }
}
