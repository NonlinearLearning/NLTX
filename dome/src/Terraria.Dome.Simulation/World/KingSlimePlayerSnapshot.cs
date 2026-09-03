using System;

namespace Terraria.Dome.Simulation.WorldModel;

public readonly record struct KingSlimePlayerSnapshot
{
  public KingSlimePlayerSnapshot(bool isActive, int maximumHealth, int defense)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maximumHealth);
    ArgumentOutOfRangeException.ThrowIfNegative(defense);

    IsActive = isActive;
    MaximumHealth = maximumHealth;
    Defense = defense;
  }

  public int Defense { get; }
  public bool IsActive { get; }
  public int MaximumHealth { get; }
}
