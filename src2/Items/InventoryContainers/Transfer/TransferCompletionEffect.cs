namespace Terraria.Items.InventoryContainers;

public sealed class TransferCompletionEffect
{
  public TransferCompletionEffect(
    string operationKey,
    string sourceContainerKey,
    string destinationContainerKey,
    ItemStackSnapshot? committedItem,
    ItemTransferPostAction postAction = ItemTransferPostAction.None)
  {
    if (string.IsNullOrWhiteSpace(operationKey))
    {
      throw new ArgumentException("An operation key is required.", nameof(operationKey));
    }

    OperationKey = operationKey;
    SourceContainerKey = sourceContainerKey;
    DestinationContainerKey = destinationContainerKey;
    CommittedItem = committedItem;
    PostAction = postAction;
  }

  public string OperationKey { get; }

  public string SourceContainerKey { get; }

  public string DestinationContainerKey { get; }

  public ItemStackSnapshot? CommittedItem { get; }

  public ItemTransferPostAction PostAction { get; }
}
