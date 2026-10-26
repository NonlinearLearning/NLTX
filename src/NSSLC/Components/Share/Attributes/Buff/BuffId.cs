using System;

namespace Terraria.Combat;

/// <summary>Stable definition identifier for an immunity entry.</summary>
public readonly record struct BuffId {
  public string Value { get; }

  public BuffId(string value) : this() {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("A buff id cannot be empty.", nameof(value));
    }
    Value = value;
  }
}
