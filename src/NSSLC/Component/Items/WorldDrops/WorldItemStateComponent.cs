namespace Terraria.Items;

public sealed class WorldItemStateComponent
{
  public WorldItemStateComponent(
    ReplicationId replicationId,
    LootSourceRef? spawnSource = null)
  {
    ReplicationId = replicationId;
    SpawnSource = spawnSource;
  }

  public LootSourceRef? SpawnSource;
  public ReplicationId ReplicationId;
}
