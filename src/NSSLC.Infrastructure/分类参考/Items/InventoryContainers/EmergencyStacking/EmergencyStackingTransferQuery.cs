namespace Terraria.Items.InventoryContainers;

public static class EmergencyStackingTransferQuery
{
  public static EmergencyStackingTransferDecision Evaluate(
    EmergencyStackingTransferPlan plan,
    int currentPlayerIndex)
  {
    ArgumentNullException.ThrowIfNull(plan);
    if (currentPlayerIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(currentPlayerIndex));
    }

    bool hasOwnership = EmergencyStackingOwnershipQuery.HasOwnership(plan, currentPlayerIndex);
    if (!hasOwnership)
    {
      return new EmergencyStackingTransferDecision(hasOwnership: false, amount: 0);
    }

    int amount = EmergencyStackingTransferAmountQuery.Calculate(plan);
    return new EmergencyStackingTransferDecision(hasOwnership, amount);
  }
}
