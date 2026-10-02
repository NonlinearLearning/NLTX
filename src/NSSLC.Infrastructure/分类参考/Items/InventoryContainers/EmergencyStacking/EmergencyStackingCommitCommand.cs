namespace Terraria.Items.InventoryContainers;

public sealed class EmergencyStackingCommitCommand
{
  private readonly HashSet<string> _committedOperations = new(StringComparer.Ordinal);
  private readonly IEmergencyStackingMutationPort _mutationPort;

  public EmergencyStackingCommitCommand(IEmergencyStackingMutationPort mutationPort)
  {
    _mutationPort = mutationPort ?? throw new ArgumentNullException(nameof(mutationPort));
  }

  public EmergencyStackingCommitResult Execute(
    EmergencyStackingTransferPlan plan,
    int currentPlayerIndex,
    string operationKey,
    ItemIdentityAndStackComponent source,
    ItemIdentityAndStackComponent destination)
  {
    ArgumentNullException.ThrowIfNull(plan);
    ArgumentNullException.ThrowIfNull(source);
    ArgumentNullException.ThrowIfNull(destination);
    if (string.IsNullOrWhiteSpace(operationKey))
    {
      throw new ArgumentException("An operation key is required.", nameof(operationKey));
    }

    if (!_committedOperations.Add(operationKey))
    {
      return EmergencyStackingCommitResult.AlreadyCommitted();
    }

    if (!EmergencyStackingOwnershipQuery.HasOwnership(plan, currentPlayerIndex))
    {
      _committedOperations.Remove(operationKey);
      return EmergencyStackingCommitResult.Rejected("ownership-rejected");
    }

    int amount = EmergencyStackingTransferAmountQuery.Calculate(plan);
    if (amount <= 0)
    {
      _committedOperations.Remove(operationKey);
      return EmergencyStackingCommitResult.Rejected("no-transferable-amount");
    }

    if (!MatchesSnapshot(source.CreateSnapshot(), plan.Source.Item)
      || !MatchesSnapshot(destination.CreateSnapshot(), plan.Destination.Item))
    {
      _committedOperations.Remove(operationKey);
      return EmergencyStackingCommitResult.Rejected("revision-or-item-conflict", amount: amount);
    }

    EmergencyStackingMutationResult mutation = _mutationPort.TryTransfer(source, destination, amount);
    if (!mutation.Committed)
    {
      _committedOperations.Remove(operationKey);
      return EmergencyStackingCommitResult.Rejected(
        mutation.FailureReason ?? "mutation-rejected",
        mutation.RolledBack,
        amount);
    }

    return EmergencyStackingCommitResult.Accepted(amount);
  }

  private static bool MatchesSnapshot(ItemStackSnapshot actual, ItemStackSnapshot expected)
  {
    return actual.ContentType == expected.ContentType
      && actual.Stack == expected.Stack
      && actual.MaxStack == expected.MaxStack
      && actual.UniqueStack == expected.UniqueStack
      && actual.Prefix == expected.Prefix
      && actual.Variant == expected.Variant
      && actual.Favorited == expected.Favorited
      && string.Equals(actual.NameOverride, expected.NameOverride, StringComparison.Ordinal)
      && actual.Revision == expected.Revision;
  }
}
