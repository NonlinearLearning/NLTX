namespace Terraria.Items.InventoryContainers;

public static class EmergencyStackingOwnershipQuery
{
  public static bool HasOwnership(
    EmergencyStackingTransferPlan plan,
    int currentPlayerIndex)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (currentPlayerIndex < 0 || currentPlayerIndex >= WorldItemLifecycleComponent.UnreservedPlayerIndex)
    {
      throw new ArgumentOutOfRangeException(nameof(currentPlayerIndex));
    }

    return plan.Source.OwnerPlayerIndex == currentPlayerIndex
      && plan.Destination.OwnerPlayerIndex == currentPlayerIndex;
  }
}
