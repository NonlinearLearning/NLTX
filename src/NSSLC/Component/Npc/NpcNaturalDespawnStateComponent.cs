using System;

namespace Terraria.Npc;

public struct NpcNaturalDespawnStateComponent
{
  public NpcNaturalDespawnStateComponent(
    bool isNaturallySpawned,
    int ticksOutsidePlayerRange = 0)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(ticksOutsidePlayerRange);
    IsNaturallySpawned = isNaturallySpawned;
    TicksOutsidePlayerRange = ticksOutsidePlayerRange;
  }

  public bool IsNaturallySpawned;

  public int TicksOutsidePlayerRange;
}
