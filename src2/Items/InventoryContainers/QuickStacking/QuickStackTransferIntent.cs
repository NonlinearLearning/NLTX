namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackTransferIntent
{
  public QuickStackTransferIntent(
    InventorySlotReference source,
    string destinationContainerKey,
    int amount)
    : this(source, new InventorySlotReference(destinationContainerKey, 0), amount)
  {
  }

  public QuickStackTransferIntent(
    InventorySlotReference source,
    InventorySlotReference destination,
    int amount)
  {
    if (!source.IsValid)
    {
      throw new ArgumentException("A transfer intent requires a valid source.", nameof(source));
    }

    if (!destination.IsValid)
    {
      throw new ArgumentException(
        "A transfer intent requires a destination.",
        nameof(destination));
    }

    if (amount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(amount));
    }

    Source = source;
    Destination = destination;
    Amount = amount;
  }

  public InventorySlotReference Source { get; }

  public InventorySlotReference Destination { get; }

  public string DestinationContainerKey => Destination.ContainerKey;

  public int Amount { get; }
}
