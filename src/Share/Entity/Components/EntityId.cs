using System;

namespace EntityEcs.Components;

public readonly record struct EntityId(Guid Value)
{
  public static EntityId None => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
