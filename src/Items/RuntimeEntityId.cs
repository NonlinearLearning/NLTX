using System;

namespace Terraria.Items;

public readonly record struct RuntimeEntityId(Guid Value)
{
  public static RuntimeEntityId Empty => new(Guid.Empty);

  public bool IsEmpty => Value == Guid.Empty;
}
