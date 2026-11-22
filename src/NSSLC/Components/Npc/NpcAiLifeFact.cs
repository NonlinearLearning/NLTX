using System;

namespace Terraria.Npc;

/// <summary>Detached health values used by NPC behaviors that change with damage taken.</summary>
public readonly record struct NpcAiLifeFact
{
  public NpcAiLifeFact(int currentLife, int maximumLife)
  {
    if (maximumLife <= 0 || currentLife < 0 || currentLife > maximumLife)
    {
      throw new ArgumentOutOfRangeException(nameof(currentLife));
    }

    IsKnown = true;
    CurrentLife = currentLife;
    MaximumLife = maximumLife;
  }

  public bool IsKnown { get; }

  public int CurrentLife { get; }

  public int MaximumLife { get; }
}
