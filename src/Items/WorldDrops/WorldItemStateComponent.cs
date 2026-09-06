namespace Terraria.Items;

public sealed class WorldItemStateComponent
{
  public WorldItemStateComponent(
    RuntimeEntityId itemEntity,
    WorldPosition worldPosition,
    long spawnedAtTick,
    bool active,
    ReplicationId replicationId,
    LootSourceRef? spawnSource = null,
    long? despawnAtTick = null,
    bool isInstanced = false,
    bool isBeingGrabbed = false,
    bool isOnConveyor = false,
    long revision = 0)
  {
    ItemEntity = itemEntity;
    WorldPosition = worldPosition;
    SpawnedAtTick = spawnedAtTick;
    Active = active;
    ReplicationId = replicationId;
    SpawnSource = spawnSource;
    DespawnAtTick = despawnAtTick;
    IsInstanced = isInstanced;
    IsBeingGrabbed = isBeingGrabbed;
    IsOnConveyor = isOnConveyor;
    Revision = revision;
  }

  public RuntimeEntityId ItemEntity;
  public LootSourceRef? SpawnSource;
  public bool Active;
  public ReplicationId ReplicationId;
  public WorldPosition WorldPosition;
  public long SpawnedAtTick;
  public long? DespawnAtTick;
  public bool IsInstanced;
  public bool IsBeingGrabbed;
  public bool IsOnConveyor;
  public long Revision;

  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue &&
    currentTick >= DespawnAtTick.Value;
}
