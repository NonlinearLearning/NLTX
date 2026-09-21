namespace Terraria.Items.InventoryContainers;

public static class QuickStackCommitCommand
{
  public static QuickStackCommitResult Execute(
    ItemIdentityAndStackComponent source,
    ItemIdentityAndStackComponent destination,
    int amount)
  {
    return Execute(source, destination, amount, null, null);
  }

  public static QuickStackCommitResult Execute(
    ItemIdentityAndStackComponent source,
    ItemIdentityAndStackComponent destination,
    int amount,
    long? expectedSourceRevision,
    long? expectedDestinationRevision)
  {
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);
    if (amount <= 0)
    {
      return QuickStackCommitResult.Rejected("amount-must-be-positive");
    }

    if (expectedSourceRevision.HasValue && source.Revision != expectedSourceRevision.Value)
    {
      return QuickStackCommitResult.Rejected("source-revision-conflict");
    }

    if (expectedDestinationRevision.HasValue && destination.Revision != expectedDestinationRevision.Value)
    {
      return QuickStackCommitResult.Rejected("destination-revision-conflict");
    }

    if (!ItemDerivedQuery.CanStack(source.CreateSnapshot(), destination.CreateSnapshot()))
    {
      return QuickStackCommitResult.Rejected("items-cannot-stack");
    }

    if (amount > source.Stack || amount > destination.MaxStack - destination.Stack)
    {
      return QuickStackCommitResult.Rejected("amount-exceeds-capacity");
    }

    int originalSourceStack = source.Stack;
    int originalDestinationStack = destination.Stack;
    if (!source.TrySetStack(originalSourceStack - amount)
      || !destination.TrySetStack(originalDestinationStack + amount))
    {
      source.TrySetStack(originalSourceStack);
      destination.TrySetStack(originalDestinationStack);
      return QuickStackCommitResult.Rejected("stack-update-failed");
    }

    return QuickStackCommitResult.Accepted();
  }
}
