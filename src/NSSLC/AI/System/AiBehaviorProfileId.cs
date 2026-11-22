using System;

namespace Terraria.Ai;

/// <summary>Stable content key selecting immutable parameters for an AI behavior.</summary>
public readonly record struct AiBehaviorProfileId
{
  public AiBehaviorProfileId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException(
          "An AI behavior profile id cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
