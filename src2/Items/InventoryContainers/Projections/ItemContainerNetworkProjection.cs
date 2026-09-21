namespace Terraria.Items.InventoryContainers;

public static class ItemContainerNetworkProjection
{
  public static ItemContainerNetworkSnapshot Create(
    string containerKey,
    ChestSlotStorageComponent chest)
  {
    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException("A container key is required.", nameof(containerKey));
    }

    ArgumentNullException.ThrowIfNull(chest);
    ItemStackSnapshot?[] slots = chest.Slots.ToArray();
    return new ItemContainerNetworkSnapshot(
      schemaVersion: 1,
      containerKey,
      Array.AsReadOnly(slots));
  }
}
