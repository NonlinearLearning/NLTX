using System;

namespace Terraria.Items;

public readonly record struct PersistentContainerId(Guid Value)
{
  public static PersistentContainerId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
