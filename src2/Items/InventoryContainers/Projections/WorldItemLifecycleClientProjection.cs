namespace Terraria.Items.InventoryContainers;

public static class WorldItemLifecycleClientProjection
{
  public static WorldItemLifecycleClientSnapshot Create(WorldItemLifecycleComponent lifecycle)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    return new WorldItemLifecycleClientSnapshot(
      schemaVersion: 1,
      lifecycle.PlayerIndexTheItemIsReservedFor,
      lifecycle.Shimmered,
      lifecycle.ShimmerTime,
      lifecycle.BeingGrabbed,
      lifecycle.OnConveyor,
      lifecycle.KeepTime);
  }
}
