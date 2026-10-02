namespace Terraria.Npc;

public readonly record struct NpcSpawnInvasionSelectionResult(
  NpcSpawnInvasionSelectionStatus Status,
  NpcSpawnEntityRequest? SpawnRequest)
{
  public bool HasRequest =>
    Status == NpcSpawnInvasionSelectionStatus.RequestProduced && SpawnRequest.HasValue;

  public bool RequiresEarlyReturn =>
    Status == NpcSpawnInvasionSelectionStatus.UnknownInvasionTypeEarlyReturn;
}
