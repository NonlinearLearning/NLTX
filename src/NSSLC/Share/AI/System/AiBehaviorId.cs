using System;

namespace Terraria.Share.Ai;

/// <summary>Stable content key selecting an AI behavior implementation.</summary>
public readonly record struct AiBehaviorId
{
  public AiBehaviorId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("An AI behavior id cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
