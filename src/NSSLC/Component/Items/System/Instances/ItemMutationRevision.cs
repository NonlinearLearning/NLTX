using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>Optimistic version of an item's instance and stack component cells.</summary>
public readonly record struct ItemMutationRevision
{
  public ItemMutationRevision(
    EntityReference itemReference,
    long instanceAttachmentRevision,
    long instanceDataRevision,
    long stackAttachmentRevision,
    long stackDataRevision)
  {
    if (itemReference.IsEmpty || itemReference.Scope != EntityReferenceScope.Item)
    {
      throw new ArgumentException(
        "An item mutation revision must be bound to an Item-scoped reference.",
        nameof(itemReference));
    }

    if (instanceAttachmentRevision <= 0 || instanceDataRevision <= 0 ||
        stackAttachmentRevision <= 0 || stackDataRevision <= 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(instanceAttachmentRevision),
        "Attached item component revisions must be positive.");
    }

    ItemReference = itemReference;
    InstanceAttachmentRevision = instanceAttachmentRevision;
    InstanceDataRevision = instanceDataRevision;
    StackAttachmentRevision = stackAttachmentRevision;
    StackDataRevision = stackDataRevision;
  }

  public EntityReference ItemReference { get; }

  public long InstanceAttachmentRevision { get; }

  public long InstanceDataRevision { get; }

  public long StackAttachmentRevision { get; }

  public long StackDataRevision { get; }

  public bool IsAssigned =>
    !ItemReference.IsEmpty &&
    ItemReference.Scope == EntityReferenceScope.Item &&
    InstanceAttachmentRevision > 0 &&
    InstanceDataRevision > 0 &&
    StackAttachmentRevision > 0 &&
    StackDataRevision > 0;
}
