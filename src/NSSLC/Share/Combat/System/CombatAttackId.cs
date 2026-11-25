using System;

namespace Terraria.Share.Combat;

/// <summary>Stable content identifier for an attack definition.</summary>
public readonly record struct CombatAttackId
{
  public CombatAttackId(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      throw new ArgumentException("An attack definition id cannot be empty.", nameof(value));
    }

    Value = value;
  }

  public string Value { get; }

  public ulong StableKey
  {
    get
    {
      unchecked
      {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;
        for (int index = 0; index < Value.Length; index++)
        {
          hash ^= Value[index];
          hash *= prime;
        }

        return hash == 0 ? 1 : hash;
      }
    }
  }

  public bool IsValid => !string.IsNullOrWhiteSpace(Value);

  public override string ToString() => Value;
}
