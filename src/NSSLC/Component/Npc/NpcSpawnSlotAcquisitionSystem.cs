using System;

namespace Terraria.Npc;

public static class NpcSpawnSlotAcquisitionSystem
{
  public static NpcSpawnSlotAcquisitionResult Acquire(
    NpcTypeId requestedType,
    bool isGoodWorld,
    int startIndex,
    INpcSpawnSlotAcquisitionPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcSpawnTypeResolutionResult typeResolution = NpcSpawnTypeResolutionSystem.Resolve(
      requestedType,
      isGoodWorld,
      port);
    NpcTypeId slotMetadataType = port.FromNetId(typeResolution.ResolvedType);
    bool searchInReverse = port.SearchSpawnSlotsInReverse(slotMetadataType);
    bool cannotSpawnInSlot0 = startIndex == 0 && port.CannotSpawnInSlot0(slotMetadataType);
    NpcSpawnSlotFact[] slotFacts = port.CaptureSlotFacts();
    ArgumentNullException.ThrowIfNull(slotFacts);

    NpcSpawnSlotSelectionResult slotSelection = NpcSpawnSlotSelectionSystem.SelectAndProtect(
      slotFacts,
      startIndex,
      searchInReverse,
      cannotSpawnInSlot0,
      port);
    return new NpcSpawnSlotAcquisitionResult(
      typeResolution,
      slotMetadataType,
      slotSelection);
  }
}
