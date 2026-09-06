namespace Terraria.Items;

public sealed class WorldItemComponent
{
  public WorldItemComponent(
    WorldPosition position,
    WorldVector velocity,
    long spawnedAtTick = 0,
    long? despawnAtTick = null,
    bool isInstanced = false,
    bool isBeingGrabbed = false,
    bool isOnConveyor = false,
    long revision = 0)
  {
    Position = position;
    Velocity = velocity;
    SpawnedAtTick = spawnedAtTick;
    DespawnAtTick = despawnAtTick;
    IsInstanced = isInstanced;
    IsBeingGrabbed = isBeingGrabbed;
    IsOnConveyor = isOnConveyor;
    Revision = revision;
  }

  public WorldPosition Position;
  public WorldVector Velocity;
  public long SpawnedAtTick;
  public long? DespawnAtTick;
  public bool IsInstanced;
  public bool IsBeingGrabbed;
  public bool IsOnConveyor;
  public long Revision;

  public bool IsExpiredAt(long currentTick) =>
    DespawnAtTick.HasValue && currentTick >= DespawnAtTick.Value;
}
