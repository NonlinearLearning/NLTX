namespace Terraria.Items.InventoryContainers;

public sealed class ItemContainerClientSnapshot
{
  public ItemContainerClientSnapshot(
    int schemaVersion,
    string containerKey,
    string? name,
    bool bankChest,
    int frame,
    int eatingAnimationTime,
    IReadOnlyList<ItemStackSnapshot?> slots)
  {
    SchemaVersion = schemaVersion;
    ContainerKey = containerKey;
    Name = name;
    BankChest = bankChest;
    Frame = frame;
    EatingAnimationTime = eatingAnimationTime;
    Slots = Array.AsReadOnly((slots ?? throw new ArgumentNullException(nameof(slots))).ToArray());
  }

  public int SchemaVersion { get; }
  public string ContainerKey { get; }
  public string? Name { get; }
  public bool BankChest { get; }
  public int Frame { get; }
  public int EatingAnimationTime { get; }
  public IReadOnlyList<ItemStackSnapshot?> Slots { get; }
}
