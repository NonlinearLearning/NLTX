namespace Terraria.Items.InventoryContainers;

public static class ItemTransferCommand
{
  private static readonly HashSet<string> CommittedOperations = new();

  public static TransferCommitResult Commit(
    string operationKey,
    ItemTransferPolicyDefinition policy,
    TransferCompletionEffectPort effectPort,
    TransferCompletionEffect effect)
  {
    if (string.IsNullOrWhiteSpace(operationKey))
    {
      throw new ArgumentException("An operation key is required.", nameof(operationKey));
    }

    ArgumentNullException.ThrowIfNull(policy);
    ArgumentNullException.ThrowIfNull(effectPort);
    ArgumentNullException.ThrowIfNull(effect);

    if (!string.Equals(operationKey, effect.OperationKey, StringComparison.Ordinal))
    {
      return TransferCommitResult.Rejected("effect-operation-key-mismatch");
    }

    lock (CommittedOperations)
    {
      if (!CommittedOperations.Add(operationKey))
      {
        return TransferCommitResult.AlreadyCommitted();
      }
    }

    TransferCompletionEffect completionEffect = effect;
    if (effect.PostAction == ItemTransferPostAction.None
      && policy.PostAction != ItemTransferPostAction.None)
    {
      completionEffect = new TransferCompletionEffect(
        effect.OperationKey,
        effect.SourceContainerKey,
        effect.DestinationContainerKey,
        effect.CommittedItem,
        policy.PostAction);
    }

    effectPort.ApplyOnce(completionEffect);
    return TransferCommitResult.Accepted();
  }
}
