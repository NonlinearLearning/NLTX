namespace Terraria.Npc;

public readonly record struct NpcSpawnSlotAcquisitionResult(
  NpcSpawnTypeResolutionResult TypeResolution,
  NpcTypeId SlotMetadataType,
  NpcSpawnSlotSelectionResult SlotSelection)
{
  public bool Found => SlotSelection.IsCommitReady;
}
