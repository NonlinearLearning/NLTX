using System;

namespace Terraria.Items;

public readonly record struct TransactionId(Guid Value)
{
  public static TransactionId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
