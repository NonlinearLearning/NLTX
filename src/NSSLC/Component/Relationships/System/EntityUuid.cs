using System;

namespace Terraria.Relationships;

public readonly record struct EntityUuid
{
  public EntityUuid(Guid value)
  {
    if (value == Guid.Empty)
    {
      throw new ArgumentException("An entity UUID must be assigned.", nameof(value));
    }

    Value = value;
  }

  public Guid Value { get; }

  public bool IsAssigned => Value != Guid.Empty;
}
