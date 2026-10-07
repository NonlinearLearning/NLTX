namespace Terraria.Npc;

public readonly record struct NpcSpawnSkyMobSelectionResult(
  NpcSpawnSkyMobSelectionStatus Status,
  NpcSpawnEntityRequest? SpawnRequest)
{
  public bool HasRequest =>
    Status == NpcSpawnSkyMobSelectionStatus.RequestProduced && SpawnRequest.HasValue;
}
