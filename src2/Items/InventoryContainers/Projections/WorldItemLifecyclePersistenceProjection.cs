namespace Terraria.Items.InventoryContainers;

public static class WorldItemLifecyclePersistenceProjection
{
  public static WorldItemLifecyclePersistenceSnapshot Create(WorldItemLifecycleComponent lifecycle)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    return new WorldItemLifecyclePersistenceSnapshot(
      schemaVersion: 1,
      lifecycle.OwnTime,
      lifecycle.PlayerIndexTheItemIsReservedFor,
      lifecycle.Instanced,
      lifecycle.OwnIgnore,
      lifecycle.TimeSinceTheItemHasBeenReservedForSomeone,
      lifecycle.TimeLeftInWhichTheItemCannotBeTakenByEnemies,
      lifecycle.TimeSinceSpawned,
      lifecycle.KeepTime);
  }
}
