namespace Terraria.Items.InventoryContainers;

public static class ItemContainerClientProjection
{
  public static ItemContainerClientSnapshot Create(
    ItemContainerNetworkSnapshot network,
    ChestMetadataComponent? metadata = null,
    ChestPresentationStateComponent? presentation = null)
  {
    ArgumentNullException.ThrowIfNull(network);
    return new ItemContainerClientSnapshot(
      network.SchemaVersion,
      network.ContainerKey,
      metadata?.Name,
      metadata?.BankChest ?? false,
      presentation?.Frame ?? 0,
      presentation?.EatingAnimationTime ?? 0,
      network.Slots);
  }
}
