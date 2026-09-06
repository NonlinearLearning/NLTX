using System;

namespace Terraria.Items;

public readonly record struct ExternalAccountId(Guid Value)
{
  public static ExternalAccountId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
