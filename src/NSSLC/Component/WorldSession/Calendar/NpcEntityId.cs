using System;

namespace Terraria.WorldSession.Calendar;

// Provisional boundary type; the final owner remains integration-review.
public readonly record struct NpcEntityId(Guid Value)
{
  public bool IsEmpty => Value == Guid.Empty;
}
