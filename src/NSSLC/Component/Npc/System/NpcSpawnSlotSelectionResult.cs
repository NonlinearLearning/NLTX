namespace Terraria.Npc;

public readonly record struct NpcSpawnSlotSelectionResult(
  int? SlotIndex,
  bool UsedReplacementFallback,
  uint? ExpectedGeneration = null)
{
  public bool Found => SlotIndex.HasValue;

  public bool IsCommitReady =>
    Found && (!UsedReplacementFallback || ExpectedGeneration is > 0);
}
