namespace Terraria.Items.InventoryContainers;

public static class ItemContainerPersistenceProjection
{
  public static ItemContainerPersistenceSnapshot Create(
    string containerKey,
    ChestMetadataComponent metadata,
    ChestSlotStorageComponent storage)
  {
    ArgumentNullException.ThrowIfNull(metadata);
    ArgumentNullException.ThrowIfNull(storage);
    return new ItemContainerPersistenceSnapshot(
      schemaVersion: 1,
      containerKey,
      metadata.X,
      metadata.Y,
      metadata.Index,
      metadata.Capacity,
      metadata.BankChest,
      metadata.Name,
      storage.Slots);
  }
}
