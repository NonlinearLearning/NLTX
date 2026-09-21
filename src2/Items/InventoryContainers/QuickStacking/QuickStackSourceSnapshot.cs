namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackSourceSnapshot
{
  private QuickStackSourceSnapshot(
    string containerKey,
    InventoryPosition position,
    IReadOnlyList<QuickStackSourceSlotSnapshot> slots)
  {
    ContainerKey = containerKey;
    PositionSnapshot = new QuickStackSourcePositionSnapshot(position);
    Slots = Array.AsReadOnly(slots.ToArray());
    SlotReferences = new QuickStackSourceSlotReferenceSnapshot(
      Slots.Select(slot => slot.Reference).ToArray());
  }

  public string ContainerKey { get; }

  public QuickStackSourcePositionSnapshot PositionSnapshot { get; }

  public InventoryPosition Position => PositionSnapshot.Position;

  public IReadOnlyList<QuickStackSourceSlotSnapshot> Slots { get; }

  public QuickStackSourceSlotReferenceSnapshot SlotReferences { get; }

  public int NumItems => Slots.Count;

  public static QuickStackSourceSnapshot Create(
    string containerKey,
    InventoryPosition position,
    params QuickStackSourceSlotSnapshot[] slots)
  {
    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException("A source snapshot requires a container key.", nameof(containerKey));
    }

    ArgumentNullException.ThrowIfNull(slots);
    return new QuickStackSourceSnapshot(
      containerKey,
      position,
      Array.AsReadOnly((QuickStackSourceSlotSnapshot[])slots.Clone()));
  }
}
