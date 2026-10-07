namespace Terraria.Npc;

public readonly record struct NpcSpawnCritterSelectionResult(
  NpcSpawnCritterSelectionStatus Status,
  NpcSpawnEntityRequest? SpawnRequest,
  int PostSpawnTimeLeftMultiplier)
{
  public bool HasRequest =>
    Status == NpcSpawnCritterSelectionStatus.RequestProduced && SpawnRequest.HasValue;
}
