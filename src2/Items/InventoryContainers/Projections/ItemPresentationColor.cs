namespace Terraria.Items.InventoryContainers;

public readonly record struct ItemPresentationColor(byte Red, byte Green, byte Blue, byte Alpha)
{
  public static ItemPresentationColor White => new(255, 255, 255, 255);
}
