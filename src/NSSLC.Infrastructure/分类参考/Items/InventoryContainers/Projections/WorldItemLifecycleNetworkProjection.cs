namespace Terraria.Items.InventoryContainers;

public static class WorldItemLifecycleNetworkProjection
{
  public static WorldItemLifecycleNetworkSnapshot Create(WorldItemLifecycleComponent lifecycle)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    return new WorldItemLifecycleNetworkSnapshot(
      schemaVersion: 1,
      lifecycle.OwnTime,
      lifecycle.PlayerIndexTheItemIsReservedFor,
      lifecycle.NoGrabDelay,
      lifecycle.Shimmered,
      lifecycle.ShimmerTime,
      lifecycle.Instanced,
      lifecycle.OwnIgnore,
      lifecycle.TimeSinceTheItemHasBeenReservedForSomeone,
      lifecycle.TimeLeftInWhichTheItemCannotBeTakenByEnemies,
      lifecycle.TimeSinceSpawned,
      lifecycle.BeingGrabbed,
      lifecycle.OnConveyor,
      lifecycle.KeepTime);
  }
}
