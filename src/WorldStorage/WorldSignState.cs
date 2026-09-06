namespace Terraria.WorldStorage;

public sealed class WorldSignState
{
  public SignSlot Slot { get; internal set; }
  public TileCoordinate Anchor { get; internal set; }
  public string Text { get; internal set; } = string.Empty;
}
