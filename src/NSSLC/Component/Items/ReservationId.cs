using System;

namespace Terraria.Items;

public readonly record struct ReservationId(Guid Value)
{
  public static ReservationId Empty => new(Guid.Empty);

  public bool IsAssigned => Value != Guid.Empty;
}
