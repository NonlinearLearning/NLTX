using System;

namespace Terraria.Relationships;

public readonly record struct EntityReference(Guid EntityId, EntityReferenceScope Scope)
{
  public static EntityReference None => new(Guid.Empty, EntityReferenceScope.None);

  public bool IsEmpty => EntityId == Guid.Empty;
}
