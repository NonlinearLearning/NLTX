using System;

namespace Terraria.Npc;

public sealed class NpcHealthComponent
{
  public NpcHealthComponent(int currentLife, int maximumLife)
  {
    if (maximumLife < 0)
    {
      throw new ArgumentOutOfRangeException(
        nameof(maximumLife),
        "Maximum NPC life cannot be negative.");
    }

    if (currentLife < 0 || currentLife > maximumLife)
    {
      throw new ArgumentOutOfRangeException(
        nameof(currentLife),
        "Current NPC life must be within the NPC health bounds.");
    }

    CurrentLife = currentLife;
    MaximumLife = maximumLife;
  }

  public int CurrentLife { get; }

  public int MaximumLife { get; }

  public bool IsDead => CurrentLife <= 0;
}
