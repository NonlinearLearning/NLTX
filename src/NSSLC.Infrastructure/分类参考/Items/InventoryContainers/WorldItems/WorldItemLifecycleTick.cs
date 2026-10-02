namespace Terraria.Items.InventoryContainers;

public readonly record struct WorldItemLifecycleTick(int ElapsedTicks, float DeltaSeconds)
{
  public bool IsValid => ElapsedTicks >= 0 && DeltaSeconds >= 0;
}
