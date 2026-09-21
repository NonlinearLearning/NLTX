namespace Terraria.Items.InventoryContainers;

public sealed class ItemContainerNetworkSnapshot
{
  public ItemContainerNetworkSnapshot(
    int schemaVersion,
    string containerKey,
    IReadOnlyList<ItemStackSnapshot?> slots)
  {
    if (schemaVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(schemaVersion));
    }

    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException("A container key is required.", nameof(containerKey));
    }

    ArgumentNullException.ThrowIfNull(slots);
    SchemaVersion = schemaVersion;
    ContainerKey = containerKey;
    Slots = Array.AsReadOnly(slots.ToArray());
  }

  public int SchemaVersion { get; }
  public string ContainerKey { get; }
  public IReadOnlyList<ItemStackSnapshot?> Slots { get; }
}
