namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackSourceSlotReferenceSnapshot
{
  public QuickStackSourceSlotReferenceSnapshot(IReadOnlyList<InventorySlotReference> references)
  {
    ArgumentNullException.ThrowIfNull(references);
    if (references.Any(reference => !reference.IsValid))
    {
      throw new ArgumentException(
        "A source reference snapshot cannot contain an invalid slot.",
        nameof(references));
    }

    References = Array.AsReadOnly(references.ToArray());
  }

  public IReadOnlyList<InventorySlotReference> References { get; }
}
