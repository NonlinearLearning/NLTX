using System;

namespace Terraria.WorldSession.Calendar;

// Provisional boundary type; the final owner remains integration-review.
public readonly record struct EntityId(Guid Value)
{
  public bool IsEmpty => Value == Guid.Empty;
}
