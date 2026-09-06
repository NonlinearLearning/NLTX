using System;
using Terraria.Relationships;

namespace EntityEcs.Components;

// status: proposed
public sealed class EntityRelationState
{
  public EntityRelationState(
    EntityReference relatedEntity = default,
    EntityRelationKind relationKind = EntityRelationKind.None,
    long expectedRevision = 0,
    long? attachedAtTick = null)
  {
    if (expectedRevision < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(expectedRevision));
    }

    if (attachedAtTick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(attachedAtTick));
    }

    RelatedEntity = relatedEntity;
    RelationKind = relationKind;
    ExpectedRevision = expectedRevision;
    AttachedAtTick = attachedAtTick;
    Validate();
  }

  public EntityReference RelatedEntity { get; }

  public EntityRelationKind RelationKind { get; }

  public long ExpectedRevision { get; }

  public long? AttachedAtTick { get; }

  public bool IsAttached =>
    RelationKind != EntityRelationKind.None
    && !RelatedEntity.IsEmpty;

  public bool IsDetached => !IsAttached;

  public void Validate()
  {
    if (RelationKind == EntityRelationKind.None
      && !RelatedEntity.IsEmpty)
    {
      throw new InvalidOperationException(
        "A relation kind is required for a non-empty entity reference.");
    }

    if (RelationKind != EntityRelationKind.None
      && RelatedEntity.IsEmpty)
    {
      throw new InvalidOperationException(
        "An attached relation requires a related entity.");
    }
  }
}
