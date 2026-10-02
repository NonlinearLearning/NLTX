namespace Terraria.Content;

public sealed record NpcSpawnDefinition(int AiStyle, bool CanBeCaught, float SpawnWeight = 1f)
{
  public int? CatchItemTypeId { get; init; }

  public int? SpawnGroup { get; init; }

  public bool SpawnNeedsSyncing { get; init; }
}
