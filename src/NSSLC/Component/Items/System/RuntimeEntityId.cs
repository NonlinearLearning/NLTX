using System;
using Terraria.Relationships;

namespace Terraria.Items;

/// <summary>A scoped compatibility projection of an entity reference.</summary>
public readonly record struct RuntimeEntityId
{
  public RuntimeEntityId(EntityReference reference)
  {
    if (!reference.IsEmpty && reference.Scope == EntityReferenceScope.None)
    {
      throw new ArgumentException(
        "A runtime entity ID must retain a scoped entity reference.",
        nameof(reference));
    }

    Reference = reference;
  }

  public static RuntimeEntityId Empty => default;

  public EntityReference Reference { get; }

  public bool IsEmpty => Reference.IsEmpty;

  public Guid ToGuidProjection() =>
    Reference.IsEmpty ? Guid.Empty : Reference.EntityId.Value;

  public static RuntimeEntityId FromEntityReference(EntityReference reference)
  {
    return reference.IsEmpty ? Empty : new RuntimeEntityId(reference);
  }

  public static RuntimeEntityId FromItemReference(ItemEntityRef itemReference)
  {
    return itemReference.IsEmpty
      ? Empty
      : new RuntimeEntityId(itemReference.Reference);
  }

  public bool TryGetEntityReference(out EntityReference reference)
  {
    if (IsEmpty)
    {
      reference = EntityReference.None;
      return false;
    }

    reference = Reference;
    return true;
  }
}
