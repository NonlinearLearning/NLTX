namespace Terraria.Npc;

public readonly record struct NpcSpawnGraveyardDualDungeonSelectionResult(
  NpcSpawnGraveyardDualDungeonSelectionStatus Status,
  NpcSpawnEntityRequest? SpawnRequest)
{
  public bool HasRequest =>
    Status == NpcSpawnGraveyardDualDungeonSelectionStatus.RequestProduced && SpawnRequest.HasValue;
}
