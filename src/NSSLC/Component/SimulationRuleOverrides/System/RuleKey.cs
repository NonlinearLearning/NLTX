using System;

namespace Terraria.SimulationRuleOverrides;

public readonly record struct RuleKey
{
  public RuleKey(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A rule key must not be blank.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
