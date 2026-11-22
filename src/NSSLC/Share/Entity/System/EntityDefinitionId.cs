using System;

namespace Terraria.Entity;

/// <summary>
/// Stable business identifier for an entity definition, not a runtime entity identity.
/// </summary>
public readonly record struct EntityDefinitionId {
  public string Value { get; }

  public EntityDefinitionId(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("An entity definition id cannot be empty.", nameof(value));
    }
    Value = value;
  }

  public override string ToString() => Value;
}
