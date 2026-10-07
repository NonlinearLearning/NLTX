namespace Terraria.Relationships;

public readonly record struct EntityReference
{
  public EntityReference(
    EntityUuid entityId,
    EntityRuntimeId runtimeId,
    EntityReferenceScope scope)
  {
    if (!Enum.IsDefined(scope))
    {
      throw new ArgumentOutOfRangeException(nameof(scope));
    }

    if (scope == EntityReferenceScope.None)
    {
      if (entityId.IsAssigned || runtimeId.IsAssigned)
      {
        throw new ArgumentException("An empty reference cannot carry an identity.", nameof(scope));
      }
    }
    else if (!entityId.IsAssigned || !runtimeId.IsAssigned)
    {
      throw new ArgumentException("A scoped entity reference requires both identities.");
    }

    EntityId = entityId;
    RuntimeId = runtimeId;
    Scope = scope;
  }

  public static EntityReference None => default;

  public EntityUuid EntityId { get; }

  public EntityRuntimeId RuntimeId { get; }

  public EntityReferenceScope Scope { get; }

  public bool IsEmpty => !EntityId.IsAssigned;
}
