namespace Terraria.Npc;

public readonly record struct NpcSpawnPreCommitResult(
  NpcSpawnEntityPreparationResult EntityPreparation,
  NpcSpawnSlotAcquisitionResult SlotAcquisition)
{
  public NpcSpawnEntityRequest ResolvedRequest =>
    EntityPreparation.PreparedRequest with
    {
      Type = SlotAcquisition.TypeResolution.ResolvedType
    };
}
