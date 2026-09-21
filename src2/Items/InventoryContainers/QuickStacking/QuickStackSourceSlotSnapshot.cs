namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackSourceSlotSnapshot
{
  public QuickStackSourceSlotSnapshot(
    InventorySlotReference reference,
    ItemStackSnapshot? item,
    bool transferBlocked = false)
  {
    if (!reference.IsValid)
    {
      throw new ArgumentException("A source slot reference must identify a valid slot.", nameof(reference));
    }

    Reference = reference;
    Item = item;
    Eligibility = new QuickStackSourceEligibility(transferBlocked);
  }

  public InventorySlotReference Reference { get; }

  public ItemStackSnapshot? Item { get; }

  public QuickStackSourceEligibility Eligibility { get; }

  public bool TransferBlocked => Eligibility.TransferBlocked;
}
