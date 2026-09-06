using System;

namespace Terraria.Items;

public readonly record struct PersistentItemId(Guid Value)
{
  public static PersistentItemId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
