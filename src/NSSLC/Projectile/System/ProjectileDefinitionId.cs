using System;

namespace Terraria.Projectile;

/// <summary>Stable content identifier for a projectile definition.</summary>
public readonly record struct ProjectileDefinitionId
{
  public ProjectileDefinitionId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("A projectile definition id cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
