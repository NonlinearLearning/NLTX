using System;
using Terraria.Npc.Queries;

namespace Terraria.Npc;

public static class NpcSpawnSlotSelectionSystem
{
  /// <summary>
  /// Selects from the captured slot facts and protects a slot that is safe to commit.
  /// </summary>
  public static NpcSpawnSlotSelectionResult SelectAndProtect(
    ReadOnlySpan<NpcSpawnSlotFact> slotFacts,
    int startIndex,
    bool searchInReverse,
    bool cannotSpawnInSlot0,
    INpcSpawnSlotProtectionPort protectionPort)
  {
    ArgumentNullException.ThrowIfNull(protectionPort);
    if (protectionPort.SlotCount != slotFacts.Length)
    {
      throw new ArgumentException(
        "Captured slot facts and protection storage must have the same capacity.",
        nameof(slotFacts));
    }

    NpcSpawnSlotSelectionResult selection = NpcSpawnSlotSelectionQuery.Select(
      slotFacts,
      startIndex,
      searchInReverse,
      cannotSpawnInSlot0);
    if (selection.SlotIndex is int slotIndex && selection.IsCommitReady)
    {
      NpcSpawnSlotProtectionSystem.ProtectSelectedSlot(slotIndex, protectionPort);
    }

    return selection;
  }
}
