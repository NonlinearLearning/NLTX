using System;

namespace Terraria.Item;

/// <summary>
/// Stable identifier for an item definition. It is not an entity identity.
/// </summary>
public readonly record struct ItemDefinitionId {
  public ItemDefinitionId(string value) {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("An item definition id cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
