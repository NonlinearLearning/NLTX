namespace Terraria.Items.InventoryContainers;

public readonly record struct WorldItemSceneMetricsSnapshot(
  bool IsInScene,
  bool IsOnScreen,
  int ActivePlayerCount)
{
  public bool IsValid => ActivePlayerCount >= 0;
}
