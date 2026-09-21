namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackDestinationReferenceSnapshot
{
  public QuickStackDestinationReferenceSnapshot(InventorySlotReference reference)
  {
    if (!reference.IsValid)
    {
      throw new ArgumentException(
        "A destination reference must identify a valid slot.",
        nameof(reference));
    }

    Reference = reference;
  }

  public InventorySlotReference Reference { get; }
}
