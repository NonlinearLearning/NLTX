using System;

namespace Terraria.Share.Entities;

/// <summary>
/// Stable business identifier for an entity definition, not a runtime entity identity.
/// </summary>
public readonly record struct EntityId {
  public string Value { get; }

  public EntityId(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("An entity definition id cannot be empty.", nameof(value));
    }
    Value = value;
  }

  public override string ToString() => Value;
}
