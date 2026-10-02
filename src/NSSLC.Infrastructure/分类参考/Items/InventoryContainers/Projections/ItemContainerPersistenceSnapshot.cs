namespace Terraria.Items.InventoryContainers;

public sealed class ItemContainerPersistenceSnapshot
{
  public ItemContainerPersistenceSnapshot(
    int schemaVersion,
    string containerKey,
    int x,
    int y,
    int index,
    int capacity,
    bool bankChest,
    string? name,
    IReadOnlyList<ItemStackSnapshot?> slots)
  {
    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException("A container key is required.", nameof(containerKey));
    }

    ArgumentNullException.ThrowIfNull(slots);
    SchemaVersion = schemaVersion;
    ContainerKey = containerKey;
    X = x;
    Y = y;
    Index = index;
    Capacity = capacity;
    BankChest = bankChest;
    Name = name;
    Slots = Array.AsReadOnly(slots.ToArray());
  }

  public int SchemaVersion { get; }
  public string ContainerKey { get; }
  public int X { get; }
  public int Y { get; }
  public int Index { get; }
  public int Capacity { get; }
  public bool BankChest { get; }
  public string? Name { get; }
  public IReadOnlyList<ItemStackSnapshot?> Slots { get; }
}
