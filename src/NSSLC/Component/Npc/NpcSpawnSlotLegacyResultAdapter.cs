using System;

namespace Terraria.Npc;

public static class NpcSpawnSlotLegacyResultAdapter
{
  public static int ToLegacyGetAvailableNpcSlotResult(
    in NpcSpawnSlotSelectionResult selection)
  {
    if (selection.SlotIndex is not int slotIndex)
    {
      return -1;
    }

    if (slotIndex < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(selection));
    }

    return slotIndex;
  }

  public static int ToLegacyNewNpcResult(
    int availableSlotIndex,
    int maxNpcSlots)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(maxNpcSlots);

    if (availableSlotIndex < 0)
    {
      return maxNpcSlots;
    }

    if (availableSlotIndex >= maxNpcSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(availableSlotIndex));
    }

    return availableSlotIndex;
  }
}
