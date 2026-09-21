namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingComponentMutationPort : IEmergencyStackingMutationPort
{
  public EmergencyStackingMutationResult TryTransfer(
    ItemIdentityAndStackComponent source,
    ItemIdentityAndStackComponent destination,
    int amount)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);
    if (ReferenceEquals(source, destination))
    {
      return EmergencyStackingMutationResult.Rejected("source-and-destination-must-differ");
    }

    if (amount <= 0)
    {
      return EmergencyStackingMutationResult.Rejected("amount-must-be-positive");
    }

    if (!ItemDerivedQuery.CanStack(source.CreateSnapshot(), destination.CreateSnapshot())
      || amount > source.Stack
      || amount > destination.MaxStack - destination.Stack)
    {
      return EmergencyStackingMutationResult.Rejected("amount-or-capacity-invalid");
    }

    int originalSourceStack = source.Stack;
    int originalDestinationStack = destination.Stack;
    if (!source.TrySetStack(originalSourceStack - amount))
    {
      return EmergencyStackingMutationResult.Rejected("source-update-failed");
    }

    if (!destination.TrySetStack(originalDestinationStack + amount))
    {
      source.TrySetStack(originalSourceStack);
      return EmergencyStackingMutationResult.Rejected("destination-update-failed", rolledBack: true);
    }

    return EmergencyStackingMutationResult.Accepted();
  }
}
