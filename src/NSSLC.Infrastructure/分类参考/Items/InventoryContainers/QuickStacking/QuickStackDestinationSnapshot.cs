namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackDestinationSnapshot
{
  private QuickStackDestinationSnapshot(
    QuickStackDestinationReferenceSnapshot reference,
    QuickStackDestinationEligibility eligibility,
    ItemStackSnapshot? item)
  {
    Reference = reference;
    Eligibility = eligibility;
    Item = item;
  }

  public QuickStackDestinationReferenceSnapshot Reference { get; }

  public QuickStackDestinationEligibility Eligibility { get; }

  public string ContainerKey => Reference.Reference.ContainerKey;

  public bool Locked => Eligibility.Locked;

  public bool TransferBlocked => Eligibility.TransferBlocked;

  public ItemStackSnapshot? Item { get; }

  public static QuickStackDestinationSnapshot Create(
    string containerKey,
    bool locked,
    bool transferBlocked,
    ItemStackSnapshot? item)
  {
    if (string.IsNullOrWhiteSpace(containerKey))
    {
      throw new ArgumentException(
        "A destination snapshot requires a container key.",
        nameof(containerKey));
    }

    return Create(
      new InventorySlotReference(containerKey, 0),
      locked,
      transferBlocked,
      item);
  }

  public static QuickStackDestinationSnapshot Create(
    InventorySlotReference reference,
    bool locked,
    bool transferBlocked,
    ItemStackSnapshot? item)
  {
    return new QuickStackDestinationSnapshot(
      new QuickStackDestinationReferenceSnapshot(reference),
      new QuickStackDestinationEligibility(locked, transferBlocked),
      item);
  }
}
