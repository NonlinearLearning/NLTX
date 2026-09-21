using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public readonly record struct LegacyFrozenChestItemIntent(int SlotIndex, int ItemType);

public readonly record struct LegacyFrozenNpcReplacementIntent(
  int SourceNpcIndex,
  int NpcType,
  int SpawnTileX,
  int SpawnTileY);

public static class LegacyWorldIsFrozenFinishPolicy
{
  private const int SantaNpcType = 22;
  private const int SantaReplacementNpcType = 142;
  private const int SnowGlobeItemType = 1869;

  public static bool TryCreateChestItemIntent(
    IReadOnlyList<int> stacks,
    LegacyPassRandomState random,
    out LegacyFrozenChestItemIntent intent)
  {
    ArgumentNullException.ThrowIfNull(stacks);
    ArgumentNullException.ThrowIfNull(random);
    if (stacks.Count < 2 || random.Next(2) != 0 || stacks[1] == 0)
    {
      intent = default;
      return false;
    }

    for (int index = 1; index < stacks.Count; index++)
    {
      if (stacks[index] == 0)
      {
        intent = new LegacyFrozenChestItemIntent(index, SnowGlobeItemType);
        return true;
      }
    }

    intent = default;
    return false;
  }

  public static IReadOnlyList<LegacyFrozenNpcReplacementIntent> CreateNpcReplacementIntents(
    IReadOnlyList<int> npcTypes,
    int spawnTileX,
    int spawnTileY,
    bool skyblockWorld,
    bool endlessChristmas)
  {
    ArgumentNullException.ThrowIfNull(npcTypes);
    if (skyblockWorld || !endlessChristmas)
    {
      return Array.Empty<LegacyFrozenNpcReplacementIntent>();
    }

    List<LegacyFrozenNpcReplacementIntent> intents = new();
    for (int index = 0; index < npcTypes.Count; index++)
    {
      if (npcTypes[index] == SantaNpcType)
      {
        intents.Add(new LegacyFrozenNpcReplacementIntent(
          index, SantaReplacementNpcType, spawnTileX, spawnTileY));
      }
    }

    return intents;
  }
}
