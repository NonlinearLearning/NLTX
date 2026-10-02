using System;
using Terraria.Npc;

namespace Terraria.Npc.Queries;

public static class NpcSpawnSlotSelectionQuery
{
  /// <summary>
  /// Selects the first unused slot, then the first replaceable slot, in legacy order.
  /// </summary>
  public static NpcSpawnSlotSelectionResult Select(
    ReadOnlySpan<NpcSpawnSlotFact> slotFacts,
    int startIndex,
    bool searchInReverse,
    bool cannotSpawnInSlot0)
  {
    if (slotFacts.IsEmpty)
    {
      return new NpcSpawnSlotSelectionResult(null, UsedReplacementFallback: false);
    }

    if ((uint)startIndex > (uint)slotFacts.Length ||
      (searchInReverse && startIndex == slotFacts.Length))
    {
      throw new ArgumentOutOfRangeException(nameof(startIndex));
    }

    int firstIndex = startIndex;
    if (firstIndex == 0 && cannotSpawnInSlot0)
    {
      firstIndex = 1;
    }

    int endIndex = slotFacts.Length;
    int step = 1;
    if (searchInReverse)
    {
      endIndex--;
      (firstIndex, endIndex) = (endIndex, firstIndex);
      step = -1;
    }

    for (int slotIndex = firstIndex; slotIndex != endIndex; slotIndex += step)
    {
      if (!slotFacts[slotIndex].IsSpawnSlotInUse)
      {
        return new NpcSpawnSlotSelectionResult(
          slotIndex,
          UsedReplacementFallback: false);
      }
    }

    for (int slotIndex = firstIndex; slotIndex != endIndex; slotIndex += step)
    {
      if (slotFacts[slotIndex].CanBeReplacedByOtherNPCs)
      {
        return new NpcSpawnSlotSelectionResult(
          slotIndex,
          UsedReplacementFallback: true,
          ExpectedGeneration: slotFacts[slotIndex].ExpectedGeneration);
      }
    }

    return new NpcSpawnSlotSelectionResult(null, UsedReplacementFallback: false);
  }
}
