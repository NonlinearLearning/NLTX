using System;

namespace Terraria.Npc;

public static class NpcSpawnPreCommitSystem
{
  public static NpcSpawnPreCommitResult Prepare(
    in NpcSpawnEntityRequest request,
    bool isAnniversaryWorld,
    bool isGoodWorld,
    in NpcSpawnTargetSelectionSnapshot targetSnapshot,
    INpcSpawnPreCommitPort port)
  {
    ArgumentNullException.ThrowIfNull(port);

    NpcSpawnEntityPreparationResult entityPreparation =
      NpcSpawnEntityPreparationSystem.Prepare(
        in request,
        isAnniversaryWorld,
        in targetSnapshot,
        port);
    NpcSpawnSlotAcquisitionResult slotAcquisition = NpcSpawnSlotAcquisitionSystem.Acquire(
      entityPreparation.PreparedRequest.Type,
      isGoodWorld,
      entityPreparation.PreparedRequest.StartIndex,
      port);
    return new NpcSpawnPreCommitResult(entityPreparation, slotAcquisition);
  }
}
