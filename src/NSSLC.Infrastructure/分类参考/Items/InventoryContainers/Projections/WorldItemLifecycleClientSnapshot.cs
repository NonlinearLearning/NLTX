namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemLifecycleClientSnapshot
{
  public WorldItemLifecycleClientSnapshot(
    int schemaVersion,
    int reservedPlayerIndex,
    bool shimmered,
    float shimmerTime,
    bool beingGrabbed,
    bool onConveyor,
    int keepTime)
  {
    SchemaVersion = schemaVersion;
    ReservedPlayerIndex = reservedPlayerIndex;
    Shimmered = shimmered;
    ShimmerTime = shimmerTime;
    BeingGrabbed = beingGrabbed;
    OnConveyor = onConveyor;
    KeepTime = keepTime;
  }

  public int SchemaVersion { get; }
  public int ReservedPlayerIndex { get; }
  public bool Shimmered { get; }
  public float ShimmerTime { get; }
  public bool BeingGrabbed { get; }
  public bool OnConveyor { get; }
  public int KeepTime { get; }
}
