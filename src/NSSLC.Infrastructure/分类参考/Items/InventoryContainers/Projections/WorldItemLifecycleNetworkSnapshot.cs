namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemLifecycleNetworkSnapshot
{
  public WorldItemLifecycleNetworkSnapshot(
    int schemaVersion,
    int ownTime,
    int reservedPlayerIndex,
    int noGrabDelay,
    bool shimmered,
    float shimmerTime,
    bool instanced,
    int ownIgnore,
    int reservationElapsedTicks,
    int enemyPickupDelay,
    int timeSinceSpawned,
    bool beingGrabbed,
    bool onConveyor,
    int keepTime)
  {
    SchemaVersion = schemaVersion;
    OwnTime = ownTime;
    ReservedPlayerIndex = reservedPlayerIndex;
    NoGrabDelay = noGrabDelay;
    Shimmered = shimmered;
    ShimmerTime = shimmerTime;
    Instanced = instanced;
    OwnIgnore = ownIgnore;
    ReservationElapsedTicks = reservationElapsedTicks;
    EnemyPickupDelay = enemyPickupDelay;
    TimeSinceSpawned = timeSinceSpawned;
    BeingGrabbed = beingGrabbed;
    OnConveyor = onConveyor;
    KeepTime = keepTime;
  }

  public int SchemaVersion { get; }
  public int OwnTime { get; }
  public int ReservedPlayerIndex { get; }
  public int NoGrabDelay { get; }
  public bool Shimmered { get; }
  public float ShimmerTime { get; }
  public bool Instanced { get; }
  public int OwnIgnore { get; }
  public int ReservationElapsedTicks { get; }
  public int EnemyPickupDelay { get; }
  public int TimeSinceSpawned { get; }
  public bool BeingGrabbed { get; }
  public bool OnConveyor { get; }
  public int KeepTime { get; }
}
