using System;

namespace Terraria.Dome.Simulation.Npc.Systems;

public static class NpcTombstoneProjectileTypePolicy
{
  public const int Version1456NpcTypeCount = 697;

  public static int Resolve(int npcType, int randomVariant)
  {
    if (npcType < 0 || npcType >= Version1456NpcTypeCount)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }

    int variantCount = npcType is 17 or 441 ? 5 : 6;
    if (randomVariant < 0 || randomVariant >= variantCount)
    {
      throw new ArgumentOutOfRangeException(nameof(randomVariant));
    }

    if (npcType is 17 or 441)
    {
      return randomVariant + 527;
    }

    return randomVariant == 0 ? 43 : randomVariant + 200;
  }
}
