namespace Terraria.Items.InventoryContainers;

public sealed class WorldItemLifecycleSystem
{
  public WorldItemSceneMetricsSnapshot? Update(
    WorldItemLifecycleComponent lifecycle,
    WorldItemLifecycleTick tick,
    IWorldItemSceneMetricsPort? sceneMetrics = null)
  {
    ArgumentNullException.ThrowIfNull(lifecycle);
    lifecycle.Advance(tick);
    return sceneMetrics?.Read();
  }
}
