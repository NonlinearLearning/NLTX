namespace Terraria.Relationships;

/// <summary>Identifies one item instance in one entity runtime.</summary>
public readonly record struct ItemEntityRef
{
  private ItemEntityRef(EntityReference reference)
  {
    if (reference.IsEmpty || reference.Scope != EntityReferenceScope.Item)
    {
      throw new ArgumentException(
        "An item reference must contain a non-empty Item-scoped entity reference.",
        nameof(reference));
    }

    Reference = reference;
  }

  public static ItemEntityRef None => default;

  public EntityReference Reference { get; }

  public EntityUuid EntityId => Reference.EntityId;

  public EntityRuntimeId RuntimeId => Reference.RuntimeId;

  public bool IsEmpty => Reference.IsEmpty;

  public static ItemEntityRef FromReference(EntityReference reference)
  {
    return reference.IsEmpty ? None : new ItemEntityRef(reference);
  }
}
