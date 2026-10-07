using System;

namespace Terraria.Relationships;

public readonly record struct EntityRuntimeId
{
  public EntityRuntimeId(Guid value)
  {
    if (value == Guid.Empty)
    {
      throw new ArgumentException("A runtime ID must be assigned.", nameof(value));
    }

    Value = value;
  }

  public Guid Value { get; }

  public bool IsAssigned => Value != Guid.Empty;
}
