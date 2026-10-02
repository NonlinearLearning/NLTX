namespace Terraria.Npc;

public readonly record struct NpcSpawnBranchSelectionResult(
  NpcSpawnBranchSelectionStatus Status,
  NpcSpawnBranchKind? SelectedBranch,
  NpcSpawnEntityRequest? SpawnRequest,
  int PostSpawnTimeLeftMultiplier)
{
  public bool HasRequest =>
    Status == NpcSpawnBranchSelectionStatus.RequestProduced && SpawnRequest.HasValue;

  public bool RequiresLegacyContinuation =>
    Status is NpcSpawnBranchSelectionStatus.EarlierUnmodeledBranchesUnresolved or
      NpcSpawnBranchSelectionStatus.NoModeledBranchMatched;
}
