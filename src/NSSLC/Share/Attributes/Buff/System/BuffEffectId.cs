using System;

namespace Terraria.Attributes;

/// <summary>Stable definition identifier for an active status effect.</summary>
public readonly record struct BuffEffectId {
  public string Value { get; }

  public BuffEffectId(string value) : this() {
    if (string.IsNullOrWhiteSpace(value)) {
      throw new ArgumentException("A buff effect id cannot be empty.", nameof(value));
    }
    Value = value;
  }
}
