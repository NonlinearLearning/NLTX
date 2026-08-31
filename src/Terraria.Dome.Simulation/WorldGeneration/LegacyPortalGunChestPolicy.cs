using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyPortalGunChestPolicy
{
  private const int PortalGunItemType = 3384;

  public static bool TryCreateIntent(
    IReadOnlyList<int> stacks,
    LegacyPassRandomState random,
    out LegacyFrozenChestItemIntent intent)
  {
    ArgumentNullException.ThrowIfNull(stacks);
    ArgumentNullException.ThrowIfNull(random);
    if (stacks.Count < 2 || random.Next(7) != 0 || stacks[1] == 0)
    {
      intent = default;
      return false;
    }

    for (int index = 1; index < stacks.Count; index++)
    {
      if (stacks[index] == 0)
      {
        intent = new LegacyFrozenChestItemIntent(index, PortalGunItemType);
        return true;
      }
    }

    intent = default;
    return false;
  }
}
