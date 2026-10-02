namespace Terraria.Npc;

public readonly record struct NpcSpawnTowerSelectionResult(
  NpcSpawnTowerSelectionStatus Status,
  NpcSpawnEntityRequest? SpawnRequest)
{
  public bool HasRequest =>
    Status == NpcSpawnTowerSelectionStatus.RequestProduced && SpawnRequest.HasValue;
}
