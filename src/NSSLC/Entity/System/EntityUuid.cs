using System;

namespace Terraria.Relationships;

/// <summary>
/// Server-owned identity for one authoritative entity instance.
/// </summary>
public readonly record struct EntityUuid {
  public Guid Value { get; }

  public bool IsValid => Value != Guid.Empty;

  private EntityUuid(Guid value) {
    Value = value;
  }

  public static EntityUuid Create() {
    return new EntityUuid(Guid.NewGuid());
  }

  public static EntityUuid From(Guid value) {
    if (value == Guid.Empty) {
      throw new ArgumentException("An entity UUID cannot be empty.", nameof(value));
    }
    return new EntityUuid(value);
  }

  public override string ToString() {
    return Value.ToString("D");
  }
}
