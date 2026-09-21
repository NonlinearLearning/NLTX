namespace Terraria.Items.InventoryContainers;

public sealed class QuickStackSourcePositionSnapshot
{
  public QuickStackSourcePositionSnapshot(InventoryPosition position)
  {
    Position = position;
  }

  public InventoryPosition Position { get; }
}
