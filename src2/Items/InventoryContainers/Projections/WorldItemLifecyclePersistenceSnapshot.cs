namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemLifecyclePersistenceSnapshot
{
  public WorldItemLifecyclePersistenceSnapshot(
    int schemaVersion,
    int ownTime,
    int reservedPlayerIndex,
    bool instanced,
    int ownIgnore,
    int reservationElapsedTicks,
    int enemyPickupDelay,
    int timeSinceSpawned,
    int keepTime)
  {
    SchemaVersion = schemaVersion;
    OwnTime = ownTime;
    ReservedPlayerIndex = reservedPlayerIndex;
    Instanced = instanced;
    OwnIgnore = ownIgnore;
    ReservationElapsedTicks = reservationElapsedTicks;
    EnemyPickupDelay = enemyPickupDelay;
    TimeSinceSpawned = timeSinceSpawned;
    KeepTime = keepTime;
  }

  public int SchemaVersion { get; }
  public int OwnTime { get; }
  public int ReservedPlayerIndex { get; }
  public bool Instanced { get; }
  public int OwnIgnore { get; }
  public int ReservationElapsedTicks { get; }
  public int EnemyPickupDelay { get; }
  public int TimeSinceSpawned { get; }
  public int KeepTime { get; }
}
