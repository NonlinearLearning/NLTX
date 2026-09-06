using System;

namespace Terraria.Items;

public readonly record struct OperationId(Guid Value)
{
  public static OperationId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
