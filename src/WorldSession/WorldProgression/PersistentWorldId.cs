using System;

namespace Terraria.WorldProgression.Components;

public readonly record struct PersistentWorldId(Guid Value)
{
  public bool IsEmpty => Value == Guid.Empty;
}
