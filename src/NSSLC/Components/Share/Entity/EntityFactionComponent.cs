using System;

namespace Terraria.Combat;

/// <summary>Identifies a broad relationship group used by combat queries.</summary>
public enum Faction : byte {
  Neutral = 0,
  Friendly = 1,
  Hostile = 2
}

/// <summary>Stores the entity's own faction; relationship rules belong to combat systems.</summary>
public readonly record struct EntityFactionComponent {
  public Faction Value { get; }

  public EntityFactionComponent(Faction value) : this() {
    if (!Enum.IsDefined(value)) {
      throw new ArgumentOutOfRangeException(nameof(value));
    }
    Value = value;
  }
}
